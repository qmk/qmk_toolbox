// ReSharper disable StringLiteralTypo
using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class Stm32DuinoDevice : BootloaderDevice
{
    public Stm32DuinoDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.Stm32Duino;
        Name = "STM32Duino";
    }

    public override void Flash(string mcu, string file)
    {
        if (Path.GetExtension(file)?.ToLower() == ".bin")
            RunProcessAsync("dfu-util", $"-a 2 -d 1EAF:0003 -R -D \"{file}\"").Wait();
        else
            PrintMessage("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
    }
}