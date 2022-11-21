import Foundation

class CaterinaDevice: BootloaderDevice {
    private var serialPort: String?

    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "Caterina"
        type = .caterina
        eepromFlashable = true
        while (serialPort == nil) {
            serialPort = findSerialPort()
        }
    }

    override func flash(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "avr109", "-U", "flash:w:\(file):i", "-P", serialPort!, "-C", "avrdude.conf"])
    }

    override func flashEEPROM(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "avr109", "-U", "eeprom:w:\(file):i", "-P", serialPort!, "-C", "avrdude.conf"])
    }

    override var description: String {
        "\(super.description) [\(serialPort!)]"
    }
}
