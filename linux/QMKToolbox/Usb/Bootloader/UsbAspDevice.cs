using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class UsbAspDevice : BootloaderDevice
    {
        public UsbAspDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.UsbAsp;
            Name = "USBasp";
            PreferredDriver = "libusbK";
            IsEepromFlashable = true;
        }

        public async override Task Flash(string mcu, string file) => await RunProcessAsync("avrdude.exe", $"-p {mcu} -c usbasp -U flash:w:\"{file}\":i");

        public async override Task FlashEeprom(string mcu, string file) => await RunProcessAsync("avrdude.exe", $"-p {mcu} -c usbasp -U eeprom:w:\"{file}\":i");
    }
}
