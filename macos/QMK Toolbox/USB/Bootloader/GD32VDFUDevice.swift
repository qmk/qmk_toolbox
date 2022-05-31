import Foundation

@objc
public class GD32VDFUDevice: BootloaderDevice {
    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "GD32V DFU"
        type = .gd32vDfu
        resettable = true
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        guard file.lowercased().hasSuffix(".bin") else {
            print(message: "Only firmware files in .bin format can be flashed with dfu-util!", type: .error)
            return
        }

        runProcess("dfu-util", args: ["-a", "0", "-d", "28E9:0189", "-s", "0x08000000:leave", "-D", file])
    }

    @objc
    public override func reset(_ mcu: String) {
        runProcess("dfu-util", args: ["-a", "0", "-d", "28E9:0189", "-s", "0x08000000:leave"])
    }
}
