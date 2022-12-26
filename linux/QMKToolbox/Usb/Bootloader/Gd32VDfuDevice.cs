// ReSharper disable StringLiteralTypo
using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class Gd32VDfuDevice : BootloaderDevice
{
    public Gd32VDfuDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.Gd32VDfu;
        Name = "GD32V DFU";
        IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        if (Path.GetExtension(file)?.ToLower() == ".bin")
            RunProcessAsync("/tmp/dfu-util", $"-a 0 -d 28E9:0189 -s 0x08000000:leave -D \"{file}\"").Wait();
        else
            PrintMessage("Only firmware files in .bin format can be flashed with dfu-util!",
                MessageType.Error);
    }

    public override void Reset(string mcu)
    { 
        RunProcessAsync("/tmp/dfu-util", "-a 0 -d 28E9:0189 -s 0x08000000:leave").Wait();
    }
}