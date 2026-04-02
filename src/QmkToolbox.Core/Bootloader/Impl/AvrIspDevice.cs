using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>AVR ISP bootloader device (via avrdude with avrisp programmer).</summary>
internal sealed class AvrIspDevice(IUsbDevice device, IFlashToolProvider toolProvider, ISerialPortService? serialPortService = null)
    : AvrdudeDevice(device, toolProvider, serialPortService,
        BootloaderType.AvrIsp, "AVR ISP", programmer: "avrisp",
        preferredDriver: "usbser", requiresComPort: true, isEepromFlashable: false);
