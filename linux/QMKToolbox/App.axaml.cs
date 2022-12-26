using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QMK_Toolbox.ViewModels;
using QMK_Toolbox.Views;

namespace QMK_Toolbox;

public class App : Application
{
    // ReSharper disable once MemberCanBePrivate.Global
    public MainWindowViewModel MainWindowViewModel { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            MainWindowViewModel = new MainWindowViewModel(Program.Arg, (IWindow)desktop.MainWindow);
            desktop.MainWindow.DataContext = MainWindowViewModel;
        }

        base.OnFrameworkInitializationCompleted();
    }
}