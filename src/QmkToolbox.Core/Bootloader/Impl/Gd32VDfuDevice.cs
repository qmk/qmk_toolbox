using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>GD32V DFU bootloader device (RISC-V, via dfu-util).</summary>
internal sealed class Gd32VDfuDevice(IUsbDevice device, IFlashToolProvider toolProvider)
    : DfuUtilDevice(device, toolProvider,
        BootloaderType.Gd32VDfu, "GD32V DFU",
        altSetting: 0, deviceId: "28E9:0189",
        flashSuffix: ["-s", "0x08000000:leave"],
        resetSuffix: ["-s", "0x08000000:leave"]);
