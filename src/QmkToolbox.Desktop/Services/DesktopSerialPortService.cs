using System.IO.Ports;
using System.Runtime.Versioning;
using Microsoft.Win32;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Cross-platform serial port service.
/// <list type="bullet">
///   <item>Linux: VID/PID matching via /dev/serial/by-id/ symlinks</item>
///   <item>macOS: most recently created /dev/cu.* device node</item>
///   <item>Windows: registry lookup mapping VID/PID → COM port name</item>
/// </list>
/// </summary>
public class DesktopSerialPortService : ISerialPortService
{
    public string? FindSerialPort(IUsbDevice device)
    {
        if (OperatingSystem.IsLinux())
            return FindByIdLinux(device);

        if (OperatingSystem.IsMacOS())
            return FindNewestSerialPortMacOS();

        if (OperatingSystem.IsWindows())
            return FindByRegistryWindows(device);

        // Unknown platform fallback
        string[] fallbackPorts = SerialPort.GetPortNames();
        return fallbackPorts.Length > 0 ? fallbackPorts[0] : null;
    }

    /// <summary>
    /// Matches a USB device by VID/PID against /dev/serial/by-id/ symlinks.
    /// These symlinks are maintained by udev and encode the VID, PID, and serial
    /// number in their filename, providing a reliable match without timestamp heuristics.
    /// </summary>
    [SupportedOSPlatform("linux")]
    private static string? FindByIdLinux(IUsbDevice device)
    {
        const string byIdDir = "/dev/serial/by-id";
        if (!Directory.Exists(byIdDir))
            return null;

        // udev names numeric-only devices as "usb-VID_PID_SERIAL-ifNN".
        // Matching the combined "VID_PID" token avoids false positives where a
        // serial number from a different device happens to contain the VID digits.
        string vidPid = $"{device.VendorId:X4}_{device.ProductId:X4}";

        foreach (string link in Directory.EnumerateFiles(byIdDir))
        {
            string name = Path.GetFileName(link);
            if (name.Contains(vidPid, StringComparison.OrdinalIgnoreCase))
            {
                var fi = new FileInfo(link);
                FileSystemInfo? resolved = fi.ResolveLinkTarget(returnFinalTarget: true);
                return resolved?.FullName ?? link;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the most recently created /dev/cu.* serial device.
    /// Since FindSerialPort is called immediately after USB device detection,
    /// the target device will be the newest serial port.
    /// <para>
    /// Known limitation: another serial device appearing between detection and this call
    /// could be selected. In practice this window is very small and users rarely have two
    /// devices in bootloader mode simultaneously.
    /// </para>
    /// </summary>
    [SupportedOSPlatform("macos")]
    private static string? FindNewestSerialPortMacOS()
    {
        string[] ports = SerialPort.GetPortNames();
        if (ports.Length == 0)
            return null;
        if (ports.Length == 1)
            return ports[0];

        // Sort by device node creation time (descending) — newest first
        return ports
            .Select(p => new FileInfo(p))
            .Where(fi => fi.Exists)
            .OrderByDescending(fi => fi.CreationTimeUtc)
            .Select(fi => fi.FullName)
            .FirstOrDefault();
    }

    /// <summary>
    /// Looks up the COM port assigned to a USB device by walking the Windows
    /// registry at HKLM\SYSTEM\CurrentControlSet\Enum\USB\VID_xxxx&amp;PID_xxxx.
    /// Each child key contains a "Device Parameters" sub-key with a "PortName"
    /// value (e.g. "COM12"). Falls back to first available port if the lookup fails.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static string? FindByRegistryWindows(IUsbDevice device)
    {
        try
        {
            string vidPid = $"VID_{device.VendorId:X4}&PID_{device.ProductId:X4}";
            string keyPath = $@"SYSTEM\CurrentControlSet\Enum\USB\{vidPid}";

            using RegistryKey? usbKey = Registry.LocalMachine.OpenSubKey(keyPath);
            if (usbKey is not null)
            {
                // Each sub-key represents a device instance (keyed by serial number)
                foreach (string instanceId in usbKey.GetSubKeyNames())
                {
                    using RegistryKey? instanceKey = usbKey.OpenSubKey(instanceId);
                    using RegistryKey? paramsKey = instanceKey?.OpenSubKey("Device Parameters");
                    if (paramsKey?.GetValue("PortName") is string portName)
                    {
                        // Verify the port actually exists right now
                        string[] activePorts = SerialPort.GetPortNames();
                        if (Array.Exists(activePorts, p => p.Equals(portName, StringComparison.OrdinalIgnoreCase)))
                            return portName;
                    }
                }
            }
        }
        catch
        {
            // Registry access may fail due to permissions; fall through to fallback
        }

        // Fallback: return first available port
        string[] ports = SerialPort.GetPortNames();
        return ports.Length > 0 ? ports[0] : null;
    }
}
