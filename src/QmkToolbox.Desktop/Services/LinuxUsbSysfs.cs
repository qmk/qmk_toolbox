using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

/// <summary>Reads USB device attributes from a Linux sysfs device node.</summary>
public static class LinuxUsbSysfs
{
    /// <summary>
    /// Reads the <c>bcdDevice</c> attribute beneath a udev syspath
    /// (e.g. <c>/sys/devices/.../usb1/1-2/bcdDevice</c>). Returns 0 when the attribute is
    /// absent or unreadable — a missing revision must never break device detection.
    /// </summary>
    public static ushort ReadBcdDevice(string syspath)
    {
        try
        {
            string file = Path.Combine(syspath, "bcdDevice");
            return !File.Exists(file) ? (ushort)0 : UsbDeviceParser.TryParseBcdDevice(File.ReadAllText(file), out ushort rev) ? rev : (ushort)0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return 0;
        }
    }
}
