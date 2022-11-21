import Foundation

class USBAspDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "USBasp"
        type = .usbAsp
        eepromFlashable = true
    }

    override func flash(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "usbasp", "-U", "flash:w:\(file):i", "-C", "avrdude.conf"])
    }

    override func flashEEPROM(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "usbasp", "-U", "eeprom:w:\(file):i", "-C", "avrdude.conf"])
    }
}
