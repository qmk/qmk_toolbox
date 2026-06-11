using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>USBasp ISP flasher device (via avrdude with usbasp programmer).</summary>
internal sealed class UsbAspDevice(IUsbDevice device, IFlashToolProvider toolProvider)
    : AvrdudeDevice(device, toolProvider, serialPortService: null,
        BootloaderType.UsbAsp, "USBasp", programmer: "usbasp",
        preferredDriver: "libusbK", requiresComPort: false, isEepromFlashable: true);
