using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.ViewModels;

public abstract partial class LogViewModelBase : ObservableObject
{
    public TerminalBuffer Buffer { get; } = new();

    private const int MaxLogLines = 10_000;

    private Func<string, Task>? _setClipboardText;

    protected Func<Func<Task>, Task>? UiInvoker { get; private set; }

    public void SetUiInvoker(Func<Func<Task>, Task> invoker) => UiInvoker = invoker;
    public void SetClipboardFunc(Func<string, Task> func) => _setClipboardText = func;

    protected Task InvokeAsync(Func<Task> action) => UiInvoker?.Invoke(action) ?? action();

    protected void Invoke(Action action) => _ = InvokeAsync(() => { action(); return Task.CompletedTask; });

    protected void Trim() => Buffer.TrimToMax(MaxLogLines);

    // Writes to the log, routing on the message type's stream discipline (see
    // MessageType.IsRawStream):
    //  - Raw types (tool stdout/stderr, HID console) go straight to the buffer, which interprets
    //    '\r'/'\n' like a terminal and invents no line breaks — Log("#") three times renders "###".
    //  - Line types (status, errors, command echo) are discrete: they start at column 0 — breaking
    //    a partial raw-stream line if one is pending — and end the line.
    public void Log(string text, MessageType type)
    {
        if (type.IsRawStream())
            Buffer.Write(text, type);
        else
            Buffer.Write(Buffer.Col > 0 ? "\n" + text + "\n" : text + "\n", type);
        Trim();
    }

    [RelayCommand]
    private void Clear() => Buffer.Clear();

    [RelayCommand]
    private async Task CopyAll()
    {
        if (_setClipboardText != null)
            await _setClipboardText(Buffer.ToString());
    }
}
