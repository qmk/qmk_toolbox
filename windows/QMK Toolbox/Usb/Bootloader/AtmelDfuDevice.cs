using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class AtmelDfuDevice : BootloaderDevice
    {
        public AtmelDfuDevice(UsbDevice d) : base(d)
        {
            if (d.RevisionBcd == 0x0936)
            {
                Type = BootloaderType.QmkDfu;
                Name = "QMK DFU";
            }
            else
            {
                Type = BootloaderType.AtmelDfu;
                Name = "Atmel DFU";
            }
            PreferredDriver = "WinUSB";
            IsEepromFlashable = true;
            IsResettable = true;
        }

        public async override Task Flash(string mcu, string file)
        {
            await RunProcessAsync("dfu-programmer.exe", $"{mcu} erase --force");
            await Task.Delay(5);
            await RunProcessAsync("dfu-programmer.exe", $"{mcu} flash --force \"{file}\"");
            await Task.Delay(5);
            await RunProcessAsync("dfu-programmer.exe", $"{mcu} reset");
        }

        public async override Task FlashEeprom(string mcu, string file)
        {
            if (Type == BootloaderType.AtmelDfu)
            {
                await RunProcessAsync("dfu-programmer.exe", $"{mcu} erase --force");
            }

            await RunProcessAsync("dfu-programmer.exe", $"{mcu} flash --force --suppress-validation --eeprom \"{file}\"");

            if (Type == BootloaderType.AtmelDfu)
            {
                PrintMessage("Please reflash device with firmware now", MessageType.Bootloader);
            }
        }

        public async override Task Reset(string mcu) => await RunProcessAsync("dfu-programmer.exe", $"{mcu} reset");
    }
}
