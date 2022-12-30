using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MessageBox.Avalonia;

namespace QMK_Toolbox.Views;

public partial class MainWindow : Window, IWindow
{
    private TextBlock _hexTextBlock;
    private Border _border;
    public MainWindow()
    {
        InitializeComponent();
        
        
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragOverEvent, DragOver);
    }

    private void OnInitialized(object sender, EventArgs e)
    {
        Task.Delay(50).ContinueWith( t => Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                var vm = ((App)App.Current).MainWindowViewModel;
                vm.InitUi();
                _hexTextBlock = this.Find<TextBlock>("HexFile");
                _border = this.Find<Border>("border");
                _border.PointerPressed += DoDrag;
            } ) );
    }
    
    public void ShowMessage(string message)
    {
        var messageBoxStandardWindow = MessageBoxManager
            .GetMessageBoxStandardWindow("QMK Toolbox - File Type Error",
                message);
        messageBoxStandardWindow.Show();
    }

    public void OnClose()
    {
        Close();
    }

    public void OnFileOpen()
    {
        var vm = ((App)App.Current).MainWindowViewModel;
        var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        if (isLinux)
        {
            var helper = new OpenDlgHelper();
            var result = helper.GetFileName();
            vm.HexFile = string.IsNullOrEmpty(result) ? vm.Prompt : result;
            return;
        }

        var hexFile = GetPath();
        vm.HexFile = string.IsNullOrEmpty(hexFile) ? vm.Prompt : hexFile;
    }
    
    private string GetPath()
    {
        var dialog = new OpenFileDialog();
        dialog.Filters.Add(new FileDialogFilter() { Name = "Hex", Extensions = { "hex" } });
        dialog.Filters.Add(new FileDialogFilter() { Name = "Bin", Extensions = { "bin" } });
        dialog.AllowMultiple = false;
        var result = dialog.ShowAsync(this).Result;
        return result?.First();
    }

    private async void DoDrag(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        DataObject dragData = new DataObject();
        var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
    }

    private void DragOver(object sender, DragEventArgs e)
    {
        Debug.WriteLine("DragOver");
        // Only allow Copy or Link as Drop Operations.
        e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

        // Only allow if the dragged data contains text or filenames.
        if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.FileNames))
            e.DragEffects = DragDropEffects.None;
    }
    private void Drop(object sender, DragEventArgs e)
    {
        var vm = ((App)App.Current).MainWindowViewModel;
        Debug.WriteLine("Drop");
        if (e.Data.Contains(DataFormats.Text))
           vm.HexFile = e.Data.GetText();
        else if (e.Data.Contains(DataFormats.FileNames))
           vm.HexFile =  e.Data.GetFileNames()?.FirstOrDefault();
    }
}