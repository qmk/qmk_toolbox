using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using QmkToolbox.Desktop.Services;
using AvaloniaTheme = Avalonia.Styling.ThemeVariant;

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
    [ObservableProperty] private string _themeVariant = "Default";

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

    public void SetWindowService(IWindowService service)
    {
        _windowService = service;
        _usbDetector.DiagnosticTrace = msg => Invoke(() => service.TraceDebug(msg));
        _flashOrchestrator.DiagnosticTrace = msg => Invoke(() => service.TraceDebug(msg));
    }

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

    partial void OnThemeVariantChanged(string value)
    {
        Application.Current!.RequestedThemeVariant = value switch
        {
            "Light" => AvaloniaTheme.Light,
            "Default" => AvaloniaTheme.Default,
            _ => AvaloniaTheme.Dark,
        };
        OnPropertyChanged(nameof(IsDarkTheme));
        OnPropertyChanged(nameof(IsLightTheme));
        OnPropertyChanged(nameof(IsSystemTheme));
    }

    public bool IsDarkTheme => ThemeVariant == "Dark";
    public bool IsLightTheme => ThemeVariant == "Light";
    public bool IsSystemTheme => ThemeVariant == "Default";

    [RelayCommand]
    private void SetTheme(string variant) => ThemeVariant = variant;

    private readonly IFlashToolProvider _toolProvider;
    private readonly IUsbEventsDetector _usbDetector;
    private readonly FlashOrchestrator _flashOrchestrator;

    private Func<Func<Task>, Task>? _invokeOnUiThread;

    public MainWindowViewModel(
        IFlashToolProvider toolProvider,
        IUsbEventsDetector usbDetector,
        ISerialPortService serialPortService,
        IMountPointService mountPointService,
        ISettingsService settingsService,
        string filePath = "")
    {
        _toolProvider = toolProvider;
        _usbDetector = usbDetector;
        Settings = settingsService;

        _flashOrchestrator = new FlashOrchestrator(toolProvider, serialPortService, mountPointService);
        _flashOrchestrator.OutputReceived += (msg, type) => Invoke(() => LogLine(msg, type));
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
        Settings.Current.ThemeVariant = ThemeVariant;
        Settings.Save();
    }

    private void LoadSettings()
    {
        AppSettings settings = Settings.Current;
        FirmwarePath = settings.FirmwareFilePath;
        SelectedMcu = settings.SelectedMcu;
        ShowAllDevices = settings.ShowAllDevices;
        AutoFlashEnabled = settings.AutoFlashEnabled;
        ThemeVariant = settings.ThemeVariant;

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
        string dirty = ThisAssembly.Git.IsDirty ? "-dirty" : "";
        string gitRev = string.IsNullOrEmpty(ThisAssembly.Git.Tag)
            ? ThisAssembly.Git.Commit + dirty
            : ThisAssembly.Git.Tag + dirty;
        string buildDate = ThisAssembly.Git.CommitDate[..10];
        LogInfo($"QMK Toolbox {version} ({gitRev}, {buildDate}) (https://qmk.fm/toolbox)");
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
                if (_busy)
                {
                    LogInfo("Auto-flash: an operation is already in progress, skipping");
                    return;
                }
                if (string.IsNullOrEmpty(FirmwarePath))
                {
                    LogError("Auto-flash: no firmware file selected");
                    return;
                }
                if (!File.Exists(FirmwarePath))
                {
                    LogError("Auto-flash: firmware file does not exist");
                    return;
                }
                SetBusy(true);
                try
                {
                    await _flashOrchestrator.FlashAllAsync(SelectedMcu, FirmwarePath);
                }
                catch (Exception ex)
                {
                    LogError($"Auto-flash failed: {ex.Message}");
                }
                finally
                {
                    SetBusy(false);
                }
            }
        });

    private void OnDeviceDisconnected(IUsbDevice device)
        => Invoke(() => _flashOrchestrator.OnDeviceDisconnected(device, ShowAllDevices));

    // True while a flash / reset / EEPROM operation is running. Gates every action command
    // so a second tool (e.g. "Exit DFU" mid-flash) can't be launched against the same device.
    // Folded into UpdateCanExecute so USB connect/disconnect events during an operation can't
    // re-enable the buttons.
    private bool _busy;

    private void SetBusy(bool value)
    {
        if (_busy == value)
            return;
        _busy = value;
        UpdateCanExecute();
    }

    private void UpdateCanExecute()
    {
        bool flash = _flashOrchestrator.HasBootloaders && !_busy;
        bool reset = _flashOrchestrator.HasResettable && !_busy;
        bool eeprom = _flashOrchestrator.HasEepromFlashable && !_busy;
        if (_windowService != null && (flash != CanFlash || reset != CanReset))
        {
            _windowService.TraceDebug(
                $"[STATE] CanFlash:{CanFlash}->{flash}  CanReset:{CanReset}->{reset}" +
                $"  (bootloaders:{_flashOrchestrator.BootloaderCount})");
        }
        CanFlash = flash;
        CanReset = reset;
        CanClearEeprom = eeprom;
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
        SetBusy(true);
        try
        {
            await _flashOrchestrator.FlashAllAsync(SelectedMcu, FirmwarePath);
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private async Task Reset()
    {
        SetBusy(true);
        try
        {
            await _flashOrchestrator.ResetAllAsync(SelectedMcu);
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task ClearEeprom() =>
        RunEepromAsync("reset.eep", "Attempting to clear EEPROM, please don't remove device", "EEPROM clear complete");

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task SetLeftHand() =>
        RunEepromAsync("reset_left.eep", "Attempting to set handedness, please don't remove device", "EEPROM write complete");

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task SetRightHand() =>
        RunEepromAsync("reset_right.eep", "Attempting to set handedness, please don't remove device", "EEPROM write complete");

    private async Task RunEepromAsync(string eepFile, string startMessage, string completeMessage)
    {
        SetBusy(true);
        try
        {
            await _flashOrchestrator.FlashEepromAsync(SelectedMcu, _toolProvider.GetToolPath(eepFile), startMessage, completeMessage);
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    // ClearAndReExtract is a synchronous blocking method; Task.Run wraps it so this
    // RelayCommand returns an awaitable Task without blocking the UI thread.
    private Task ClearResources() =>
        Task.Run(_toolProvider.ClearAndReExtract);

    [RelayCommand]
    private void Exit()
    {
        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime lt)
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
    private void OpenDebugLog() => _windowService?.ShowDebugLog();

    [RelayCommand]
    private void InstallDrivers() => WindowsDriversInstaller.Install(_toolProvider, LogError);

    [RelayCommand]
    private async Task InstallUdevRules() =>
        await LinuxUdevInstaller.InstallAsync(
            _toolProvider,
            msg => Invoke(() => LogLine(msg, MessageType.UdevOutput)),
            msg => Invoke(() => LogLine(msg, MessageType.Error)));

    [RelayCommand]
    private async Task CopyLog()
    {
        if (_windowService == null)
            return;
        await _windowService.SetClipboardTextAsync(Buffer.ToString());
    }

    [RelayCommand]
    private void ClearLog() => Buffer.Clear();

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

    // Raw terminal write: text goes straight to the buffer, which interprets '\r'/'\n'
    // exactly like a terminal. It invents no line breaks — Log("#") three times renders
    // "###", not three lines.
    public void Log(string message, MessageType type)
    {
        Buffer.Write(message, type);
        Trim();
    }

    // A producer emitting discrete, line-oriented output whose terminator has been stripped
    // (tool stdout/stderr, status messages). A trailing '\r' means "overwrite the current
    // line" (progress bars, e.g. "aaa\rbb" → "bba"); anything else is a completed line.
    private void LogLine(string message, MessageType type)
        => Log(message.EndsWith('\r') ? message : message + '\n', type);

    public void LogBootloader(string message) => LogLine(message, MessageType.Bootloader);
    public void LogCommand(string message) => LogLine(message, MessageType.Command);
    public void LogError(string message) => LogLine(message, MessageType.Error);
    public void LogInfo(string message) => LogLine(message, MessageType.Info);
    public void LogUsb(string message) => LogLine(message, MessageType.Usb);
}
