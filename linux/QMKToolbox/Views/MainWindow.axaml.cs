using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MessageBox.Avalonia;

namespace QMK_Toolbox.Views;

public partial class MainWindow : Window, IWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnInitialized(object sender, EventArgs e)
    {
        Task.Delay(50).ContinueWith( t => Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                var vm = ((App)App.Current).MainWindowViewModel;
                vm.InitUi();
            } ) );
    }
    
    public void ShowMessage(string message)
    {
        var messageBoxStandardWindow = MessageBoxManager
            .GetMessageBoxStandardWindow("QMK Toolbox - File Type Error",
                message);
        messageBoxStandardWindow.Show();
    }
}