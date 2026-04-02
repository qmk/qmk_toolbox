using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>ArteryTek AT32 DFU bootloader device (via dfu-util).</summary>
internal sealed class At32DfuDevice(IUsbDevice device, IFlashToolProvider toolProvider)
    : DfuUtilDevice(device, toolProvider,
        BootloaderType.At32Dfu, "ArteryTek AT32 DFU",
        altSetting: 0, deviceId: "2E3C:DF11",
        flashSuffix: ["-s", "0x08000000:leave"],
        resetSuffix: ["-s", "0x08000000:leave"]);
