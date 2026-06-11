using Avalonia.Input;
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
}
