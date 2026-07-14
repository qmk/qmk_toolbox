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

    // Raw terminal write: text goes straight to the buffer, which interprets '\r'/'\n'
    // exactly like a terminal. It invents no line breaks — Log("#") three times renders
    // "###", not three lines.
    public void Log(string text, MessageType type)
    {
        Buffer.Write(text, type);
        Trim();
    }

    // A discrete, line-oriented message (status, errors, command echo): starts at
    // column 0 — breaking a partial raw-stream line if one is pending — and ends the line.
    protected void LogLine(string message, MessageType type)
        => Log(Buffer.Col > 0 ? "\n" + message + "\n" : message + "\n", type);

    [RelayCommand]
    private void Clear() => Buffer.Clear();

    [RelayCommand]
    private async Task CopyAll()
    {
        if (_setClipboardText != null)
            await _setClipboardText(Buffer.ToString());
    }
}
