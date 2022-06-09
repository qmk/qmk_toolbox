import Foundation

class AtmelDFUDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        if revisionBCD == 0x0936 {
            name = "QMK DFU"
            type = .qmkDfu
        } else {
            name = "Atmel DFU"
            type = .atmelDfu
        }
        eepromFlashable = true
        resettable = true
    }

    override func flash(_ mcu: String, file: String) {
        runProcess("dfu-programmer", args: [mcu, "erase", "--force"])
        runProcess("dfu-programmer", args: [mcu, "flash", "--force", file])
        runProcess("dfu-programmer", args: [mcu, "reset"])
    }

    override func flashEEPROM(_ mcu: String, file: String) {
        if type == .atmelDfu {
            runProcess("dfu-programmer", args: [mcu, "erase", "--force"])
        }
        runProcess("dfu-programmer", args: [mcu, "flash", "--force", "--suppress-validation", "--eeprom", file])
        if type == .atmelDfu {
            print(message: "Please reflash device with firmware now", type: .bootloader)
        }
    }

    override func reset(_ mcu: String) {
        runProcess("dfu-programmer", args: [mcu, "reset"])
    }
}
