import Foundation

class USBTinyISPDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "USBTinyISP"
        type = .usbTinyIsp
        eepromFlashable = true
    }

    override func flash(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "usbtiny", "-U", "flash:w:\(file):i", "-C", "avrdude.conf"])
    }

    override func flashEEPROM(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "usbtiny", "-U", "eeprom:w:\(file):i", "-C", "avrdude.conf"])
    }
}
