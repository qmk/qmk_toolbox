using System.Management;

namespace QMK_Toolbox.Usb
{
    public interface IUsbDevice
    {
        ManagementBaseObject WmiDevice { get; }

        ushort VendorId { get; }

        ushort ProductId { get; }

        ushort RevisionBcd { get; }

        string ManufacturerString { get; }

        string ProductString { get; }

        string Driver { get; }
    }
}
