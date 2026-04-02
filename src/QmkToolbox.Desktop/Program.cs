using Avalonia;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome;

namespace QmkToolbox.Desktop;

/// <summary>Application entry point.</summary>
internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        IconProvider.Current.Register<FontAwesomeIconProvider>();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
