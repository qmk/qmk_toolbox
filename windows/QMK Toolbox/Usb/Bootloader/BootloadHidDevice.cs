using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class BootloadHidDevice : BootloaderDevice
    {
        public BootloadHidDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.AtmelDfu;
            Name = "BootloadHID";
            PreferredDriver = "HidUsb";
            IsResettable = true;
        }

        public async override Task Flash(string mcu, string file) => await RunProcessAsync("bootloadHID.exe", $"-r \"{file}\"");

        public async override Task Reset(string mcu) => await RunProcessAsync("bootloadHID.exe", $"-r");
    }
}
