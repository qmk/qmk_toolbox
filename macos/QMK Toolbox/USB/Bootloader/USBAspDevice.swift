import Foundation

@objc
public class USBAspDevice: BootloaderDevice {
    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "USBasp"
        type = .usbAsp
        eepromFlashable = true
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "usbasp", "-U", "flash:w:\(file):i", "-C", "avrdude.conf"])
    }

    @objc
    public override func flashEEPROM(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "usbasp", "-U", "eeprom:w:\(file):i", "-C", "avrdude.conf"])
    }
}
