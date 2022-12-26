// ReSharper disable StringLiteralTypo
namespace QMK_Toolbox.Usb.Bootloader;

internal class CaterinaDevice : BootloaderDevice
{
    public CaterinaDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.Caterina;
        Name = "Caterina";

        IsEepromFlashable = true;

        // TODO: Fix this
        ComPort = "/dev/ttyS0"; // hard coded for now
    }

    public string ComPort { get; }

    public override void Flash(string mcu, string file)
    {
        if (ComPort == null)
        {
            PrintMessage("COM port not found!", MessageType.Error);
            return;
        }

        RunProcessAsync("/tmp/avrdude", $"-p {mcu} -c avr109 -U flash:w:\"{file}\":i -P {ComPort}").Wait();
    }

    public override void FlashEeprom(string mcu, string file)
    {
        if (ComPort == null)
        {
            PrintMessage("COM port not found!", MessageType.Error);
            return;
        }

        RunProcessAsync("/tmp/avrdude", $"-p {mcu} -c avr109 -U eeprom:w:\"{file}\":i -P {ComPort}").Wait();
    }

    public override string ToString()
    {
        return $"{base.ToString()} [{ComPort}]";
    }
}