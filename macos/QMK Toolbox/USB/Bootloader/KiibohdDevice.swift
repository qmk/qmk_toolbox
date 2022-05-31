import Foundation

@objc
public class KiibohdDFUDevice: BootloaderDevice {
    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "Kiibohd DFU"
        type = .kiibohdDfu
        resettable = true
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        guard file.lowercased().hasSuffix(".bin") else {
            print(message: "Only firmware files in .bin format can be flashed with dfu-util!", type: .error)
            return
        }

        runProcess("dfu-util", args: ["-a", "0", "-d", "1C11:B007", "-D", file])
    }

    @objc
    public override func reset(_ mcu: String) {
        runProcess("dfu-util", args: ["-a", "0", "-d", "1C11:B007", "-e"])
    }
}
