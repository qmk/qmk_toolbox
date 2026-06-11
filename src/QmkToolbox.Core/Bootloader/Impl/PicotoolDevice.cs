using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>Raspberry Pi BOOTSEL bootloader device (via picotool).</summary>
internal sealed class PicotoolDevice : BootloaderDevice
{
    private static readonly Dictionary<ushort, string> ModelNames = new()
    {
        [0x0003] = "RP2040",
        [0x000F] = "RP2350",
    };

    public PicotoolDevice(IUsbDevice device, IFlashToolProvider toolProvider)
        : base(device, toolProvider)
    {
        string model = ModelNames.GetValueOrDefault(device.ProductId, $"0x{device.ProductId:X4}");
        Type = BootloaderType.Picotool;
        Name = $"Picotool ({model})";
        PreferredDriver = "WinUSB";
        IsResettable = true;
    }

    public override async Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".uf2", ".bin");
        await RunToolAsync("picotool", "load", file).ConfigureAwait(false);
        await RunToolAsync("picotool", "reboot").ConfigureAwait(false);
    }

    public override Task ResetAsync(string mcu) =>
        RunToolAsync("picotool", "reboot");
}
