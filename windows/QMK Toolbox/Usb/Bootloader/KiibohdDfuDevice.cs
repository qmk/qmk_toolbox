﻿using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class KiibohdDfuDevice : BootloaderDevice
    {
        public KiibohdDfuDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.KiibohdDfu;
            Name = "Kiibohd DFU";
            PreferredDriver = "WinUSB";
            IsResettable = true;
        }

        public async override Task Flash(string mcu, string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                await RunProcessAsync("dfu-util.exe", $"-a 0 -d 1C11:B007 -D \"{file}\"");
            }
            else
            {
                PrintMessage("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        public async override Task Reset(string mcu) => await RunProcessAsync("dfu-util.exe", "-a 0 -d 1C11:B007 -e");
    }
}
