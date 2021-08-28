using HidLibrary;
using System.Linq;

namespace QMK_Toolbox.HidConsole
{
    public class HidConsoleDevice
    {
        public delegate void HidConsoleReportReceivedDelegate(HidConsoleDevice device, string data);

        public HidConsoleReportReceivedDelegate consoleReportReceived;

        public IHidDevice HidDevice { get; }

        public string ManufacturerString { get; }

        public string ProductString { get; }

        public ushort VendorId { get; }

        public ushort ProductId { get; }

        public ushort RevisionBcd { get; }

        public HidConsoleDevice(IHidDevice device)
        {
            HidDevice = device;
            HidDevice.OpenDevice();

            ManufacturerString = GetManufacturerString(HidDevice);
            ProductString = GetProductString(HidDevice);
            VendorId = (ushort)HidDevice.Attributes.VendorId;
            ProductId = (ushort)HidDevice.Attributes.ProductId;
            RevisionBcd = (ushort)HidDevice.Attributes.Version;

            HidDevice.MonitorDeviceEvents = true;
            HidDevice.ReadReport(HidDeviceReportEvent);

            HidDevice.CloseDevice();
        }

        public override string ToString()
        {
            return $"{ManufacturerString} {ProductString} ({VendorId:X4}:{ProductId:X4}:{RevisionBcd:X4})";
        }

        private void HidDeviceReportEvent(HidReport report)
        {
            if (HidDevice.IsConnected)
            {
                var outputString = string.Empty;
                for (var i = 0; i < report.Data.Length; i++)
                {
                    outputString += (char)report.Data[i];
                    if (i % 16 != 15 || i >= report.Data.Length) continue;
                }
                consoleReportReceived?.Invoke(this, outputString);
                outputString = string.Empty;

                HidDevice.ReadReport(HidDeviceReportEvent);
            }
        }

        private static string GetManufacturerString(IHidDevice d)
        {
            if (d == null) return "";

            d.ReadManufacturer(out var bs);
            return System.Text.Encoding.Default.GetString(bs.Where(b => b > 0).ToArray());
        }

        private static string GetProductString(IHidDevice d)
        {
            if (d == null) return "";

            d.ReadProduct(out var bs);
            return System.Text.Encoding.Default.GetString(bs.Where(b => b > 0).ToArray());
        }
    }
}
