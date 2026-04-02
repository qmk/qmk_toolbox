using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>Atmel DFU / QMK DFU bootloader device (ATmega/AT90USB, via dfu-programmer).</summary>
internal sealed class AtmelDfuDevice : BootloaderDevice
{
    public AtmelDfuDevice(IUsbDevice device, IFlashToolProvider toolProvider, ISerialPortService? serialPortService = null)
        : base(device, toolProvider, serialPortService)
    {
        if (device.RevisionBcd == QmkRevisionMarker)
        {
            Type = BootloaderType.QmkDfu;
            Name = "QMK DFU";
        }
        else
        {
            Type = BootloaderType.AtmelDfu;
            Name = "Atmel DFU";
        }
        PreferredDriver = "WinUSB";
        IsEepromFlashable = true;
        IsResettable = true;
    }

    public override async Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".hex");
        await RunToolAsync("dfu-programmer", mcu, "erase", "--force").ConfigureAwait(false);
        await Task.Delay(100).ConfigureAwait(false);
        await RunToolAsync("dfu-programmer", mcu, "flash", "--force", file).ConfigureAwait(false);
        await Task.Delay(100).ConfigureAwait(false);
        await RunToolAsync("dfu-programmer", mcu, "reset").ConfigureAwait(false);
    }

    public override async Task FlashEepromAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".eep", ".hex");

        if (Type == BootloaderType.AtmelDfu)
            await RunToolAsync("dfu-programmer", mcu, "erase", "--force").ConfigureAwait(false);

        await RunToolAsync("dfu-programmer", mcu, "flash", "--force", "--suppress-validation", "--eeprom", file).ConfigureAwait(false);

        if (Type == BootloaderType.AtmelDfu)
            PrintMessage("Please reflash device with firmware now", MessageType.Bootloader);
    }

    public override Task ResetAsync(string mcu) =>
        RunToolAsync("dfu-programmer", mcu, "reset");
}
