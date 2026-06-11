using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using QmkToolbox.Desktop.Models;
using QmkToolbox.Desktop.ViewModels;

namespace QmkToolbox.Desktop.Views;

public partial class KeyTesterWindow : Window
{
    private static readonly IBrush DarkKeyBrush = new SolidColorBrush(Color.Parse("#3A3A3A"));
    private static readonly IBrush LightKeyBrush = new SolidColorBrush(Color.Parse("#D8D8D8"));
    private static readonly IBrush KeyBorderBrush = new SolidColorBrush(Color.Parse("#808080"));

    private readonly List<(Border Border, TextBlock Label, KeyViewModel Vm)> _keyControls = [];

    public KeyTesterWindow()
    {
        InitializeComponent();
        ActualThemeVariantChanged += (_, _) => RefreshKeyColors();
    }

    private bool IsDark => ActualThemeVariant == ThemeVariant.Dark;

    private IBrush GetKeyBackground(KeyState state) => state switch
    {
        KeyState.Default => IsDark ? DarkKeyBrush : LightKeyBrush,
        KeyState.Pressed => Brushes.Yellow,
        KeyState.Tested => Brushes.Lime,
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
    };

    private IBrush GetKeyForeground(KeyState state) =>
        state == KeyState.Default && IsDark ? Brushes.White : Brushes.Black;

    private void RefreshKeyColors()
    {
        foreach ((Border? border, TextBlock? label, KeyViewModel? vm) in _keyControls)
        {
            border.Background = GetKeyBackground(vm.State);
            label.Foreground = GetKeyForeground(vm.State);
        }
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
        foreach (KeyViewModel keyVm in vm.Keys)
        {
            var textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 10,
                TextWrapping = TextWrapping.Wrap,
                Text = keyVm.Label,
                Foreground = GetKeyForeground(keyVm.State),
            };

            var border = new Border
            {
                Width = keyVm.Width,
                Height = keyVm.Height,
                BorderBrush = KeyBorderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Background = GetKeyBackground(keyVm.State),
                Child = textBlock,
            };

            keyVm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(KeyViewModel.State))
                {
                    border.Background = GetKeyBackground(keyVm.State);
                    textBlock.Foreground = GetKeyForeground(keyVm.State);
                }
            };

            _keyControls.Add((border, textBlock, keyVm));
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
