using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class HalfKayDevice : BootloaderDevice
    {
        public HalfKayDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.HalfKay;
            Name = "HalfKay";
            PreferredDriver = "HidUsb";
            IsResettable = true;
        }

        public async override Task Flash(string mcu, string file) => await RunProcessAsync("teensy_loader_cli.exe", $"-mmcu={mcu} \"{file}\" -v");

        public async override Task Reset(string mcu) => await RunProcessAsync("teensy_loader_cli.exe", $"-mmcu={mcu} -bv");
    }
}
