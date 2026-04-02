using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using QmkToolbox.Desktop.Services;
using QmkToolbox.Desktop.ViewModels;
using QmkToolbox.Desktop.Views;

namespace QmkToolbox.Desktop;

/// <summary>Avalonia application entry point — creates the main window, wires commands, and builds the native app menu.</summary>
public partial class App : Application
{
    // Retained for AppAbout_OnClick — the Click handler wired in App.axaml.
    // The AXAML-declared NativeMenu.Menu is loaded during Initialize() and is the menu
    // item macOS actually makes clickable. Do NOT remove this field or AppAbout_OnClick
    // even if a static analyser reports them as "unread" — the AXAML Click binding is
    // the only reference and is invisible to Roslyn's read-detection.
    private MainWindowViewModel? _mainWindowViewModel;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string[] args = desktop.Args ?? [];
            string filePath = args.Length > 0 ? args[0] : "";
            var vm = new MainWindowViewModel(
                new FlashToolProvider(),
                new UsbEventsDetector(),
                new DesktopSerialPortService(),
                new DesktopMountPointService(),
                new SettingsService(),
                filePath);
            _mainWindowViewModel = vm;
            desktop.MainWindow = new MainWindow { DataContext = vm };

            // Builds the native app menu for non-macOS platforms (Windows, Linux).
            // On macOS the NSMenuBar is built from the AXAML-declared NativeMenu.Menu
            // (loaded during Initialize()) — NativeMenu.SetMenu() called here arrives
            // too late to affect it. The macOS About handler is AppAbout_OnClick below.
            // On macOS the NSMenuBar reads NativeMenu.Menu from the Application during
            // Initialize(), before this method runs, so SetMenu() below has no effect on
            // the macOS app menu — the AXAML-declared NativeMenu.Menu is what appears
            // there. Skip the About item on macOS to avoid a misleading dead entry;
            // the functional handler is AppAbout_OnClick, wired in App.axaml.
            var appMenu = new NativeMenu();
            if (!OperatingSystem.IsMacOS())
            {
                appMenu.Add(new NativeMenuItem("About QMK Toolbox") { Command = vm.OpenAboutCommand });
                appMenu.Add(new NativeMenuItemSeparator());
            }
            appMenu.Add(new NativeMenuItem("Quit QMK Toolbox")
            {
                Command = vm.ExitCommand,
                Gesture = new KeyGesture(Key.Q, KeyModifiers.Meta)
            });
            var appRootMenu = new NativeMenu
            {
                new NativeMenuItem("QMK Toolbox") { Menu = appMenu }
            };
            NativeMenu.SetMenu(this, appRootMenu);
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Handler for the "About QMK Toolbox" NativeMenuItem declared in App.axaml.
    // On macOS, the AXAML-declared NativeMenu.Menu is what the NSMenuBar uses for the
    // app menu — the programmatic NativeMenu.SetMenu call above does not replace it.
    // This Click handler is therefore the actual code path for About on macOS.
    // Do NOT remove this method; it looks unreferenced to Roslyn but is called by Avalonia
    // via the AXAML Click="AppAbout_OnClick" binding at runtime.
    private void AppAbout_OnClick(object? sender, EventArgs args) => _mainWindowViewModel?.OpenAboutCommand.Execute(null);
}
