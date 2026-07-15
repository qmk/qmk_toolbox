using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>LUFA HID / QMK HID bootloader device (via hid_bootloader_cli).</summary>
internal sealed class LufaHidDevice : BootloaderDevice
{
    public LufaHidDevice(IUsbDevice device, IFlashToolProvider toolProvider, BootloaderType type)
        : base(device, toolProvider)
    {
        Type = type;
        Name = type == BootloaderType.QmkHid ? "QMK HID" : "LUFA HID";
        PreferredDriver = "HidUsb";
    }

    public override Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".hex");
        return RunToolAsync("hid_bootloader_cli", $"-mmcu={mcu}", file, "-v");
    }
}
