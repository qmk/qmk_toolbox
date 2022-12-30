// ReSharper disable StringLiteralTypo
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class HalfKayDevice : BootloaderDevice
{
    public HalfKayDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.HalfKay;
        Name = "HalfKay";

        IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        RunProcessAsync("teensy_loader_cli", $"-mmcu={mcu} \"{file}\" -v").Wait();
    }

    public override void Reset(string mcu)
    {
        RunProcessAsync("teensy_loader_cli", $"-mmcu={mcu} -bv").Wait();
    }
}