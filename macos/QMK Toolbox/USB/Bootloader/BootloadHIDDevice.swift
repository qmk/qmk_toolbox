import Foundation

@objc
public class BootloadHIDDevice: BootloaderDevice {
    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "BootloadHID"
        type = .bootloadHid
        resettable = true
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        runProcess("bootloadHID", args: ["-r", file])
    }

    @objc
    public override func reset(_ mcu: String) {
        runProcess("bootloadHID", args: ["-r"])
    }
}
