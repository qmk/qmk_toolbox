using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>APM32 DFU bootloader device (Geehy APM32, via dfu-util).</summary>
internal sealed class Apm32DfuDevice(IUsbDevice device, IFlashToolProvider toolProvider)
    : DfuUtilDevice(device, toolProvider,
        BootloaderType.Apm32Dfu, "APM32 DFU",
        altSetting: 0, deviceId: "314B:0106",
        flashSuffix: ["-s", "0x08000000:leave"],
        resetSuffix: ["-s", "0x08000000:leave"]);
