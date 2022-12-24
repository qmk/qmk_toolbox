using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class AvrIspDevice : BootloaderDevice
    {
        private string ComPort { get; }

        public AvrIspDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.AvrIsp;
            Name = "AVR ISP";
            PreferredDriver = "usbser";

            ComPort = FindComPort();
        }

        public async override Task Flash(string mcu, string file)
        {
            if (ComPort == null)
            {
                PrintMessage("COM port not found!", MessageType.Error);
                return;
            }

            await RunProcessAsync("avrdude.exe", $"-p {mcu} -c avrisp -U flash:w:\"{file}\":i -P {ComPort}");
        }
    }
}
