// ReSharper disable StringLiteralTypo
using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class KiibohdDfuDevice : BootloaderDevice
{
    public KiibohdDfuDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.KiibohdDfu;
        Name = "Kiibohd DFU";
        IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        if (Path.GetExtension(file)?.ToLower() == ".bin")
            RunProcessAsync("/tmp/dfu-util", $"-a 0 -d 1C11:B007 -D \"{file}\"").Wait();
        else
            PrintMessage("Only firmware files in .bin format can be flashed with dfu-util!",
                MessageType.Error);
    }

    public override void Reset(string mcu)
    {
        RunProcessAsync("/tmp/dfu-util", "-a 0 -d 1C11:B007 -e").Wait();
    }
}