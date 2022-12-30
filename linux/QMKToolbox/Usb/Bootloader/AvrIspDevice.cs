// ReSharper disable StringLiteralTypo
namespace QMK_Toolbox.Usb.Bootloader;

internal class AvrIspDevice : BootloaderDevice
{
    public AvrIspDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.AvrIsp;
        Name = "AVR ISP";
        // TODO: Fix this
        ComPort = "/dev/ttyS0"; // hard coded for now
    }

    private string ComPort { get; }

    public override void Flash(string mcu, string file)
    {
        if (ComPort == null)
        {
            PrintMessage("COM port not found!", MessageType.Error);
            return;
        }
        RunProcessAsync("avrdude", $"-p {mcu} -c avrisp -U flash:w:\"{file}\":i -P {ComPort}").Wait();
    }
}