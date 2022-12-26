// ReSharper disable StringLiteralTypo
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class UsbAspDevice : BootloaderDevice
{
    public UsbAspDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.UsbAsp;
        Name = "USBasp";
        IsEepromFlashable = true;
    }

    public override void Flash(string mcu, string file)
    {
        RunProcessAsync("/tmp/avrdude", $"-p {mcu} -c usbasp -U flash:w:\"{file}\":i").Wait();
    }

    public override void FlashEeprom(string mcu, string file)
    {
        RunProcessAsync("/tmp/avrdude", $"-p {mcu} -c usbasp -U eeprom:w:\"{file}\":i").Wait();
    }
}