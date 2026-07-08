using CommunityToolkit.Mvvm.ComponentModel;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.ViewModels;

public abstract partial class LogViewModelBase : ObservableObject
{
    public TerminalBuffer Buffer { get; } = new();

    private const int MaxLogLines = 10_000;

    protected void Trim() => Buffer.TrimToMax(MaxLogLines);
}
