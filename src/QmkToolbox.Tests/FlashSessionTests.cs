using NSubstitute;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using QmkToolbox.Desktop.Services;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Drives FlashSession through its interface: a fake USB detector raises events, the real
/// FlashOrchestrator runs over mocked tool/serial/mount seams (GetToolPath returns "/bin/true"
/// so child processes exit harmlessly), and an immediate invoker stands in for the UI thread.
/// The per-test harness owns the temp firmware file and deletes it on Dispose.
/// </summary>
public sealed class FlashSessionTests : IDisposable
{
    private sealed class FakeUsbDetector : IUsbEventsDetector
    {
        public event Action<IUsbDevice>? DeviceConnected;
        public event Action<IUsbDevice>? DeviceDisconnected;
        public Action<string>? DiagnosticTrace { get; set; }

        public Action? OnStart;
        public bool Started;
        public bool Stopped;
        public bool Disposed;

        public void Start()
        {
            Started = true;
            OnStart?.Invoke();
        }

        public void Stop() => Stopped = true;
        public void Dispose() => Disposed = true;

        public void RaiseConnected(IUsbDevice device) => DeviceConnected?.Invoke(device);
        public void RaiseDisconnected(IUsbDevice device) => DeviceDisconnected?.Invoke(device);
    }

    private sealed class Harness : IDisposable
    {
        public readonly FakeUsbDetector Detector = new();
        public readonly IFlashToolProvider ToolProvider;
        public readonly FlashOrchestrator Orchestrator;
        public readonly FlashSession Session;

        // The orchestrator emits "device connected" from a WhenReadyAsync continuation on a
        // thread pool thread, so output collection must be synchronized.
        private readonly List<(string Message, MessageType Type)> _output = [];

        private string? _firmwareFile;

        public Harness()
        {
            ToolProvider = Substitute.For<IFlashToolProvider>();
            ToolProvider.GetToolPath(Arg.Any<string>()).Returns("/bin/true");
            ToolProvider.GetResourceFolder().Returns(Path.GetTempPath());
            ISerialPortService serial = Substitute.For<ISerialPortService>();
            serial.FindSerialPort(Arg.Any<IUsbDevice>()).Returns("ttyACM0");
            Orchestrator = new FlashOrchestrator(ToolProvider, serial, Substitute.For<IMountPointService>());
            Session = new FlashSession(f => f(), Detector, Orchestrator, ToolProvider);
            Session.Output += (msg, type) => { lock (_output) { _output.Add((msg, type)); } };
        }

        /// <summary>A real on-disk .hex file, created on first use and deleted on Dispose.</summary>
        public string FirmwareFile
        {
            get
            {
                if (_firmwareFile == null)
                {
                    _firmwareFile = Path.Combine(Path.GetTempPath(), $"flash-session-test-{Guid.NewGuid():N}.hex");
                    File.WriteAllText(_firmwareFile, ":00000001FF\n");
                }
                return _firmwareFile;
            }
        }

        public List<(string Message, MessageType Type)> Snapshot()
        {
            lock (_output)
            { return [.. _output]; }
        }

        public bool HasOutput(string fragment) => Snapshot().Any(o => o.Message.Contains(fragment));

        public void Dispose()
        {
            if (_firmwareFile != null)
                File.Delete(_firmwareFile);
        }
    }

    private readonly Harness _h = new();

    public void Dispose() => _h.Dispose();

    private static IUsbDevice AtmelDfu() => new UsbDeviceInfo(0x03EB, 0x2FEF, 0, "", "", "", "");
    private static IUsbDevice Unknown() => new UsbDeviceInfo(0x1234, 0x5678, 0, "", "", "", "");

    // ── readiness flags ───────────────────────────────────────────────────────

    [Fact]
    public void DeviceConnected_Bootloader_EnablesActions()
    {
        Assert.False(_h.Session.CanFlash);

        _h.Detector.RaiseConnected(AtmelDfu());

        Assert.True(_h.Session.CanFlash);
        Assert.True(_h.Session.CanReset);
        Assert.True(_h.Session.CanClearEeprom);
        Assert.True(_h.Session.CanClearResources);
    }

    [Fact]
    public void DeviceDisconnected_LastBootloader_DisablesActions()
    {
        _h.Detector.RaiseConnected(AtmelDfu());
        Assert.True(_h.Session.CanFlash);

        _h.Detector.RaiseDisconnected(AtmelDfu());

        Assert.False(_h.Session.CanFlash);
        Assert.False(_h.Session.CanReset);
        Assert.False(_h.Session.CanClearEeprom);
    }

    [Fact]
    public async Task BusyOperation_DisablesAllActions_ThenRestores()
    {
        _h.Detector.RaiseConnected(AtmelDfu());
        var gate = new TaskCompletionSource();

        Task<bool> op = _h.Orchestrator.RunExclusiveAsync(() => gate.Task);
        Assert.False(_h.Session.CanFlash);
        Assert.False(_h.Session.CanReset);
        Assert.False(_h.Session.CanClearEeprom);
        Assert.False(_h.Session.CanClearResources);

        gate.SetResult();
        await op;
        Assert.True(_h.Session.CanFlash);
        Assert.True(_h.Session.CanClearResources);
    }

    // ── show-all-devices policy ───────────────────────────────────────────────

    [Fact]
    public void UnknownDevice_ShowAllDisabled_Silent()
    {
        _h.Detector.RaiseConnected(Unknown());

        Assert.False(_h.Session.CanFlash);
        Assert.Empty(_h.Snapshot());
    }

    [Fact]
    public void UnknownDevice_ShowAllEnabled_Reported()
    {
        _h.Session.ShowAllDevices = true;

        _h.Detector.RaiseConnected(Unknown());
        _h.Detector.RaiseDisconnected(Unknown());

        Assert.True(_h.HasOutput("USB device connected"));
        Assert.True(_h.HasOutput("USB device disconnected"));
    }

    // ── auto-flash policy ─────────────────────────────────────────────────────

    [Fact]
    public async Task AutoFlash_EnabledWithValidFirmware_FlashesOnConnect()
    {
        _h.Session.AutoFlashEnabled = true;
        _h.Session.SetFirmwarePath(_h.FirmwareFile);

        _h.Detector.RaiseConnected(AtmelDfu());
        Assert.NotNull(_h.Session.AutoFlashTask);
        await _h.Session.AutoFlashTask!;

        Assert.True(_h.HasOutput("Attempting to flash"));
        Assert.True(_h.HasOutput("Flash complete"));
        Assert.Contains(_h.Snapshot(), o => o.Type == MessageType.Command && o.Message.StartsWith("dfu-programmer"));
    }

    [Fact]
    public void AutoFlash_Disabled_DoesNotFlash()
    {
        _h.Session.SetFirmwarePath(_h.FirmwareFile);

        _h.Detector.RaiseConnected(AtmelDfu());

        Assert.Null(_h.Session.AutoFlashTask);
        Assert.False(_h.HasOutput("Attempting to flash"));
    }

    [Fact]
    public void AutoFlash_NoFirmwareSelected_ReportsError()
    {
        _h.Session.AutoFlashEnabled = true;

        _h.Detector.RaiseConnected(AtmelDfu());

        Assert.Null(_h.Session.AutoFlashTask);
        Assert.Contains(("Auto-flash: no firmware file selected", MessageType.Error), _h.Snapshot());
    }

    [Fact]
    public void AutoFlash_FirmwareFileMissing_ReportsError()
    {
        _h.Session.AutoFlashEnabled = true;
        _h.Session.SetFirmwarePath("/nonexistent/firmware.hex");

        _h.Detector.RaiseConnected(AtmelDfu());

        Assert.Null(_h.Session.AutoFlashTask);
        Assert.Contains(("Auto-flash: firmware file does not exist", MessageType.Error), _h.Snapshot());
    }

    [Fact]
    public async Task AutoFlash_WhileBusy_SkipsWithMessage()
    {
        _h.Session.AutoFlashEnabled = true;
        _h.Session.SetFirmwarePath(_h.FirmwareFile);
        var gate = new TaskCompletionSource();
        Task<bool> blocker = _h.Orchestrator.RunExclusiveAsync(() => gate.Task);

        _h.Detector.RaiseConnected(AtmelDfu());
        Assert.NotNull(_h.Session.AutoFlashTask);
        await _h.Session.AutoFlashTask!;

        Assert.True(_h.HasOutput("Auto-flash: an operation is already in progress, skipping"));
        gate.SetResult();
        await blocker;
    }

    // ── manual flash validation ───────────────────────────────────────────────

    [Fact]
    public async Task FlashAsync_NoFirmwareSelected_ReportsError()
    {
        _h.Detector.RaiseConnected(AtmelDfu());

        await _h.Session.FlashAsync();

        Assert.Contains(("no firmware file selected", MessageType.Error), _h.Snapshot());
        Assert.False(_h.HasOutput("Attempting to flash"));
    }

    // ── firmware history ──────────────────────────────────────────────────────

    [Fact]
    public void SetFirmwarePath_MovesToFront_Deduplicates_TrimsToTen()
    {
        for (int i = 0; i < 12; i++)
            _h.Session.SetFirmwarePath($"/fw/file{i}.hex");
        _h.Session.SetFirmwarePath("/fw/file5.hex");

        Assert.Equal("/fw/file5.hex", _h.Session.FirmwarePath);
        Assert.Equal("/fw/file5.hex", _h.Session.FirmwareHistory[0]);
        Assert.Equal(10, _h.Session.FirmwareHistory.Count);
        Assert.Equal(1, _h.Session.FirmwareHistory.Count(p => p == "/fw/file5.hex"));
    }

    [Fact]
    public void SetFirmwarePath_EmptyPath_Ignored()
    {
        _h.Session.SetFirmwarePath("");

        Assert.Equal("", _h.Session.FirmwarePath);
        Assert.Empty(_h.Session.FirmwareHistory);
    }

    // ── settings slice ────────────────────────────────────────────────────────

    [Fact]
    public void LoadFrom_MapsSettingsAndParsesMcuList()
    {
        var settings = new AppSettings
        {
            FirmwareFilePath = "/fw/current.hex",
            FirmwareFileHistory = ["/fw/current.hex", "/fw/old.hex"],
            SelectedMcu = "at90usb1286",
            ShowAllDevices = true,
            AutoFlashEnabled = true,
        };

        _h.Session.LoadFrom(settings);

        Assert.Equal("/fw/current.hex", _h.Session.FirmwarePath);
        Assert.Equal(new[] { "/fw/current.hex", "/fw/old.hex" }, _h.Session.FirmwareHistory);
        Assert.Equal("at90usb1286", _h.Session.SelectedMcu);
        Assert.True(_h.Session.ShowAllDevices);
        Assert.True(_h.Session.AutoFlashEnabled);
        Assert.NotEmpty(_h.Session.McuList);
    }

    [Fact]
    public void LoadFrom_NoSelectedMcu_DefaultsToFirstListEntry()
    {
        _h.Session.LoadFrom(new AppSettings { SelectedMcu = "" });

        Assert.NotEmpty(_h.Session.McuList);
        Assert.Equal(_h.Session.McuList[0].Key, _h.Session.SelectedMcu);
    }

    [Fact]
    public void SaveTo_RoundTripsTheFlashDomainSlice()
    {
        _h.Session.LoadFrom(new AppSettings());
        _h.Session.SetFirmwarePath("/fw/a.hex");
        _h.Session.SetFirmwarePath("/fw/b.hex");
        _h.Session.SelectedMcu = "atmega32u2";
        _h.Session.AutoFlashEnabled = true;

        var saved = new AppSettings();
        _h.Session.SaveTo(saved);

        Assert.Equal("/fw/b.hex", saved.FirmwareFilePath);
        Assert.Equal(new[] { "/fw/b.hex", "/fw/a.hex" }, saved.FirmwareFileHistory);
        Assert.Equal("atmega32u2", saved.SelectedMcu);
        Assert.True(saved.AutoFlashEnabled);
        Assert.False(saved.ShowAllDevices);
    }

    // ── lifecycle ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartAsync_ExtractsResourcesBeforeStartingDetector()
    {
        var order = new List<string>();
        _h.ToolProvider.When(p => p.EnsureResourceFolder()).Do(_ => order.Add("extract"));
        _h.Detector.OnStart = () => order.Add("detector");

        await _h.Session.StartAsync();

        Assert.Equal(new[] { "extract", "detector" }, order);
    }

    [Fact]
    public async Task StartAsync_ExtractionFailure_ReportedAndDetectorStillStarts()
    {
        _h.ToolProvider.When(p => p.EnsureResourceFolder()).Do(_ => throw new IOException("disk full"));

        await _h.Session.StartAsync();

        Assert.True(_h.HasOutput("Failed to extract resources: disk full"));
        Assert.True(_h.Detector.Started);
    }

    [Fact]
    public void Stop_StopsAndDisposesDetector()
    {
        _h.Session.Stop();

        Assert.True(_h.Detector.Stopped);
        Assert.True(_h.Detector.Disposed);
    }
}
