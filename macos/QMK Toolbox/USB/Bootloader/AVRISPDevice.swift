import Foundation

@objc
public class AVRISPDevice: BootloaderDevice {
    private var serialPort: String?

    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "AVR ISP"
        type = .avrIsp
        eepromFlashable = true
        while serialPort == nil {
            serialPort = findSerialPort()
        }
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "avrisp", "-U", "flash:w:\(file):i", "-P", serialPort!, "-C", "avrdude.conf"])
    }

    @objc
    public override func flashEEPROM(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "avrisp", "-U", "eeprom:w:\(file):i", "-P", serialPort!, "-C", "avrdude.conf"])
    }

    @objc
    public override var description: String {
        "\(super.description) [\(serialPort!)]"
    }
}
