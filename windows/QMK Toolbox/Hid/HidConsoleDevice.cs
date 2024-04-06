using HidLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Hid
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

            RegisterReportTask();

            HidDevice.CloseDevice();
        }

        public override string ToString()
        {
            return $"{ManufacturerString} {ProductString} ({VendorId:X4}:{ProductId:X4}:{RevisionBcd:X4})";
        }

        private void RegisterReportTask()
        {
            Task.Run(async () => await ReadReportAsync()).ContinueWith(t => HidDeviceReportEvent(t.Result));
        }

        private async Task<HidReport> ReadReportAsync()
        {
            return await Task.Run(() => HidDevice.ReadReport());
        }

        private List<byte> currentLine = new();

        private void HidDeviceReportEvent(HidReport report)
        {
            if (HidDevice.IsConnected)
            {
                // Check if we have a completed line queued
                int lineEnd = currentLine.IndexOf((byte)'\n');
                if (lineEnd == -1)
                {
                    // Partial line or nothing - append incoming report to current line
                    foreach (byte b in report.Data)
                    {
                        // Trim trailing null bytes
                        if (b == 0) break;
                        currentLine.Add(b);
                    }
                }

                // Check again for a completed line
                lineEnd = currentLine.IndexOf((byte)'\n');
                while (lineEnd >= 0)
                {
                    // Fire delegate with completed lines until we have none left
                    // Only convert to string at the last possible moment in case there is a UTF-8 sequence split across reports
                    string completedLine = Encoding.UTF8.GetString(currentLine.GetRange(0, lineEnd).ToArray());
                    currentLine = currentLine.Skip(lineEnd + 1).ToList();
                    lineEnd = currentLine.IndexOf((byte)'\n');
                    consoleReportReceived?.Invoke(this, completedLine);
                }

                // Reregister this callback
                RegisterReportTask();
            }
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
