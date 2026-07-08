using CommunityToolkit.Mvvm.Input;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.ViewModels;

public partial class DebugLogViewModel : LogViewModelBase
{
    private Func<string, Task>? _setClipboardText;

    public void SetClipboardFunc(Func<string, Task> func) => _setClipboardText = func;

    public void Append(string message)
    {
        Buffer.Write($"{DateTime.Now:HH:mm:ss.fff}  {message}\n", MessageType.Debug);
        Trim();
    }

    [RelayCommand]
    private void Clear() => Buffer.Clear();

    [RelayCommand]
    private async Task CopyAll()
    {
        if (_setClipboardText == null)
            return;
        await _setClipboardText(Buffer.ToString());
    }
}
