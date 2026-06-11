namespace QmkToolbox.Core.Models;

/// <summary>
/// Represents a connected USB device.
/// </summary>
public interface IUsbDevice
{
    ushort VendorId { get; }
    ushort ProductId { get; }
    /// <summary>Device revision in BCD format (e.g. <c>0x0200</c> = 2.00).</summary>
    ushort RevisionBcd { get; }
    string ManufacturerString { get; }
    string ProductString { get; }
    /// <summary>Driver or subsystem name reported by the OS (e.g. <c>"libusb"</c>, <c>"usbfs"</c>).</summary>
    string Driver { get; }
    string DevicePath { get; }
}
