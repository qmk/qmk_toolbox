import Foundation

class BootloadHIDDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "BootloadHID"
        type = .bootloadHid
        resettable = true
    }

    override func flash(_ mcu: String, file: String) {
        runProcess("bootloadHID", args: ["-r", file])
    }

    override func reset(_ mcu: String) {
        runProcess("bootloadHID", args: ["-r"])
    }
}
