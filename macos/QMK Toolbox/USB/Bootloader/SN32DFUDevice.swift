import Foundation

class SN32DFUDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "SN32 DFU"
        type = .sn32Dfu
    }

    override func flash(_ mcu: String, file: String) {
        if productID == 0x7010 {
            runProcess("sonixflasher", args: ["-v", "\(vendorID):\(productID)", "-o", "0x200", "-f", file])
        } else {
            runProcess("sonixflasher", args: ["-v", "\(vendorID):\(productID)", "-f", file])
        }
    }
}
