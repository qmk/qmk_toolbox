using System.Threading.Tasks;

// ReSharper disable StringLiteralTypo
namespace QMK_Toolbox.Usb.Bootloader;

internal class AtmelSamBaDevice : BootloaderDevice
{
    public AtmelSamBaDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.AtmelSamBa;
        Name = "Atmel SAM-BA";
        IsResettable = true;
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

        RunProcessAsync("/tmp/mdloader", $"-p {ComPort} -D \"{file}\" --restart").Wait();
    }

    public override void Reset(string mcu)
    {
        if (ComPort == null)
        {
            PrintMessage("COM port not found!", MessageType.Error);
            return;
        }

        RunProcessAsync("/tmp/mdloader", $"-p {ComPort} --restart").Wait();
    }

    public override string ToString()
    {
        return $"{base.ToString()} [{ComPort}]";
    }
}