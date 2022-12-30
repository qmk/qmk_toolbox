// ReSharper disable StringLiteralTypo

using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class Apm32DfuDevice : BootloaderDevice
{
    public Apm32DfuDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.Apm32Dfu;
        Name = "APM32 DFU";
        IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        if (Path.GetExtension(file)?.ToLower() == ".bin")
            RunProcessAsync("dfu-util", $"-a 0 -d 314B:0106 -s 0x08000000:leave -D \"{file}\"").Wait();
        else
            PrintMessage("Only firmware files in .bin format can be flashed with dfu-util!",
                MessageType.Error);
    }

    public override void Reset(string mcu)
    {
        RunProcessAsync("dfu-util", "-a 0 -d 314B:0106 -s 0x08000000:leave").Wait();
    }
}