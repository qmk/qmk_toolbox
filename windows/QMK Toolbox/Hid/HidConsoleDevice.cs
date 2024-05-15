using HidLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Hid
{
    public class HidConsoleDevice : BaseHidDevice
    {
        public delegate void HidConsoleReportReceivedDelegate(HidConsoleDevice device, string data);

        public HidConsoleReportReceivedDelegate consoleReportReceived;

        public HidConsoleDevice(IHidDevice device) : base(device)
        {
            RegisterReportTask();
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
    }
}
