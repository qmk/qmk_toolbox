using QMK_Toolbox.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    public abstract class BootloaderDevice : IUsbDevice
    {
        public delegate void FlashOutputReceivedDelegate(BootloaderDevice device, string data, MessageType type);

        public FlashOutputReceivedDelegate outputReceived;

        private UsbDevice UsbDevice { get; }

        public ManagementBaseObject WmiDevice { get => UsbDevice.WmiDevice; }

        public ushort VendorId { get => UsbDevice.VendorId; }

        public ushort ProductId { get => UsbDevice.ProductId; }

        public ushort RevisionBcd { get => UsbDevice.RevisionBcd; }

        public string ManufacturerString { get => UsbDevice.ManufacturerString; }

        public string ProductString { get => UsbDevice.ProductString; }

        public string Driver { get => UsbDevice.Driver; }

        public string PreferredDriver { get; protected set; }

        public bool IsEepromFlashable { get; protected set; }

        public bool IsResettable { get; protected set; }

        public BootloaderType Type { get; protected set; }

        public string Name { get; protected set; }

        public BootloaderDevice(UsbDevice d)
        {
            UsbDevice = d;
        }

        public override string ToString() => UsbDevice.ToString();

        public abstract Task Flash(string mcu, string file);

        public virtual Task FlashEeprom(string mcu, string file)
        {
            throw new NotImplementedException();
        }

        public virtual Task Reset(string mcu)
        {
            throw new NotImplementedException();
        }

        protected async Task<int> RunProcessAsync(string command, string args)
        {
            PrintMessage($"{command} {args}", MessageType.Command);
            string toolboxData = EmbeddedResourceHelper.GetResourceFolder();

            using var process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(toolboxData, command),
                    Arguments = args,
                    WorkingDirectory = toolboxData,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };
            return await RunProcessAsync(process).ConfigureAwait(false);
        }

        private Task<int> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.Exited += (sender, e) =>
            {
                process.WaitForExit();
                tcs.SetResult(process.ExitCode);
            };

            process.OutputDataReceived += ProcessOutput;
            process.ErrorDataReceived += ProcessErrorOutput;

            bool started = process.Start();
            if (!started)
            {
                PrintMessage($"Could not start process: {process}", MessageType.Error);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        private void ProcessOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                PrintMessage(e.Data, MessageType.CommandOutput);
            }
        }

        private void ProcessErrorOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                PrintMessage(e.Data, MessageType.CommandError);
            }
        }

        protected void PrintMessage(string message, MessageType type)
        {
            outputReceived?.Invoke(this, message, type);
        }

        protected string FindComPort()
        {
            using var searcher = new ManagementObjectSearcher("SELECT PNPDeviceID, DeviceID FROM Win32_SerialPort");
            foreach (var device in searcher.Get())
            {
                if (device.GetPropertyValue("PNPDeviceID").ToString().Equals(WmiDevice.GetPropertyValue("PNPDeviceID").ToString()))
                {
                    return device.GetPropertyValue("DeviceID").ToString();
                }
            }

            return null;
        }
    }
}
