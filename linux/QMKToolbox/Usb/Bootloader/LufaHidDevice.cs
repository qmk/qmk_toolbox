using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class LufaHidDevice : BootloaderDevice
    {
        public LufaHidDevice(UsbDevice d) : base(d)
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
            PreferredDriver = "HidUsb";
            //IsResettable = true;
        }

        public async override Task Flash(string mcu, string file) => await RunProcessAsync("hid_bootloader_cli.exe", $"-mmcu={mcu} \"{file}\" -v");

        // hid_bootloader_cli 210130 lacks -b flag
        // Next LUFA release should have it thanks to abcminiuser/lufa#173
        //public async override Task Reset(string mcu) => await RunProcessAsync("hid_bootloader_cli.exe", $"-mmcu={mcu} -bv");
    }
}
