using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.ViewModels;

public partial class HidConsoleViewModel : LogViewModelBase, IDisposable
{
    private const string AllDevices = "(All connected devices)";

    [ObservableProperty] private string _selectedDevice = AllDevices;

    public ObservableCollection<string> Devices { get; } = [AllDevices];

    private readonly IHidListener _hidListener;
    private readonly Dictionary<string, string> _devicePaths = []; // display name → path

    private Func<Func<Task>, Task>? _invokeOnUiThread;
    private Func<string, Task>? _setClipboardText;

    public void SetUiInvoker(Func<Func<Task>, Task> invoker) => _invokeOnUiThread = invoker;
    public void SetClipboardFunc(Func<string, Task> func) => _setClipboardText = func;

    public HidConsoleViewModel(IHidListener hidListener)
    {
        _hidListener = hidListener;
        _hidListener.HidDeviceConnected += OnDeviceConnected;
        _hidListener.HidDeviceDisconnected += OnDeviceDisconnected;
        _hidListener.ConsoleReportReceived += OnConsoleReportReceived;
        _hidListener.ErrorOccurred += OnErrorOccurred;
    }

    public void Start() => _hidListener.Start();

    private void OnDeviceConnected(IHidDevice device)
    {
        if (!device.IsConsoleDevice)
            return;
        string label = device.ToString() ?? string.Empty;
        Invoke(() =>
        {
            _devicePaths[label] = device.DevicePath;
            if (!Devices.Contains(label))
                Devices.Add(label);
            LogLine($"HID console device connected: {device}", MessageType.Hid);
        });
    }

    private void OnDeviceDisconnected(IHidDevice device)
    {
        if (!device.IsConsoleDevice)
            return;
        string label = device.ToString() ?? string.Empty;
        Invoke(() =>
        {
            Devices.Remove(label);
            _devicePaths.Remove(label);
            LogLine($"HID console device disconnected: {device}", MessageType.Hid);
            if (SelectedDevice == label)
                SelectedDevice = AllDevices;
        });
    }

    private void OnConsoleReportReceived(IHidDevice device, string data)
    {
        string label = device.ToString() ?? string.Empty;
        if (SelectedDevice != AllDevices && SelectedDevice != label)
            return;
        Invoke(() => Log(data, MessageType.HidOutput));
    }

    private void Invoke(Action action)
    {
        if (_invokeOnUiThread != null)
            _ = _invokeOnUiThread(() => { action(); return Task.CompletedTask; });
        else
            action();
    }

    [RelayCommand]
    private void Clear() => Buffer.Clear();

    [RelayCommand]
    private async Task CopyAll()
    {
        if (_setClipboardText == null)
            return;
        await _setClipboardText(Buffer.ToString());
    }

    private void OnErrorOccurred(string message) =>
        Invoke(() => LogLine(message, MessageType.Error));

    public void Dispose() => _hidListener.Dispose();
}
