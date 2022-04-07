using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class UsbTinyIspDevice : BootloaderDevice
    {
        public UsbTinyIspDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.UsbTinyIsp;
            Name = "USBtinyISP";
            PreferredDriver = "libusb0";
            IsEepromFlashable = true;
        }

        public async override Task Flash(string mcu, string file) => await RunProcessAsync("avrdude.exe", $"-p {mcu} -c usbtiny -U flash:w:\"{file}\":i");

        public async override Task FlashEeprom(string mcu, string file) => await RunProcessAsync("avrdude.exe", $"-p {mcu} -c usbtiny -U eeprom:w:\"{file}\":i");
    }
}
