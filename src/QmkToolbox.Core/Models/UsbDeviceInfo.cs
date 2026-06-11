namespace QmkToolbox.Core.Models;

public record UsbDeviceInfo(
    ushort VendorId,
    ushort ProductId,
    ushort RevisionBcd,
    string ManufacturerString,
    string ProductString,
    string Driver,
    string DevicePath
) : IUsbDevice;
