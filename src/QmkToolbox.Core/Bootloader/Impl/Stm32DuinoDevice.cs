using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>STM32Duino bootloader device (Leaflabs, via dfu-util).</summary>
internal sealed class Stm32DuinoDevice(IUsbDevice device, IFlashToolProvider toolProvider)
    : DfuUtilDevice(device, toolProvider,
        BootloaderType.Stm32Duino, "STM32Duino",
        altSetting: 2, deviceId: "1EAF:0003",
        flashSuffix: ["-R"],
        resetSuffix: null);
