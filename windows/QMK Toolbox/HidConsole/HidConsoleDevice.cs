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

        private string currentLine = "";

        private void HidDeviceReportEvent(HidReport report)
        {
            if (HidDevice.IsConnected)
            {
                // Check if we have a completed line queued
                int lineEnd = currentLine.IndexOf('\n');
                if (lineEnd == -1)
                {
                    // Partial line or nothing - append incoming report to current line
                    var reportString = string.Empty;
                    for (var i = 0; i < report.Data.Length; i++)
                    {
                        reportString += (char)report.Data[i];
                        if (i % 16 != 15 || i >= report.Data.Length) continue;
                    }
                    currentLine += reportString.Trim('\0');
                }

                // Check again for a completed line
                lineEnd = currentLine.IndexOf('\n');
                while (lineEnd >= 0)
                {
                    // Fire delegate with completed lines until we have none left
                    string completedLine = currentLine.Substring(0, lineEnd);
                    currentLine = currentLine.Substring(lineEnd + 1);
                    lineEnd = currentLine.IndexOf('\n');
                    consoleReportReceived?.Invoke(this, completedLine);
                }

                // Reregister this callback
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
