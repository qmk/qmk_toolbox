using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class CaterinaDevice : BootloaderDevice
    {
        public string ComPort { get; }

        public CaterinaDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.Caterina;
            Name = "Caterina";
            PreferredDriver = "usbser";
            IsEepromFlashable = true;

            ComPort = FindComPort();
        }

        public async override Task Flash(string mcu, string file)
        {
            if (ComPort == null)
            {
                PrintMessage("COM port not found!", MessageType.Error);
                return;
            }

            await RunProcessAsync("avrdude.exe", $"-p {mcu} -c avr109 -U flash:w:\"{file}\":i -P {ComPort}");
        }

        public async override Task FlashEeprom(string mcu, string file)
        {
            if (ComPort == null)
            {
                PrintMessage("COM port not found!", MessageType.Error);
                return;
            }

            await RunProcessAsync("avrdude.exe", $"-p {mcu} -c avr109 -U eeprom:w:\"{file}\":i -P {ComPort}");
        }

        public override string ToString() => $"{base.ToString()} [{ComPort}]";
    }
}
