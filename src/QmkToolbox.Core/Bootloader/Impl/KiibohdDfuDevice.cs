using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>Kiibohd DFU bootloader device (Input Club, via dfu-util).</summary>
internal sealed class KiibohdDfuDevice(IUsbDevice device, IFlashToolProvider toolProvider)
    : DfuUtilDevice(device, toolProvider,
        BootloaderType.KiibohdDfu, "Kiibohd DFU",
        altSetting: 0, deviceId: "1C11:B007",
        flashSuffix: null,
        resetSuffix: ["-e"]);
