using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class Wb32DfuDevice : BootloaderDevice
    {
        public Wb32DfuDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.Wb32Dfu;
            Name = "WB32 DFU";
            PreferredDriver = "WinUSB";
            IsResettable = true;
        }

        public async override Task Flash(string mcu, string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                await RunProcessAsync("wb32-dfu-updater_cli.exe", $"--toolbox-mode --dfuse-address 0x08000000 --download \"{file}\"");
            }
            else if (Path.GetExtension(file)?.ToLower() == ".hex")
            {
                await RunProcessAsync("wb32-dfu-updater_cli.exe", $"--toolbox-mode --download \"{file}\"");
            }
            else
            {
                PrintMessage("Only firmware files in .bin or .hex format can be flashed with wb32-dfu-updater_cli!", MessageType.Error);
            }
        }

        public async override Task Reset(string mcu) => await RunProcessAsync("wb32-dfu-updater_cli.exe", $"--reset");
    }
}
