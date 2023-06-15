using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class Sn32DfuDevice : BootloaderDevice
    {
        public Sn32DfuDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.Sn32Dfu;
            Name = "SN32 DFU";
            PreferredDriver = "HidUsb";
        }

        public async override Task Flash(string mcu, string file)
        {
            if (ProductId == 0x7010) // SN32F260
            {
                await RunProcessAsync("sonixflasher.exe", $"-v {VendorId:x4}:{ProductId:x4} -o 0x200 -f \"{file}\"");
                return;
            }

            await RunProcessAsync("sonixflasher.exe", $"-v {VendorId:x4}:{ProductId:x4} -f \"{file}\"");
        }
    }
}
