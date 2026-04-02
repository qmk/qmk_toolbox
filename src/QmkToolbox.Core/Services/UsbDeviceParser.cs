using System.Globalization;
using System.Text.RegularExpressions;

namespace QmkToolbox.Core.Services;

/// <summary>USB device path and ID parsing helpers.</summary>
public static class UsbDeviceParser
{
    public static readonly Regex HwIdRegex = new(
        @"VID_([0-9A-Fa-f]{4})&PID_([0-9A-Fa-f]{4})(?:&REV_([0-9A-Fa-f]{4}))?",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses a USB ID string whose format varies by platform:
    /// macOS IOKit reports plain decimal (e.g. "1155" for 0x0483),
    /// Linux udev uses "0x"-prefixed hex, and Windows uses bare 4-digit hex.
    /// </summary>
    public static bool TryParseUsbId(string? s, out ushort value)
        => TryParseUsbId(s, OperatingSystem.IsMacOS(), out value);

    public static bool TryParseUsbId(string? s, bool isMacOS, out ushort value)
    {
        value = 0;
        if (string.IsNullOrEmpty(s))
            return false;
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return ushort.TryParse(s.AsSpan(2), NumberStyles.HexNumber, null, out value);
        if (isMacOS)
            return ushort.TryParse(s, NumberStyles.None, null, out value);
        // Windows/Linux bare hex
        return ushort.TryParse(s, NumberStyles.HexNumber, null, out value);
    }

    public static bool TryParseHwId(string devicePath, out ushort vid, out ushort pid, out ushort rev)
    {
        vid = pid = rev = 0;
        Match m = HwIdRegex.Match(devicePath);
        if (!m.Success)
            return false;
        vid = ushort.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
        pid = ushort.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
        if (m.Groups[3].Success)
            rev = ushort.Parse(m.Groups[3].Value, NumberStyles.HexNumber);
        return true;
    }

    /// <summary>
    /// Returns true only for Windows root USB devices (USB\VID_...\... without a &amp;MI_ interface suffix).
    /// On non-Windows platforms always returns true.
    /// </summary>
    public static bool IsWindowsRootUsbDevice(string devicePath)
        => IsWindowsRootUsbDevice(devicePath, OperatingSystem.IsWindows());

    public static bool IsWindowsRootUsbDevice(string devicePath, bool isWindows) =>
        !isWindows ||
        (devicePath.StartsWith("USB\\VID_", StringComparison.OrdinalIgnoreCase) &&
         !devicePath.Contains("&MI_", StringComparison.OrdinalIgnoreCase));
}
