using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>LUFA HID / QMK HID bootloader device (via hid_bootloader_cli).</summary>
internal sealed class LufaHidDevice : BootloaderDevice
{
    public LufaHidDevice(IUsbDevice device, IFlashToolProvider toolProvider)
        : base(device, toolProvider)
    {
        if (device.RevisionBcd == QmkRevisionMarker)
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
    }

    public override Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".hex");
        return RunToolAsync("hid_bootloader_cli", $"-mmcu={mcu}", file, "-v");
    }
}
