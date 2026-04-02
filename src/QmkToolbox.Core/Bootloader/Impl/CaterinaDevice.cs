using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>Caterina bootloader device (Arduino/Pro Micro, via avrdude with avr109 programmer).</summary>
internal sealed class CaterinaDevice(IUsbDevice device, IFlashToolProvider toolProvider, ISerialPortService? serialPortService = null)
    : AvrdudeDevice(device, toolProvider, serialPortService,
        BootloaderType.Caterina, "Caterina", programmer: "avr109",
        preferredDriver: "usbser", requiresComPort: true, isEepromFlashable: true);
