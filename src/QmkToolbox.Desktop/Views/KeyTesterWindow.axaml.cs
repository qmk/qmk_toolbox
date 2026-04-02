using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using QmkToolbox.Desktop.ViewModels;

namespace QmkToolbox.Desktop.Views;

public partial class KeyTesterWindow : Window
{
    public KeyTesterWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is not KeyTesterViewModel vm)
            return;

        PopulateKeyboard(vm);
        Focus();
    }

    private void PopulateKeyboard(KeyTesterViewModel vm)
    {
        var borderBrush = new SolidColorBrush(Color.Parse("#808080"));

        foreach (KeyViewModel keyVm in vm.Keys)
        {
            var textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 10,
                TextWrapping = TextWrapping.Wrap,
                Text = keyVm.Label,
                Foreground = keyVm.Foreground
            };

            var border = new Border
            {
                Width = keyVm.Width,
                Height = keyVm.Height,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Background = keyVm.Background,
                Child = textBlock
            };

            // Update visuals when key state changes — no reflection-based
            // bindings, fully trim-safe.
            keyVm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(KeyViewModel.Background))
                    border.Background = keyVm.Background;
                else if (args.PropertyName == nameof(KeyViewModel.Foreground))
                    textBlock.Foreground = keyVm.Foreground;
            };

            Canvas.SetLeft(border, keyVm.X);
            Canvas.SetTop(border, keyVm.Y);
            KeyCanvas.Children.Add(border);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is KeyTesterViewModel vm)
            vm.OnKeyDown(e.PhysicalKey);
        e.Handled = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (DataContext is KeyTesterViewModel vm)
            vm.OnKeyUp(e.PhysicalKey);
        e.Handled = true;
    }
}
