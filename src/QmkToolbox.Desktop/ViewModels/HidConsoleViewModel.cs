using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Services;

namespace QmkToolbox.Desktop.ViewModels;

public partial class HidConsoleViewModel : LogViewModelBase, IDisposable
{
    private const string AllDevices = "(All connected devices)";

    [ObservableProperty] private string _selectedDevice = AllDevices;

    public ObservableCollection<string> Devices { get; } = [AllDevices];

    private readonly IHidListener _hidListener;

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
            if (!Devices.Contains(label))
                Devices.Add(label);
            Log($"HID console device connected: {device}", MessageType.Hid);
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
            Log($"HID console device disconnected: {device}", MessageType.Hid);
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

    private void OnErrorOccurred(string message) =>
        Invoke(() => Log(message, MessageType.Error));

    public void Dispose() => _hidListener.Dispose();
}
