using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>WB32 DFU bootloader device (WestBerryTech, via wb32-dfu-updater_cli).</summary>
internal sealed class Wb32DfuDevice : BootloaderDevice
{
    public Wb32DfuDevice(IUsbDevice device, IFlashToolProvider toolProvider)
        : base(device, toolProvider)
    {
        Type = BootloaderType.Wb32Dfu;
        Name = "WB32 DFU";
        PreferredDriver = "WinUSB";
        IsResettable = true;
    }

    public override async Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".bin", ".hex");

        if (string.Equals(Path.GetExtension(file), ".bin", StringComparison.OrdinalIgnoreCase))
            await RunToolAsync("wb32-dfu-updater_cli", "--toolbox-mode", "--dfuse-address", "0x08000000", "--download", file).ConfigureAwait(false);
        else
            await RunToolAsync("wb32-dfu-updater_cli", "--toolbox-mode", "--download", file).ConfigureAwait(false);
    }

    public override Task ResetAsync(string mcu) =>
        RunToolAsync("wb32-dfu-updater_cli", "--reset");
}
