using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using Usb.Events;
#if WINDOWS
using System.Diagnostics;
using System.Management;
#endif

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// <see cref="IUsbDetector"/> implementation backed by the Usb.Events library.
/// Translates platform-specific USB device events into normalised <see cref="UsbDeviceInfo"/> instances.
/// </summary>
public class UsbEventsDetector : IUsbDetector
{
    private readonly List<IUsbDevice> _devices = [];
    private readonly Lock _devicesLock = new();
    private UsbEventWatcher? _watcher;

    public event Action<IUsbDevice>? DeviceConnected;
    public event Action<IUsbDevice>? DeviceDisconnected;

    public void Start()
    {
        // usePnPEntity: true uses Win32_PnPEntity instead of Win32_USBControllerDevice.
        // Win32_USBControllerDevice only lists devices Windows has fully installed a driver for,
        // so DFU-class devices (STM32, ATmega) without WinUSB/libusb drivers are invisible to it.
        // Win32_PnPEntity covers all PnP devices regardless of driver status.
        _watcher = new UsbEventWatcher(usePnPEntity: true);
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
        DeviceConnected?.Invoke(device);
    }

    private void OnRemoved(object? sender, UsbDevice usbDevice)
    {
        IUsbDevice? existing = null;
        string path = usbDevice.DeviceSystemPath;
        // Build the fallback device info outside the lock to avoid holding it
        // during potentially slow operations (e.g., WMI queries on Windows).
        UsbDeviceInfo? fallbackDevice = ToUsbDeviceInfo(usbDevice);

        lock (_devicesLock)
        {
            // Try matching by DeviceSystemPath first — this is the most reliable
            // identifier since removal events often lack VID/PID on Linux/macOS.
            if (!string.IsNullOrEmpty(path))
            {
                existing = _devices.Find(d =>
                    d is UsbDeviceInfo info &&
                    !string.IsNullOrEmpty(info.DevicePath) &&
                    info.DevicePath == path);
            }

            // Fall back to VID+PID matching if path didn't match
            if (existing == null && fallbackDevice != null)
            {
                existing = _devices.Find(d =>
                    d.VendorId == fallbackDevice.VendorId && d.ProductId == fallbackDevice.ProductId);
            }

            if (existing != null)
                _devices.Remove(existing);
        }

        if (existing != null)
            DeviceDisconnected?.Invoke(existing);
    }

#if WINDOWS
    // Driver service names in priority order for composite device interface selection.
    // When a device exposes multiple interfaces, we surface the one most relevant for flashing.
    private static readonly string[] DriverPriority =
        ["WinUSB", "libusbK", "libusb0", "HidUsb", "usbser", "USBSTOR"];

    /// <summary>
    /// Queries WMI for the driver service bound to <paramref name="deviceId"/>.
    /// For USB composite devices (service = "usbccgp"), queries child interface devices
    /// and returns the highest-priority flashing-relevant service among them.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static string GetWindowsDriverService(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
            return "";
        try
        {
            string escaped = deviceId.Replace("\\", "\\\\").Replace("'", "''");
            string service = "";
            using (var searcher = new ManagementObjectSearcher(
                $"SELECT Service FROM Win32_PnPEntity WHERE DeviceID = '{escaped}'"))
            {
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    service = obj["Service"]?.ToString() ?? "";
                    break;
                }
            }

            // USB composite device driver — surface the best child interface service instead.
            if (string.Equals(service, "usbccgp", StringComparison.OrdinalIgnoreCase))
            {
                service = GetBestCompositeInterfaceService(deviceId);
            }

            return service;
        }
        catch (Exception ex) { Trace.WriteLine($"WMI driver query failed for '{deviceId}': {ex.Message}"); }
        return "";
    }

    /// <summary>
    /// For a composite USB root device, queries all its MI_ interface children and returns
    /// the service from the highest-priority entry in <see cref="DriverPriority"/>.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static string GetBestCompositeInterfaceService(string rootDeviceId)
    {
        // rootDeviceId: "USB\VID_2E8A&PID_0003\5&2D4F03CB&0&2"
        // Interface children: "USB\VID_2E8A&PID_0003&MI_00\7&..." etc.
        string[] parts = rootDeviceId.Split('\\');
        if (parts.Length < 2)
            return "";

        // WQL LIKE: '%' = any chars, '_' = any single char (harmless for VID_/PID_ literals)
        string hwPart = parts[1]; // "VID_2E8A&PID_0003"
        string likePattern = $"USB\\\\{hwPart}&MI_%";
        try
        {
            var services = new List<string>();
            using var searcher = new ManagementObjectSearcher(
                $"SELECT Service FROM Win32_PnPEntity WHERE DeviceID LIKE '{likePattern}'");
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                string svc = obj["Service"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(svc) &&
                    !string.Equals(svc, "usbccgp", StringComparison.OrdinalIgnoreCase))
                {
                    services.Add(svc);
                }
            }

            foreach (string preferred in DriverPriority)
            {
                if (services.Any(s => string.Equals(s, preferred, StringComparison.OrdinalIgnoreCase)))
                {
                    return preferred;
                }
            }

            return services.FirstOrDefault() ?? "";
        }
        catch (Exception ex) { Trace.WriteLine($"WMI composite interface query failed for '{rootDeviceId}': {ex.Message}"); }
        return "";
    }
#endif

    /// <summary>
    /// Converts a <see cref="UsbDevice"/> to a normalised <see cref="UsbDeviceInfo"/>.
    /// Falls back to parsing <see cref="UsbDevice.DeviceSystemPath"/> if VID/PID fields are empty.
    /// On Windows, only root USB devices are processed — composite interface children (MI_xx),
    /// HID children, and other non-USB PnP entities are skipped to prevent duplicate detections.
    /// </summary>
    private static UsbDeviceInfo? ToUsbDeviceInfo(UsbDevice d)
    {
        string devicePath = d.DeviceSystemPath ?? "";

        // On Windows, Win32_PnPEntity fires for every child PnP entity (HID\, USBSTOR\,
        // USB\...\&MI_xx, etc.) in addition to the root USB device. Process only the root
        // USB device to get one detection event per physical device.
        if (!UsbDeviceParser.IsWindowsRootUsbDevice(devicePath))
            return null;

        ushort rev = 0;

        UsbDeviceParser.TryParseUsbId(d.VendorID, out ushort vid);
        UsbDeviceParser.TryParseUsbId(d.ProductID, out ushort pid);

        if (vid == 0 && pid == 0)
        {
            if (!UsbDeviceParser.TryParseHwId(devicePath, out vid, out pid, out rev))
                return null;
        }

        string driver = "";
#if WINDOWS
#pragma warning disable CA1416 // Already gated by #if WINDOWS compile-time guard
        driver = GetWindowsDriverService(devicePath);
#pragma warning restore CA1416
#endif

        return new UsbDeviceInfo(
            vid, pid, rev,
            d.Vendor ?? "",
            d.Product ?? d.DeviceName ?? "",
            driver,
            devicePath);
    }
}
