using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class AtmelSamBaDevice : BootloaderDevice
    {
        private string ComPort { get; }

        public AtmelSamBaDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.AtmelSamBa;
            Name = "Atmel SAM-BA";
            PreferredDriver = "usbser";
            IsResettable = true;

            ComPort = FindComPort();
        }

        public async override Task Flash(string mcu, string file)
        {
            if (ComPort == null)
            {
                PrintMessage("COM port not found!", MessageType.Error);
                return;
            }

            await RunProcessAsync("mdloader.exe", $"-p {ComPort} -D \"{file}\" --restart");
        }

        public async override Task Reset(string mcu)
        {
            if (ComPort == null)
            {
                PrintMessage("COM port not found!", MessageType.Error);
                return;
            }

            await RunProcessAsync("mdloader.exe", $"-p {ComPort} --restart");
        }

        public override string ToString() => $"{base.ToString()} [{ComPort}]";
    }
}
