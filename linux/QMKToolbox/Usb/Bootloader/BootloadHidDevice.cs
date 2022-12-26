// ReSharper disable StringLiteralTypo
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class BootloadHidDevice : BootloaderDevice
{
    public BootloadHidDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.AtmelDfu;
        Name = "BootloadHID";
        IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        RunProcessAsync("bootloadHID", $"-r \"{file}\"").Wait();
    }

    public override void Reset(string mcu)
    {
        RunProcessAsync("bootloadHID", "-r").Wait();
    }
}