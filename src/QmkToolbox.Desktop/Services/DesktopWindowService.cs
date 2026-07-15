using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using QmkToolbox.Desktop.Models;
using QmkToolbox.Desktop.ViewModels;
using QmkToolbox.Desktop.Views;

namespace QmkToolbox.Desktop.Services;

public sealed class DesktopWindowService
{
    private readonly Window _owner;
    private readonly Dictionary<Type, Window> _singletons = [];

    public DesktopWindowService(Window owner)
    {
        _owner = owner;
        owner.Closed += (_, _) =>
        {
            foreach (Window w in _singletons.Values.ToList())
                w.Close();
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
                new FilePickerFileType("Firmware Files") { Patterns = FirmwareFiles.PickerPatterns },
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

    private void ShowSingleton<T>(Func<T> create) where T : Window
    {
        if (_singletons.TryGetValue(typeof(T), out Window? existing))
        {
            existing.Activate();
            return;
        }
        T window = create();
        _singletons[typeof(T)] = window;
        window.Closed += (_, _) => _singletons.Remove(typeof(T));
        window.Show(_owner);
    }

    public void ShowKeyTester() =>
        ShowSingleton(() => new KeyTesterWindow { DataContext = new KeyTesterViewModel() });

    // HidApiListener calls Hid.Init() on Start() and Hid.Exit() on Dispose().
    // Its lifecycle is scoped to the console window — created here and disposed
    // when the window closes (via HidConsoleWindow.OnClosed → HidConsoleViewModel.Dispose).
    public void ShowHidConsole() =>
        ShowSingleton(() => new HidConsoleWindow { DataContext = new HidConsoleViewModel(new HidApiListener()) });

    public void ShowAbout()
    {
        var win = new AboutWindow();
        win.ShowDialog(_owner);
    }

    public void ShowDebugLog() =>
        ShowSingleton(() => new DebugLogWindow { DataContext = new DebugLogViewModel() });

    /// <summary>Appends a diagnostic trace line to the Debug Log window. No-op when the window is not open.</summary>
    public void TraceDebug(string message)
    {
        if (_singletons.TryGetValue(typeof(DebugLogWindow), out Window? w) && w.DataContext is DebugLogViewModel vm)
            vm.Append(message);
    }
}
