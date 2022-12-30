// ReSharper disable StringLiteralTypo
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class UsbTinyIspDevice : BootloaderDevice
{
    public UsbTinyIspDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.UsbTinyIsp;
        Name = "USBtinyISP";
        IsEepromFlashable = true;
    }

    public override void Flash(string mcu, string file)
    {
        RunProcessAsync("avrdude", $"-p {mcu} -c usbtiny -U flash:w:\"{file}\":i").Wait();
    }

    public override void FlashEeprom(string mcu, string file)
    {
        RunProcessAsync("avrdude", $"-p {mcu} -c usbtiny -U eeprom:w:\"{file}\":i").Wait();
    }
}