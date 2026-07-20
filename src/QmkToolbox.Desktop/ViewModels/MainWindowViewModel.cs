using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using QmkToolbox.Desktop.Services;
using AvaloniaTheme = Avalonia.Styling.ThemeVariant;

namespace QmkToolbox.Desktop.ViewModels;

/// <summary>
/// Thin adapter binding Avalonia to the <see cref="FlashSession"/>: commands, theme switching,
/// the confirm-dialog protocol, and startup logging. Flash-domain state and policy live on the
/// session; XAML binds to it via <see cref="Session"/>.
/// </summary>
public partial class MainWindowViewModel : LogViewModelBase
{
    [ObservableProperty] private string _themeVariant = "Default";

    [ObservableProperty] private bool _isConfirmVisible;
    [ObservableProperty] private string _confirmTitle = "";
    [ObservableProperty] private string _confirmMessage = "";
    private TaskCompletionSource<bool>? _confirmTcs;

    public bool IsWindows { get; } = OperatingSystem.IsWindows();
    public bool IsLinux { get; } = OperatingSystem.IsLinux();

    public FlashSession Session { get; }
    public SettingsService Settings { get; }

    private readonly IFlashToolProvider _toolProvider;
    private DesktopWindowService? _windowService;

    public MainWindowViewModel(
        FlashSession session,
        IFlashToolProvider toolProvider,
        SettingsService settingsService,
        string filePath = "")
    {
        Session = session;
        _toolProvider = toolProvider;
        Settings = settingsService;
        Settings.ErrorLogger = LogError;

        // Log routes each message by its type's stream discipline (see MessageType.IsRawStream).
        Session.Output += Log;
        Session.PropertyChanged += OnSessionPropertyChanged;

        ThemeVariant = Settings.Current.ThemeVariant;
        Session.LoadFrom(Settings.Current);
        LogStartupBanner();

        if (!string.IsNullOrEmpty(filePath))
            Session.SetFirmwarePath(filePath);
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(FlashSession.CanFlash):
                FlashCommand.NotifyCanExecuteChanged();
                break;
            case nameof(FlashSession.CanReset):
                ResetCommand.NotifyCanExecuteChanged();
                break;
            case nameof(FlashSession.CanClearEeprom):
                ClearEepromCommand.NotifyCanExecuteChanged();
                SetLeftHandCommand.NotifyCanExecuteChanged();
                SetRightHandCommand.NotifyCanExecuteChanged();
                break;
            case nameof(FlashSession.CanClearResources):
                ClearResourcesCommand.NotifyCanExecuteChanged();
                break;
        }
    }

    public void SetWindowService(DesktopWindowService service)
    {
        _windowService = service;
        Session.DiagnosticTrace = service.TraceDebug;
    }

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

    public void SaveSettings()
    {
        Settings.Current.ThemeVariant = ThemeVariant;
        Session.SaveTo(Settings.Current);
        Settings.Save();
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

    private bool CanFlash => Session.CanFlash;
    private bool CanReset => Session.CanReset;
    private bool CanClearEeprom => Session.CanClearEeprom;
    private bool CanClearResources => Session.CanClearResources;

    [RelayCommand(CanExecute = nameof(CanFlash))]
    private Task Flash() => Session.FlashAsync();

    [RelayCommand(CanExecute = nameof(CanReset))]
    private Task Reset() => Session.ResetAsync();

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task ClearEeprom() => Session.ClearEepromAsync();

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task SetLeftHand() => Session.SetHandednessAsync(left: true);

    [RelayCommand(CanExecute = nameof(CanClearEeprom))]
    private Task SetRightHand() => Session.SetHandednessAsync(left: false);

    [RelayCommand(CanExecute = nameof(CanClearResources))]
    private Task ClearResources() => Session.ClearResourcesAsync();

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
            Session.SetFirmwarePath(path);
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
            msg => Invoke(() => Log(msg, MessageType.UdevOutput)),
            msg => Invoke(() => Log(msg, MessageType.Error)));

    [RelayCommand]
    private void ToggleAutoFlash() => Session.AutoFlashEnabled = !Session.AutoFlashEnabled;

    [RelayCommand]
    private void ToggleShowAllDevices() => Session.ShowAllDevices = !Session.ShowAllDevices;

    public void LogError(string message) => Log(message, MessageType.Error);
    public void LogInfo(string message) => Log(message, MessageType.Info);
}
