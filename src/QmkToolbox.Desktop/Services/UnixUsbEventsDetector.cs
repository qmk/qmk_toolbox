#if !WINDOWS
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using Usb.Events;

namespace QmkToolbox.Desktop.Services;

// Used on Linux and macOS only. Usb.Events is not used on Windows — see WindowsUsbEventsDetector.

/// <summary>
/// <see cref="IUsbEventsDetector"/> implementation for Linux and macOS, backed by Usb.Events.
/// Translates platform-specific USB device events into normalised <see cref="UsbDeviceInfo"/> instances.
/// </summary>
public class UnixUsbEventsDetector : IUsbEventsDetector
{
    private readonly List<IUsbDevice> _devices = [];
    private readonly Lock _devicesLock = new();
    private UsbEventWatcher? _watcher;

    public Action<string>? DiagnosticTrace { get; set; }

    public event Action<IUsbDevice>? DeviceConnected;
    public event Action<IUsbDevice>? DeviceDisconnected;

    public void Start()
    {
        _watcher = new UsbEventWatcher();
        _watcher.UsbDeviceAdded += OnAdded;
        _watcher.UsbDeviceRemoved += OnRemoved;
    }

    public void Stop()
    {
        if (_watcher == null)
            return;
        _watcher.UsbDeviceAdded -= OnAdded;
        _watcher.UsbDeviceRemoved -= OnRemoved;
        _watcher.Dispose();
        _watcher = null;
    }

    public void Dispose() => Stop();

    private void OnAdded(object? sender, UsbDevice usbDevice)
    {
        UsbDeviceInfo? device = ToUsbDeviceInfo(usbDevice);
        if (device == null)
            return;
        lock (_devicesLock)
            _devices.Add(device);
        DiagnosticTrace?.Invoke(
            $"[USB+] VID:{device.VendorId:X4} PID:{device.ProductId:X4} " +
            $"Path:{(string.IsNullOrEmpty(device.DevicePath) ? "(empty)" : $"\"{device.DevicePath}\"")}");
        DeviceConnected?.Invoke(device);
    }

    private void OnRemoved(object? sender, UsbDevice usbDevice)
    {
        IUsbDevice? existing = null;
        bool matchedByPath = false;
        int vidPidCandidates = 0;
        string path = usbDevice.DeviceSystemPath;
        UsbDeviceInfo? fallbackDevice = ToUsbDeviceInfo(usbDevice);

        lock (_devicesLock)
        {
            // Removal events often lack VID/PID on Linux/macOS — match by path first.
            if (!string.IsNullOrEmpty(path))
            {
                existing = _devices.Find(d =>
                    d is UsbDeviceInfo info &&
                    !string.IsNullOrEmpty(info.DevicePath) &&
                    info.DevicePath == path);
                if (existing != null)
                    matchedByPath = true;
            }

            if (existing == null && fallbackDevice != null)
            {
                if (DiagnosticTrace != null)
                {
                    vidPidCandidates = _devices.FindAll(d =>
                        d.VendorId == fallbackDevice.VendorId && d.ProductId == fallbackDevice.ProductId).Count;
                }
                existing = _devices.Find(d =>
                    d.VendorId == fallbackDevice.VendorId && d.ProductId == fallbackDevice.ProductId);
            }

            if (existing != null)
                _devices.Remove(existing);
        }

        if (DiagnosticTrace != null)
        {
            string eventPath = string.IsNullOrEmpty(path) ? "(empty)" : $"\"{path}\"";
            string eventVidPid = fallbackDevice != null
                ? $"VID:{fallbackDevice.VendorId:X4} PID:{fallbackDevice.ProductId:X4}"
                : "VID:(empty) PID:(empty)";
            DiagnosticTrace($"[USB-] event path:{eventPath} {eventVidPid}");
            if (existing != null)
            {
                DiagnosticTrace(matchedByPath
                    ? $"[USB-] matched by path  (VID:{existing.VendorId:X4} PID:{existing.ProductId:X4})"
                    : $"[USB-] matched by VID/PID ({vidPidCandidates} candidate(s))  (VID:{existing.VendorId:X4} PID:{existing.ProductId:X4})");
            }
            else
            {
                DiagnosticTrace("[USB-] no match -> disconnect dropped");
            }
        }

        if (existing != null)
            DeviceDisconnected?.Invoke(existing);
    }

    private static UsbDeviceInfo? ToUsbDeviceInfo(UsbDevice d)
    {
        string devicePath = d.DeviceSystemPath ?? "";

        UsbDeviceParser.TryParseUsbId(d.VendorID, out ushort vid);
        UsbDeviceParser.TryParseUsbId(d.ProductID, out ushort pid);

        if (vid == 0 && pid == 0)
        {
            if (!UsbDeviceParser.TryParseHwId(devicePath, out vid, out pid, out _))
                return null;
        }

        return new UsbDeviceInfo(
            vid, pid, 0,
            d.Vendor ?? "",
            d.Product ?? d.DeviceName ?? "",
            "",
            devicePath);
    }
}
#endif
