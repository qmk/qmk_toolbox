import Foundation

@objc
public class USBTinyISPDevice: BootloaderDevice {
    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "USBTinyISP"
        type = .usbTinyIsp
        eepromFlashable = true
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "usbtiny", "-U", "flash:w:\(file):i", "-C", "avrdude.conf"])
    }

    @objc
    public override func flashEEPROM(_ mcu: String, file: String) {
        runProcess("avrdude", args: ["-p", mcu, "-c", "usbtiny", "-U", "eeprom:w:\(file):i", "-C", "avrdude.conf"])
    }
}
