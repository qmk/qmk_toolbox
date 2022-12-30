// ReSharper disable StringLiteralTypo
using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class Stm32DfuDevice : BootloaderDevice
{
    public Stm32DfuDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.Stm32Dfu;
        Name = "STM32 DFU";
        IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        if (Path.GetExtension(file)?.ToLower() == ".bin")
            RunProcessAsync("dfu-util", $"-a 0 -d 0483:DF11 -s 0x08000000:leave -D \"{file}\"").Wait();
        else
            PrintMessage("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
    }

    public override void Reset(string mcu)
    {
        RunProcessAsync("dfu-util", "-a 0 -d 0483:DF11 -s 0x08000000:leave").Wait();
    }
}