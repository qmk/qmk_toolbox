using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Services;

namespace QmkToolbox.Desktop.ViewModels;

/// <summary>
/// A device-selector entry: keyed by <paramref name="DevicePath"/> so two identical keyboards
/// (same VID/PID/strings) stay distinct; <paramref name="Label"/> is display-only.
/// <see langword="null"/> path = the "all devices" entry.
/// </summary>
public sealed record HidDeviceEntry(string? DevicePath, string Label)
{
    public override string ToString() => Label;
}

public partial class HidConsoleViewModel : LogViewModelBase, IDisposable
{
    private static readonly HidDeviceEntry AllDevices = new(null, "(All connected devices)");

    [ObservableProperty] private HidDeviceEntry? _selectedDevice = AllDevices;

    public ObservableCollection<HidDeviceEntry> Devices { get; } = [AllDevices];

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
        var entry = new HidDeviceEntry(device.DevicePath, device.ToString() ?? string.Empty);
        Invoke(() =>
        {
            if (!Devices.Any(d => d.DevicePath == device.DevicePath))
                Devices.Add(entry);
            Log($"HID console device connected: {device}", MessageType.Hid);
        });
    }

    private void OnDeviceDisconnected(IHidDevice device)
    {
        if (!device.IsConsoleDevice)
            return;
        Invoke(() =>
        {
            HidDeviceEntry? entry = Devices.FirstOrDefault(d => d.DevicePath == device.DevicePath);
            if (entry != null)
                Devices.Remove(entry);
            Log($"HID console device disconnected: {device}", MessageType.Hid);
            if (SelectedDevice?.DevicePath == device.DevicePath)
                SelectedDevice = AllDevices;
        });
    }

    private void OnConsoleReportReceived(IHidDevice device, string data)
    {
        string? selectedPath = SelectedDevice?.DevicePath;
        if (selectedPath != null && selectedPath != device.DevicePath)
            return;
        Invoke(() => Log(data, MessageType.HidOutput));
    }

    private void OnErrorOccurred(string message) =>
        Invoke(() => Log(message, MessageType.Error));

    public void Dispose() => _hidListener.Dispose();
}
