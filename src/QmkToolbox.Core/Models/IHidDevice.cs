namespace QmkToolbox.Core.Models;

/// <summary>
/// Represents a connected HID device.
/// </summary>
public interface IHidDevice
{
    ushort VendorId { get; }
    ushort ProductId { get; }
    /// <summary>Device revision in BCD format (e.g. <c>0x0200</c> = 2.00).</summary>
    ushort RevisionBcd { get; }
    /// <summary>HID usage page (top-level category, e.g. <c>0xFF60</c> for QMK console).</summary>
    ushort UsagePage { get; }
    /// <summary>HID usage within the usage page.</summary>
    ushort Usage { get; }
    string ManufacturerString { get; }
    string ProductString { get; }
    string DevicePath { get; }
    /// <summary><see langword="true"/> when the device exposes a QMK HID console interface.</summary>
    bool IsConsoleDevice { get; }
}
