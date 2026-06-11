using Avalonia.Controls;
using Avalonia.Input.Platform;
using QmkToolbox.Desktop.ViewModels;

namespace QmkToolbox.Desktop.Views;

public partial class DebugLogWindow : Window
{
    public DebugLogWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is DebugLogViewModel vm)
        {
            TopLevel? top = GetTopLevel(this);
            if (top?.Clipboard is { } clipboard)
                vm.SetClipboardFunc(clipboard.SetTextAsync);
        }
    }
}
