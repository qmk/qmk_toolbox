using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using QmkToolbox.Desktop.ViewModels;

namespace QmkToolbox.Desktop.Views;

public partial class HidConsoleWindow : Window
{
    public HidConsoleWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is HidConsoleViewModel vm)
        {
            vm.SetUiInvoker(Dispatcher.UIThread.InvokeAsync);
            TopLevel? top = GetTopLevel(this);
            if (top?.Clipboard is { } clipboard)
                vm.SetClipboardFunc(clipboard.SetTextAsync);
            vm.Start();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is HidConsoleViewModel vm)
            vm.Dispose();
        base.OnClosed(e);
    }
}
