using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace QmkToolbox.Desktop.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.1";
        VersionText.Text = $"Version {version}";
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
