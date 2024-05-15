using HidLibrary;
using System.Linq;
using System.Text;

namespace QMK_Toolbox.Hid
{
    public abstract class BaseHidDevice
    {
        public IHidDevice HidDevice { get; }

        public string ManufacturerString { get; }

        public string ProductString { get; }

        public ushort VendorId { get; }

        public ushort ProductId { get; }

        public ushort RevisionBcd { get; }

        public ushort UsagePage { get; }

        public ushort Usage { get; }

        public BaseHidDevice(IHidDevice device)
        {
            HidDevice = device;
            HidDevice.OpenDevice();

            ManufacturerString = GetManufacturerString(HidDevice);
            ProductString = GetProductString(HidDevice);

            VendorId = (ushort)HidDevice.Attributes.VendorId;
            ProductId = (ushort)HidDevice.Attributes.ProductId;
            RevisionBcd = (ushort)HidDevice.Attributes.Version;
            UsagePage = (ushort)HidDevice.Capabilities.UsagePage;
            Usage = (ushort)HidDevice.Capabilities.Usage;

            HidDevice.CloseDevice();
        }

        public override string ToString()
        {
            return $"{ManufacturerString} {ProductString} ({VendorId:X4}:{ProductId:X4}:{RevisionBcd:X4})";
        }

        private static string GetManufacturerString(IHidDevice d)
        {
            if (d == null) return "";

            d.ReadManufacturer(out var bs);
            return Encoding.Default.GetString(bs.Where(b => b > 0).ToArray());
        }

        private static string GetProductString(IHidDevice d)
        {
            if (d == null) return "";

            d.ReadProduct(out var bs);
            return Encoding.Default.GetString(bs.Where(b => b > 0).ToArray());
        }
    }
}
