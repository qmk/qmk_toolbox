using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class AT32DfuDevice : BootloaderDevice
    {
        public AT32DfuDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.AT32Dfu;
            Name = "AT32 DFU";
            PreferredDriver = "WinUSB";
            IsResettable = true;
        }

        public async override Task Flash(string mcu, string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                await RunProcessAsync("dfu-util.exe", $"-a 0 -d 2E3C:DF11 -s 0x08000000:leave -D \"{file}\"");
            }
            else
            {
                PrintMessage("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        public async override Task Reset(string mcu) => await RunProcessAsync("dfu-util.exe", "-a 0 -d 2E3C:DF11 -s 0x08000000:leave");
    }
}
