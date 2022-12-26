// ReSharper disable StringLiteralTypo

namespace QMK_Toolbox.Usb.Bootloader;

internal class LufaHidDevice : BootloaderDevice
{
    public LufaHidDevice(KnownHidDevice d) : base(d)
    {
        if (d.RevisionBcd == 0x0936)
        {
            Type = BootloaderType.QmkHid;
            Name = "QMK HID";
        }
        else
        {
            Type = BootloaderType.LufaHid;
            Name = "LUFA HID";
        }

        //IsResettable = true;
    }

    public override void Flash(string mcu, string file)
    {
        RunProcessAsync("/tmp/hid_bootloader_cli", $"-mmcu={mcu} \"{file}\" -v").Wait();
    }

    // hid_bootloader_cli 210130 lacks -b flag
    // Next LUFA release should have it thanks to abcminiuser/lufa#173
    //public async override Task Reset(string mcu) => await RunProcessAsync("/tmp/hid_bootloader_cli", $"-mmcu={mcu} -bv");
}