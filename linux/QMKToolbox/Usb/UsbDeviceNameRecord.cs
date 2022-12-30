namespace QMK_Toolbox.Usb;

public class UsbDeviceNameRecord
{
    public ushort ProductId{ get; set; }
    public ushort VendorId{ get; set; }

    public ushort DeviceRevisionBcd{ get; set; }
    public string ManufacturerName { get; set; }
}
