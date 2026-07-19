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
/// </summary>
public class FlashSessionTests
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

    private sealed class Harness
    {
        public readonly FakeUsbDetector Detector = new();
        public readonly IFlashToolProvider ToolProvider;
        public readonly FlashOrchestrator Orchestrator;
        public readonly FlashSession Session;

        // The orchestrator emits "device connected" from a WhenReadyAsync continuation on a
        // thread pool thread, so output collection must be synchronized.
        private readonly List<(string Message, MessageType Type)> _output = [];

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

        public List<(string Message, MessageType Type)> Snapshot()
        {
            lock (_output)
            { return [.. _output]; }
        }

        public bool HasOutput(string fragment) => Snapshot().Any(o => o.Message.Contains(fragment));
    }

    private static IUsbDevice AtmelDfu() => new UsbDeviceInfo(0x03EB, 0x2FEF, 0, "", "", "", "");
    private static IUsbDevice Unknown() => new UsbDeviceInfo(0x1234, 0x5678, 0, "", "", "", "");

    private static string TempFirmwareFile()
    {
        string path = Path.Combine(Path.GetTempPath(), $"flash-session-test-{Guid.NewGuid():N}.hex");
        File.WriteAllText(path, ":00000001FF\n");
        return path;
    }

    // ── readiness flags ───────────────────────────────────────────────────────

    [Fact]
    public void DeviceConnected_Bootloader_EnablesActions()
    {
        var h = new Harness();
        Assert.False(h.Session.CanFlash);

        h.Detector.RaiseConnected(AtmelDfu());

        Assert.True(h.Session.CanFlash);
        Assert.True(h.Session.CanReset);
        Assert.True(h.Session.CanClearEeprom);
        Assert.True(h.Session.CanClearResources);
    }

    [Fact]
    public void DeviceDisconnected_LastBootloader_DisablesActions()
    {
        var h = new Harness();
        h.Detector.RaiseConnected(AtmelDfu());
        Assert.True(h.Session.CanFlash);

        h.Detector.RaiseDisconnected(AtmelDfu());

        Assert.False(h.Session.CanFlash);
        Assert.False(h.Session.CanReset);
        Assert.False(h.Session.CanClearEeprom);
    }

    [Fact]
    public async Task BusyOperation_DisablesAllActions_ThenRestores()
    {
        var h = new Harness();
        h.Detector.RaiseConnected(AtmelDfu());
        var gate = new TaskCompletionSource();

        Task<bool> op = h.Orchestrator.RunExclusiveAsync(() => gate.Task);
        Assert.False(h.Session.CanFlash);
        Assert.False(h.Session.CanReset);
        Assert.False(h.Session.CanClearEeprom);
        Assert.False(h.Session.CanClearResources);

        gate.SetResult();
        await op;
        Assert.True(h.Session.CanFlash);
        Assert.True(h.Session.CanClearResources);
    }

    // ── show-all-devices policy ───────────────────────────────────────────────

    [Fact]
    public void UnknownDevice_ShowAllDisabled_Silent()
    {
        var h = new Harness();
        h.Detector.RaiseConnected(Unknown());

        Assert.False(h.Session.CanFlash);
        Assert.Empty(h.Snapshot());
    }

    [Fact]
    public void UnknownDevice_ShowAllEnabled_Reported()
    {
        var h = new Harness();
        h.Session.ShowAllDevices = true;

        h.Detector.RaiseConnected(Unknown());
        h.Detector.RaiseDisconnected(Unknown());

        Assert.True(h.HasOutput("USB device connected"));
        Assert.True(h.HasOutput("USB device disconnected"));
    }

    // ── auto-flash policy ─────────────────────────────────────────────────────

    [Fact]
    public async Task AutoFlash_EnabledWithValidFirmware_FlashesOnConnect()
    {
        var h = new Harness();
        string firmware = TempFirmwareFile();
        try
        {
            h.Session.AutoFlashEnabled = true;
            h.Session.SetFirmwarePath(firmware);

            h.Detector.RaiseConnected(AtmelDfu());
            Assert.NotNull(h.Session.AutoFlashTask);
            await h.Session.AutoFlashTask!;

            Assert.True(h.HasOutput("Attempting to flash"));
            Assert.True(h.HasOutput("Flash complete"));
            Assert.Contains(h.Snapshot(), o => o.Type == MessageType.Command && o.Message.StartsWith("dfu-programmer"));
        }
        finally
        {
            File.Delete(firmware);
        }
    }

    [Fact]
    public void AutoFlash_Disabled_DoesNotFlash()
    {
        var h = new Harness();
        string firmware = TempFirmwareFile();
        try
        {
            h.Session.SetFirmwarePath(firmware);

            h.Detector.RaiseConnected(AtmelDfu());

            Assert.Null(h.Session.AutoFlashTask);
            Assert.False(h.HasOutput("Attempting to flash"));
        }
        finally
        {
            File.Delete(firmware);
        }
    }

    [Fact]
    public void AutoFlash_NoFirmwareSelected_ReportsError()
    {
        var h = new Harness();
        h.Session.AutoFlashEnabled = true;

        h.Detector.RaiseConnected(AtmelDfu());

        Assert.Null(h.Session.AutoFlashTask);
        Assert.Contains(("Auto-flash: no firmware file selected", MessageType.Error), h.Snapshot());
    }

    [Fact]
    public void AutoFlash_FirmwareFileMissing_ReportsError()
    {
        var h = new Harness();
        h.Session.AutoFlashEnabled = true;
        h.Session.SetFirmwarePath("/nonexistent/firmware.hex");

        h.Detector.RaiseConnected(AtmelDfu());

        Assert.Null(h.Session.AutoFlashTask);
        Assert.Contains(("Auto-flash: firmware file does not exist", MessageType.Error), h.Snapshot());
    }

    [Fact]
    public async Task AutoFlash_WhileBusy_SkipsWithMessage()
    {
        var h = new Harness();
        string firmware = TempFirmwareFile();
        try
        {
            h.Session.AutoFlashEnabled = true;
            h.Session.SetFirmwarePath(firmware);
            var gate = new TaskCompletionSource();
            Task<bool> blocker = h.Orchestrator.RunExclusiveAsync(() => gate.Task);

            h.Detector.RaiseConnected(AtmelDfu());
            Assert.NotNull(h.Session.AutoFlashTask);
            await h.Session.AutoFlashTask!;

            Assert.True(h.HasOutput("Auto-flash: an operation is already in progress, skipping"));
            gate.SetResult();
            await blocker;
        }
        finally
        {
            File.Delete(firmware);
        }
    }

    // ── manual flash validation ───────────────────────────────────────────────

    [Fact]
    public async Task FlashAsync_NoFirmwareSelected_ReportsError()
    {
        var h = new Harness();
        h.Detector.RaiseConnected(AtmelDfu());

        await h.Session.FlashAsync();

        Assert.Contains(("no firmware file selected", MessageType.Error), h.Snapshot());
        Assert.False(h.HasOutput("Attempting to flash"));
    }

    // ── firmware history ──────────────────────────────────────────────────────

    [Fact]
    public void SetFirmwarePath_MovesToFront_Deduplicates_TrimsToTen()
    {
        var h = new Harness();
        for (int i = 0; i < 12; i++)
            h.Session.SetFirmwarePath($"/fw/file{i}.hex");
        h.Session.SetFirmwarePath("/fw/file5.hex");

        Assert.Equal("/fw/file5.hex", h.Session.FirmwarePath);
        Assert.Equal("/fw/file5.hex", h.Session.FirmwareHistory[0]);
        Assert.Equal(10, h.Session.FirmwareHistory.Count);
        Assert.Equal(1, h.Session.FirmwareHistory.Count(p => p == "/fw/file5.hex"));
    }

    [Fact]
    public void SetFirmwarePath_EmptyPath_Ignored()
    {
        var h = new Harness();
        h.Session.SetFirmwarePath("");

        Assert.Equal("", h.Session.FirmwarePath);
        Assert.Empty(h.Session.FirmwareHistory);
    }

    // ── settings slice ────────────────────────────────────────────────────────

    [Fact]
    public void LoadFrom_MapsSettingsAndParsesMcuList()
    {
        var h = new Harness();
        var settings = new AppSettings
        {
            FirmwareFilePath = "/fw/current.hex",
            FirmwareFileHistory = ["/fw/current.hex", "/fw/old.hex"],
            SelectedMcu = "at90usb1286",
            ShowAllDevices = true,
            AutoFlashEnabled = true,
        };

        h.Session.LoadFrom(settings);

        Assert.Equal("/fw/current.hex", h.Session.FirmwarePath);
        Assert.Equal(new[] { "/fw/current.hex", "/fw/old.hex" }, h.Session.FirmwareHistory);
        Assert.Equal("at90usb1286", h.Session.SelectedMcu);
        Assert.True(h.Session.ShowAllDevices);
        Assert.True(h.Session.AutoFlashEnabled);
        Assert.NotEmpty(h.Session.McuList);
    }

    [Fact]
    public void LoadFrom_NoSelectedMcu_DefaultsToFirstListEntry()
    {
        var h = new Harness();
        h.Session.LoadFrom(new AppSettings { SelectedMcu = "" });

        Assert.NotEmpty(h.Session.McuList);
        Assert.Equal(h.Session.McuList[0].Key, h.Session.SelectedMcu);
    }

    [Fact]
    public void SaveTo_RoundTripsTheFlashDomainSlice()
    {
        var h = new Harness();
        h.Session.LoadFrom(new AppSettings());
        h.Session.SetFirmwarePath("/fw/a.hex");
        h.Session.SetFirmwarePath("/fw/b.hex");
        h.Session.SelectedMcu = "atmega32u2";
        h.Session.AutoFlashEnabled = true;

        var saved = new AppSettings();
        h.Session.SaveTo(saved);

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
        var h = new Harness();
        var order = new List<string>();
        h.ToolProvider.When(p => p.EnsureResourceFolder()).Do(_ => order.Add("extract"));
        h.Detector.OnStart = () => order.Add("detector");

        await h.Session.StartAsync();

        Assert.Equal(new[] { "extract", "detector" }, order);
    }

    [Fact]
    public async Task StartAsync_ExtractionFailure_ReportedAndDetectorStillStarts()
    {
        var h = new Harness();
        h.ToolProvider.When(p => p.EnsureResourceFolder()).Do(_ => throw new IOException("disk full"));

        await h.Session.StartAsync();

        Assert.True(h.HasOutput("Failed to extract resources: disk full"));
        Assert.True(h.Detector.Started);
    }

    [Fact]
    public void Stop_StopsAndDisposesDetector()
    {
        var h = new Harness();
        h.Session.Stop();

        Assert.True(h.Detector.Stopped);
        Assert.True(h.Detector.Disposed);
    }
}
