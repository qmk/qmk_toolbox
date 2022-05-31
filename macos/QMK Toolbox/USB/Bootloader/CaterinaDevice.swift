import Foundation

@objc
public class CaterinaDevice: BootloaderDevice {
    private var serialPort: String?

    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "Caterina"
        type = .caterina
        eepromFlashable = true
        while (serialPort == nil) {
            serialPort = findSerialPort()
        }
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "avr109", "-U", "flash:w:\(file):i", "-P", serialPort!, "-C", "avrdude.conf"])
    }

    @objc
    public override func flashEEPROM(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "avr109", "-U", "eeprom:w:\(file):i", "-P", serialPort!, "-C", "avrdude.conf"])
    }

    @objc
    public override var description: String {
        "\(super.description) [\(serialPort!)]"
    }
}
