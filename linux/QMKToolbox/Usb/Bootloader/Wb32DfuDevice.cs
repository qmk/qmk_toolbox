// ReSharper disable StringLiteralTypo
using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class Wb32DfuDevice : BootloaderDevice
{
    public Wb32DfuDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.Wb32Dfu;
        Name = "WB32 DFU";
        IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        if (Path.GetExtension(file)?.ToLower() == ".bin")
            RunProcessAsync
                ("/tmp/wb32-dfu-updater_cli", $"--toolbox-mode --dfuse-address 0x08000000 --download \"{file}\"").Wait();
        else if (Path.GetExtension(file)?.ToLower() == ".hex")
            RunProcessAsync("wb32-dfu-updater_cli", $"--toolbox-mode --download \"{file}\"").Wait();
        else
            PrintMessage("Only firmware files in .bin or .hex format can be flashed with wb32-dfu-updater_cli!",
                MessageType.Error);
    }

    public override void Reset(string mcu)
    {
        RunProcessAsync("wb32-dfu-updater_cli", "--reset").Wait();
    }
}