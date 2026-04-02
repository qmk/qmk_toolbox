using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.ViewModels;

public partial class KeyViewModel(PhysicalKey key, string label, double x, double y, double w, double h = 40) : ObservableObject
{
    public PhysicalKey Key { get; } = key;
    public string Label { get; } = label;
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Width { get; } = w;
    public double Height { get; } = h;

    [ObservableProperty] private KeyState _state;

    private static readonly IBrush DefaultKeyBrush = new SolidColorBrush(Color.Parse("#3A3A3A"));

    public IBrush Background => State switch
    {
        KeyState.Default => DefaultKeyBrush,
        KeyState.Pressed => Brushes.Yellow,
        KeyState.Tested => Brushes.Lime,
        _ => throw new ArgumentOutOfRangeException(),
    };

    public IBrush Foreground => State == KeyState.Default ? Brushes.White : Brushes.Black;

    partial void OnStateChanged(KeyState value)
    {
        OnPropertyChanged(nameof(Background));
        OnPropertyChanged(nameof(Foreground));
    }
}
