using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using QmkToolbox.Desktop.ViewModels;
using QmkToolbox.Desktop.Views;

namespace QmkToolbox.Desktop.Services;

public sealed class DesktopWindowService : IWindowService
{
    private readonly Window _owner;
    private HidConsoleWindow? _hidConsoleWindow;
    private KeyTesterWindow? _keyTesterWindow;

    public DesktopWindowService(Window owner)
    {
        _owner = owner;
        owner.Closed += (_, _) =>
        {
            _hidConsoleWindow?.Close();
            _keyTesterWindow?.Close();
        };
    }

    public async Task<string?> PickFirmwareFileAsync()
    {
        IReadOnlyList<IStorageFile> files = await _owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Firmware File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Firmware Files") { Patterns = ["*.hex", "*.bin", "*.uf2"] },
                new FilePickerFileType("All Files") { Patterns = ["*"] }
            ]
        });
        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task SetClipboardTextAsync(string text)
    {
        if (TopLevel.GetTopLevel(_owner)?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(text);
    }

    public void ShowKeyTester()
    {
        if (_keyTesterWindow != null)
        {
            _keyTesterWindow.Activate();
            return;
        }
        _keyTesterWindow = new KeyTesterWindow { DataContext = new KeyTesterViewModel() };
        _keyTesterWindow.Closed += (_, _) => _keyTesterWindow = null;
        _keyTesterWindow.Show(_owner);
    }

    public void ShowHidConsole()
    {
        if (_hidConsoleWindow != null)
        {
            _hidConsoleWindow.Activate();
            return;
        }
        // HidApiListener calls Hid.Init() on Start() and Hid.Exit() on Dispose().
        // Its lifecycle is scoped to the console window — created here and disposed
        // when the window closes (via HidConsoleWindow.OnClosed → HidConsoleViewModel.Dispose).
        _hidConsoleWindow = new HidConsoleWindow { DataContext = new HidConsoleViewModel(new HidApiListener()) };
        _hidConsoleWindow.Closed += (_, _) => _hidConsoleWindow = null;
        _hidConsoleWindow.Show(_owner);
    }

    public void ShowAbout()
    {
        var win = new AboutWindow();
        win.ShowDialog(_owner);
    }
}
