using QMK_Toolbox.Usb.Bootloader;

namespace QMK_Toolbox.Usb;

internal class KnownHidDevice
{
    public ushort VendorId { get; init; }
    public ushort ProductId { get; init; }

    public ushort RevisionBcd { get; set; } = 0x0100;
    public string VendorName { get; init; }
    public string ProductName { get; init; }
    public BootloaderType BootloaderType { get; init; }
}