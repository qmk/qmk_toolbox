namespace QmkToolbox.Core.Models;

public record UsbDeviceInfo(
    ushort VendorId,
    ushort ProductId,
    ushort RevisionBcd,
    string ManufacturerString,
    string ProductString,
    string Driver,
    string DevicePath
) : IUsbDevice
{
    // Keep the same shape as BootloaderDevice.ToString() so log lines render consistently.
    public override string ToString() =>
        $"{ManufacturerString} {ProductString} ({VendorId:X4}:{ProductId:X4}:{RevisionBcd:X4})".Trim();
}
