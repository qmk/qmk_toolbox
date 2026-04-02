using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>USBtinyISP flasher device (via avrdude with usbtiny programmer).</summary>
internal sealed class UsbTinyIspDevice(IUsbDevice device, IFlashToolProvider toolProvider)
    : AvrdudeDevice(device, toolProvider, serialPortService: null,
        BootloaderType.UsbTinyIsp, "USBtinyISP", programmer: "usbtiny",
        preferredDriver: "libusb0", requiresComPort: false, isEepromFlashable: true);
