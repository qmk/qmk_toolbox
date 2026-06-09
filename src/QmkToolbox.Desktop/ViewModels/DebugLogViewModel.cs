using System.Text;
using CommunityToolkit.Mvvm.Input;
using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.ViewModels;

public partial class DebugLogViewModel : LogViewModelBase
{
    private Func<string, Task>? _setClipboardText;

    public void SetClipboardFunc(Func<string, Task> func) => _setClipboardText = func;

    public void Append(string message)
    {
        LogEntries.Add(new LogEntry($"{DateTime.Now:HH:mm:ss.fff}  {message}", MessageType.Debug));
        TrimLogEntries();
    }

    [RelayCommand]
    private void Clear() => LogEntries.Clear();

    [RelayCommand]
    private async Task CopyAll()
    {
        if (_setClipboardText == null)
            return;
        var sb = new StringBuilder();
        foreach (LogEntry entry in LogEntries)
            sb.AppendLine(entry.Text);
        await _setClipboardText(sb.ToString());
    }
}
