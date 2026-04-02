using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using QmkToolbox.Desktop.Services;
using QmkToolbox.Desktop.ViewModels;

namespace QmkToolbox.Desktop.Views;

/// <summary>Main application window — hosts firmware selection, flashing controls, and the log panel.</summary>
public partial class MainWindow : Window
{
    private NativeMenuItem? _autoFlashItem;
    private NativeMenuItem? _showAllItem;
    private MainWindowViewModel? _nativeMenuVm;

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is not MainWindowViewModel vm)
            return;

        AppSettings settings = vm.Settings.Current;
        if (settings.WindowWidth.HasValue && settings.WindowHeight.HasValue)
        {
            Width = settings.WindowWidth.Value;
            Height = settings.WindowHeight.Value;
        }
        if (settings.WindowX.HasValue && settings.WindowY.HasValue)
        {
            var pos = new PixelPoint((int)settings.WindowX.Value, (int)settings.WindowY.Value);
            if (Screens.All.Any(s => s.WorkingArea.Contains(pos)))
                Position = pos;
        }

        BuildNativeMenu(vm);

        // SetUiInvoker MUST be called before StartListeners — USB events fire on
        // background threads and rely on the invoker to marshal back to the UI thread.
        vm.SetUiInvoker(Avalonia.Threading.Dispatcher.UIThread.InvokeAsync);
        vm.SetWindowService(new DesktopWindowService(this));
        vm.StartListeners();
        await vm.RunFirstStartSetupAsync();
    }

    // NativeMenu.Menu on a Window in AXAML doesn't inherit DataContext, so all {Binding}
    // commands resolve to null and the items are disabled. Build the menu programmatically
    // with direct references to the live ViewModel instead.
    private void BuildNativeMenu(MainWindowViewModel vm)
    {
        var fileMenu = new NativeMenu
        {
            new NativeMenuItem("Open...")
            {
                Command = vm.OpenFileCommand,
                Gesture = new KeyGesture(Key.O, KeyModifiers.Meta)
            }
        };

        var eepromMenu = new NativeMenu
        {
            new NativeMenuItem("Clear EEPROM") { Command = vm.ClearEepromCommand },
            new NativeMenuItem("Set Left Hand") { Command = vm.SetLeftHandCommand },
            new NativeMenuItem("Set Right Hand") { Command = vm.SetRightHandCommand }
        };

        var autoFlashItem = new NativeMenuItem("Auto-Flash")
        {
            Command = vm.ToggleAutoFlashCommand,
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = vm.AutoFlashEnabled
        };

        var showAllItem = new NativeMenuItem("Show All Devices")
        {
            Command = vm.ToggleShowAllDevicesCommand,
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = vm.ShowAllDevices
        };

        vm.PropertyChanged += OnVmPropertyChanged;
        _autoFlashItem = autoFlashItem;
        _showAllItem = showAllItem;
        _nativeMenuVm = vm;

        var toolsMenu = new NativeMenu
        {
            new NativeMenuItem("Flash") { Command = vm.FlashCommand },
            new NativeMenuItem("Exit DFU") { Command = vm.ResetCommand },
            new NativeMenuItem("EEPROM") { Menu = eepromMenu },
            new NativeMenuItemSeparator(),
            autoFlashItem,
            showAllItem,
            new NativeMenuItemSeparator(),
            new NativeMenuItem("Key Tester") { Command = vm.OpenKeyTesterCommand },
            new NativeMenuItem("HID Console") { Command = vm.OpenHidConsoleCommand },
            new NativeMenuItemSeparator(),
            new NativeMenuItem("Clear Resources") { Command = vm.ClearResourcesCommand }
        };

        var windowMenu = new NativeMenu
        {
            new NativeMenuItem("File") { Menu = fileMenu },
            new NativeMenuItem("Tools") { Menu = toolsMenu }
        };

        NativeMenu.SetMenu(this, windowMenu);
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(MainWindowViewModel.AutoFlashEnabled))
            _autoFlashItem!.IsChecked = _nativeMenuVm!.AutoFlashEnabled;
        else if (args.PropertyName == nameof(MainWindowViewModel.ShowAllDevices))
            _showAllItem!.IsChecked = _nativeMenuVm!.ShowAllDevices;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _nativeMenuVm?.PropertyChanged -= OnVmPropertyChanged;
        if (DataContext is MainWindowViewModel vm)
        {
            // Save window bounds before vm.SaveSettings() serialises the whole settings object
            AppSettings s = vm.Settings.Current;
            s.WindowX = Position.X;
            s.WindowY = Position.Y;
            s.WindowWidth = Width;
            s.WindowHeight = Height;

            vm.SaveSettings();
            vm.StopListeners();
        }
        base.OnClosing(e);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }
        string? path = e.DataTransfer.TryGetFile()?.TryGetLocalPath();
        e.DragEffects = path != null &&
            (path.EndsWith(".hex", StringComparison.OrdinalIgnoreCase) ||
             path.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) ||
             path.EndsWith(".uf2", StringComparison.OrdinalIgnoreCase))
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;
        if (!e.DataTransfer.Contains(DataFormat.File))
            return;

        IStorageItem? file = e.DataTransfer.TryGetFile();
        if (file == null)
            return;

        string? path = file.TryGetLocalPath();
        if (path == null)
            return;

        if (path.EndsWith(".hex", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".uf2", StringComparison.OrdinalIgnoreCase))
        {
            vm.SetFirmwarePath(path);
        }
    }
}
