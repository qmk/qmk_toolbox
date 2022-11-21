import Foundation

class HalfKayDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "HalfKay"
        type = .halfKay
        resettable = true
    }

    override func flash(_ mcu: String, file: String) {
        runProcess("teensy_loader_cli", args: ["-mmcu=\(mcu)", file, "-v"])
    }

    override func reset(_ mcu: String) {
        runProcess("teensy_loader_cli", args: ["-mmcu=\(mcu)", "-bv"])
    }
}
