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

    /// <summary>
    /// Returns true when any interface beneath the device syspath reports
    /// <c>bInterfaceClass</c> 08 (mass storage). The kernel emits the device's add uevent
    /// before its interfaces are registered, so this waits briefly until the number of
    /// interface nodes matches the device's <c>bNumInterfaces</c> (or the settle window
    /// runs out) before deciding.
    /// </summary>
    public static bool HasMassStorageInterface(string syspath)
    {
        const int attempts = 5;
        const int delayMs = 100;
        try
        {
            if (!int.TryParse(ReadAttribute(syspath, "bNumInterfaces"), out int expected))
                expected = 1;
            for (int i = 0; i < attempts; i++)
            {
                int seen = 0;
                foreach (string dir in Directory.EnumerateDirectories(syspath))
                {
                    string cls = ReadAttribute(dir, "bInterfaceClass") ?? "";
                    if (cls.Length == 0)
                        continue;
                    seen++;
                    if (cls == "08")
                        return true;
                }
                if (seen >= expected)
                    return false;
                if (i < attempts - 1)
                    Thread.Sleep(delayMs);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
        return false;
    }

    private static string? ReadAttribute(string dir, string name)
    {
        try
        {
            string file = Path.Combine(dir, name);
            return File.Exists(file) ? File.ReadAllText(file).Trim() : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }
}
