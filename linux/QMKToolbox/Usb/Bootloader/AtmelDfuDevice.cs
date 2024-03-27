
// ReSharper disable StringLiteral

using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class AtmelDfuDevice : BootloaderDevice
{
    public AtmelDfuDevice(KnownHidDevice d) : base(d)
    {
        if (d.RevisionBcd == 0x0936)
        {
            Type = BootloaderType.QmkDfu;
            Name = "QMK DFU";
        }
        else
        {
            Type = BootloaderType.AtmelDfu;
            Name = "Atmel DFU";
        }

        IsEepromFlashable = true;
        IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        RunProcessAsync("dfu-programmer", $"{mcu} erase --force").Wait();
        Task.Delay(5).Wait();
        RunProcessAsync("dfu-programmer", $"{mcu} flash --force \"{file}\"").Wait();
        Task.Delay(5).Wait();
        RunProcessAsync("dfu-programmer", $"{mcu} reset").Wait();
    }

    public override void FlashEeprom(string mcu, string file)
    {
        if (Type == BootloaderType.AtmelDfu) 
            RunProcessAsync("dfu-programmer", $"{mcu} erase --force").Wait();

        RunProcessAsync("dfu-programmer",
            $"{mcu} flash --force --suppress-validation --eeprom \"{file}\"").Wait();

        if (Type == BootloaderType.AtmelDfu)
            PrintMessage("Please reflash device with firmware now", MessageType.Bootloader);
    }

    public override void Reset(string mcu)
    {
        RunProcessAsync("dfu-programmer", $"{mcu} reset").Wait();
    }
}