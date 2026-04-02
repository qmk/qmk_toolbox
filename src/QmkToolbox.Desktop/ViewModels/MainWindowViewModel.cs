using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using QmkToolbox.Desktop.Converters;
using QmkToolbox.Desktop.Models;
using QmkToolbox.Desktop.Services;

namespace QmkToolbox.Desktop.ViewModels;

public partial class MainWindowViewModel : LogViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FlashCommand))]
    private bool _canFlash;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    private bool _canReset;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearEepromCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetLeftHandCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetRightHandCommand))]
    private bool _canClearEeprom;

    [ObservableProperty] private string _firmwarePath = "";
    [ObservableProperty] private string _selectedMcu = "";
    [ObservableProperty] private bool _autoFlashEnabled;
    [ObservableProperty] private bool _showAllDevices;

    [ObservableProperty] private bool _isConfirmVisible;
    [ObservableProperty] private string _confirmTitle = "";
    [ObservableProperty] private string _confirmMessage = "";
    private TaskCompletionSource<bool>? _confirmTcs;

    public ObservableCollection<string> FirmwareHistory { get; } = [];
    public ObservableCollection<McuItem> McuList { get; } = [];

    public bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public bool IsLinux { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public ISettingsService Settings { get; }

    private IWindowService? _windowService;

    public void SetWindowService(IWindowService service) => _windowService = service;

    public McuItem? SelectedMcuPair
    {
        get => McuList.FirstOrDefault(m => m.Key == SelectedMcu) ?? McuList.FirstOrDefault();
        set
        {
            if (value is not null)
                SelectedMcu = value.Key;
            OnPropertyChanged();
        }
    }

    partial void OnSelectedMcuChanged(string value) => OnPropertyChanged(nameof(SelectedMcuPair));

    private readonly IFlashToolProvider _toolProvider;
    private readonly IUsbDetector _usbDetector;
    private readonly FlashOrchestrator _flashOrchestrator;

    private Func<Func<Task>, Task>? _invokeOnUiThread;

    public MainWindowViewModel(
        IFlashToolProvider toolProvider,
        IUsbDetector usbDetector,
        ISerialPortService serialPortService,
        IMountPointService mountPointService,
        ISettingsService settingsService,
        string filePath = "")
    {
        _toolProvider = toolProvider;
        _usbDetector = usbDetector;
        Settings = settingsService;

        _flashOrchestrator = new FlashOrchestrator(toolProvider, serialPortService, mountPointService);
        _flashOrchestrator.OutputReceived += (msg, type) => Invoke(() => Log(msg, type));
        _flashOrchestrator.StateChanged += () => Invoke(UpdateCanExecute);

        _usbDetector.DeviceConnected += OnDeviceConnected;
        _usbDetector.DeviceDisconnected += OnDeviceDisconnected;

        LoadSettings();
        LoadMcuList();
        LogStartupBanner();

        if (!string.IsNullOrEmpty(filePath))
            SetFirmwarePath(filePath);
    }

    public void SetUiInvoker(Func<Func<Task>, Task> invoker) => _invokeOnUiThread = invoker;

    public void StartListeners()
    {
        if (_invokeOnUiThread is null)
            throw new InvalidOperationException("SetUiInvoker must be called before StartListeners.");
        // EnsureResourceFolder is blocking file I/O (resource extraction); offload to
        // a thread pool thread so StartListeners returns without blocking the UI thread.
        _ = Task.Run(() =>
        {
            try
            { _toolProvider.EnsureResourceFolder(); }
            catch (Exception ex) { Invoke(() => LogError($"Failed to extract resources: {ex.Message}")); }
        });
        try
        { _usbDetector.Start(); }
        catch (Exception ex) { LogError($"USB device enumeration failed: {ex.Message}"); }
    }

    public async Task RunFirstStartSetupAsync()
    {
        if (!Settings.Current.FirstStart)
            return;

        if (OperatingSystem.IsWindows())
        {
            if (await ShowConfirmAsync("Windows Driver Installation", "Would you like to install Windows drivers for QMK-supported bootloaders?"))
                InstallDrivers();
        }
        else if (OperatingSystem.IsLinux())
        {
            if (await ShowConfirmAsync("Linux udev Rules", "Would you like to install Linux udev rules for QMK-supported bootloaders and HID devices?"))
                await InstallUdevRules();
        }

        Settings.Current.FirstStart = false;
        Settings.Save();
    }

    private Task<bool> ShowConfirmAsync(string title, string message)
    {
        _confirmTcs?.TrySetResult(false);
        ConfirmTitle = title;
        ConfirmMessage = message;
        IsConfirmVisible = true;
        _confirmTcs = new TaskCompletionSource<bool>();
        return _confirmTcs.Task;
    }

    [RelayCommand]
    private void ConfirmYes() => CompleteConfirm(true);

    [RelayCommand]
    private void ConfirmNo() => CompleteConfirm(false);

    private void CompleteConfirm(bool result)
    {
        IsConfirmVisible = false;
        _confirmTcs?.TrySetResult(result);
        _confirmTcs = null;
    }

    public void StopListeners()
    {
        _usbDetector.Stop();
        _usbDetector.Dispose();
    }

    public void SaveSettings()
    {
        Settings.Current.FirmwareFilePath = FirmwarePath;
        Settings.Current.FirmwareFileHistory = [.. FirmwareHistory];
        Settings.Current.SelectedMcu = SelectedMcu;
        Settings.Current.ShowAllDevices = ShowAllDevices;
        Settings.Current.AutoFlashEnabled = AutoFlashEnabled;
        Settings.Save();
    }

    private void LoadSettings()
    {
        AppSettings settings = Settings.Current;
        FirmwarePath = settings.FirmwareFilePath;
        SelectedMcu = settings.SelectedMcu;
        ShowAllDevices = settings.ShowAllDevices;
        AutoFlashEnabled = settings.AutoFlashEnabled;

        foreach (string item in settings.FirmwareFileHistory)
            FirmwareHistory.Add(item);
    }

    private void LoadMcuList()
    {
        try
        {
            using Stream? stream = typeof(MainWindowViewModel).Assembly
                .GetManifestResourceStream("QmkToolbox.Desktop.Resources.mcu-list.txt");
            if (stream == null)
                return;
            using var reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
            foreach (string line in content.Split('\n'))
            {
                string[] parts = line.Trim().Split(':', 2);
                if (parts.Length == 2)
                    McuList.Add(new McuItem(parts[0], parts[1]));
            }
            if (string.IsNullOrEmpty(SelectedMcu) && McuList.Count > 0)
                SelectedMcu = McuList[0].Key;
        }
        catch (Exception ex)
        {
            LogError($"Failed to load MCU list: {ex.Message}");
        }
    }

    private void LogStartupBanner()
    {
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.1";
        LogInfo($"QMK Toolbox {version} (https://qmk.fm/toolbox)");
        try
        {
            (string? flashUtils, string? hidApi, string? udevRules) = _toolProvider.GetManifestInfo();
            string manifestInfo = $"Flash utils: {flashUtils}, hidapi: {hidApi}";
            if (udevRules != null)
                manifestInfo += $", qmk_udev: {udevRules}";
            LogInfo(manifestInfo);
        }
        catch (Exception ex) { Debug.WriteLine($"Failed to read release manifests: {ex.Message}"); }

        LogInfo("Supported bootloaders:");
        LogInfo(" - ARM DFU (APM32, AT32, Kiibohd, STM32, STM32duino) and RISC-V DFU (GD32V) via dfu-util (http://dfu-util.sourceforge.net/)");
        LogInfo(" - Atmel SAM-BA (Massdrop) via Massdrop Loader (https://github.com/massdrop/mdloader)");
        LogInfo(" - Atmel/LUFA/QMK DFU via dfu-programmer (http://dfu-programmer.github.io/)");
        LogInfo(" - BootloadHID (Atmel, PS2AVRGB) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)");
        LogInfo(" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)");
        LogInfo(" - HalfKay (Teensy, Ergodox EZ) via Teensy Loader (https://pjrc.com/teensy/loader_cli.html)");
        LogInfo(" - LUFA Mass Storage");
        LogInfo(" - LUFA/QMK HID via hid_bootloader_cli (https://github.com/abcminiuser/lufa)");
        LogInfo(" - Raspberry Pi RP2040/RP2350 (BOOTSEL) via picotool (https://github.com/raspberrypi/picotool)");
        LogInfo(" - WB32 DFU via wb32-dfu-updater_cli (https://github.com/WestberryTech/wb32-dfu-updater)");
        LogInfo("Supported ISP flashers:");
        LogInfo(" - AVRISP (Arduino ISP)");
        LogInfo(" - USBasp (AVR ISP)");
        LogInfo(" - USBTiny (AVR Pocket)");
    }

    private void OnDeviceConnected(IUsbDevice device)
        => _ = InvokeAsync(async () =>
        {
            bool bootloaderAdded = _flashOrchestrator.OnDeviceConnected(device, ShowAllDevices);
            if (bootloaderAdded && AutoFlashEnabled)
            {
                try
                {
                    await _flashOrchestrator.FlashAllAsync(SelectedMcu, FirmwarePath);
                }
                catch (Exception ex)
                {
                    LogError($"Auto-flash failed: {ex.Message}");
                }
            }
        });

    private void OnDeviceDisconnected(IUsbDevice device)
        => Invoke(() => _flashOrchestrator.OnDeviceDisconnected(device, ShowAllDevices));

    private void UpdateCanExecute()
    {
        CanFlash = _flashOrchestrator.HasBootloaders;
        CanReset = _flashOrchestrator.HasResettable;
        CanClearEeprom = _flashOrchestrator.HasEepromFlashable;
    }

    private void Invoke(Action action) => _ = InvokeAsync(() => { action(); return Task.CompletedTask; });

    private Task InvokeAsync(Func<Task> action) =>
        _invokeOnUiThread?.Invoke(action) ?? action();

    [RelayCommand(CanExecute = nameof(CanFlash))]
    private async Task Flash()
    {
        if (string.IsNullOrEmpty(FirmwarePath))
        {
            LogError("Please select a file");
            return;
        }
        if (!File.Exists(FirmwarePath))
        {
            LogError("File does not exist!");
            return;
        }
        CanFlash = false;
        await _flashOrchestrator.FlashAllAsync(SelectedMcu, FirmwarePath);
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private Task Reset() => _flashOrchestrator.ResetAllAsync(SelectedMcu);

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task ClearEeprom() =>
        _flashOrchestrator.FlashEepromAsync(SelectedMcu, _toolProvider.GetToolPath("reset.eep"), "Attempting to clear EEPROM, please don't remove device", "EEPROM clear complete");

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task SetLeftHand() =>
        _flashOrchestrator.FlashEepromAsync(SelectedMcu, _toolProvider.GetToolPath("reset_left.eep"), "Attempting to set handedness, please don't remove device", "EEPROM write complete");

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task SetRightHand() =>
        _flashOrchestrator.FlashEepromAsync(SelectedMcu, _toolProvider.GetToolPath("reset_right.eep"), "Attempting to set handedness, please don't remove device", "EEPROM write complete");

    [RelayCommand]
    // ClearAndReExtract is a synchronous blocking method; Task.Run wraps it so this
    // RelayCommand returns an awaitable Task without blocking the UI thread.
    private Task ClearResources() =>
        Task.Run(_toolProvider.ClearAndReExtract);

    [RelayCommand]
    private void Exit()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime lt)
            lt.Shutdown();
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        if (_windowService == null)
            return;
        string? path = await _windowService.PickFirmwareFileAsync();
        if (path != null)
            SetFirmwarePath(path);
    }

    [RelayCommand]
    private void OpenKeyTester() => _windowService?.ShowKeyTester();

    [RelayCommand]
    private void OpenHidConsole() => _windowService?.ShowHidConsole();

    [RelayCommand]
    private void OpenAbout() => _windowService?.ShowAbout();

    [RelayCommand]
    private void InstallDrivers() => WindowsDriversInstaller.Install(_toolProvider, LogError);

    [RelayCommand]
    private async Task InstallUdevRules() =>
        await LinuxUdevInstaller.InstallAsync(
            _toolProvider,
            msg => Invoke(() => Log(msg, MessageType.UdevOutput)),
            msg => Invoke(() => Log(msg, MessageType.Error)));

    [RelayCommand]
    private async Task CopyLog()
    {
        if (_windowService == null)
            return;
        var sb = new StringBuilder();
        foreach (LogEntry entry in LogEntries)
            sb.AppendLine(MessageTypeToPrefixConverter.GetPrefix(entry.Type) + entry.Text);
        await _windowService.SetClipboardTextAsync(sb.ToString());
    }

    [RelayCommand]
    private void ClearLog() { LogEntries.Clear(); _lastLineWasOverwrite = false; }

    [RelayCommand]
    private void ToggleAutoFlash() => AutoFlashEnabled = !AutoFlashEnabled;

    [RelayCommand]
    private void ToggleShowAllDevices() => ShowAllDevices = !ShowAllDevices;

    private const int MaxFirmwareHistory = 10;

    public void SetFirmwarePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        FirmwareHistory.Remove(path);
        FirmwareHistory.Insert(0, path);
        while (FirmwareHistory.Count > MaxFirmwareHistory)
            FirmwareHistory.RemoveAt(FirmwareHistory.Count - 1);
        FirmwarePath = path;
    }

    private bool _lastLineWasOverwrite;

    public void Log(string message, MessageType type)
    {
        bool isOverwrite = message.Length > 0 && message[^1] == '\r';
        if (isOverwrite)
            message = message[..^1];

        if (_lastLineWasOverwrite && LogEntries.Count > 0)
        {
            LogEntries[^1] = new LogEntry(message, type);
        }
        else
        {
            message = message.Replace("\r\n", "\n").Replace('\r', '\n');
            if (message.Length > 1 && message[^1] == '\n')
                message = message[..^1];
            foreach (string line in message.Split('\n'))
                LogEntries.Add(new LogEntry(line, type));
            TrimLogEntries();
        }

        _lastLineWasOverwrite = isOverwrite;
    }

    public void LogBootloader(string message) => Log(message, MessageType.Bootloader);
    public void LogCommand(string message) => Log(message, MessageType.Command);
    public void LogError(string message) => Log(message, MessageType.Error);
    public void LogInfo(string message) => Log(message, MessageType.Info);
    public void LogUsb(string message) => Log(message, MessageType.Usb);
}
