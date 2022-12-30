using System.Collections.Generic;

namespace QMK_Toolbox.Usb;

internal class UsbDeviceRecordComparer : IEqualityComparer<UsbDeviceNameRecord>
{
    public bool Equals(UsbDeviceNameRecord x, UsbDeviceNameRecord y)
    {
        if (x.VendorId == y.VendorId && x.ProductId == y.ProductId)
        {
            return true;
        }

        return false;
    }

    public int GetHashCode(UsbDeviceNameRecord obj)
    {
        return obj.VendorId.GetHashCode() + obj.ProductId.GetHashCode();
    }
}