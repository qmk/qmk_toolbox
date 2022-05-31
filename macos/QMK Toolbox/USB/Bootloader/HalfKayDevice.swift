import Foundation

@objc
public class HalfKayDevice: BootloaderDevice {
    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "HalfKay"
        type = .halfKay
        resettable = true
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        runProcess("teensy_loader_cli", args: ["-mmcu=\(mcu)", file, "-v"])
    }

    @objc
    public override func reset(_ mcu: String) {
        runProcess("teensy_loader_cli", args: ["-mmcu=\(mcu)", "-bv"])
    }
}
