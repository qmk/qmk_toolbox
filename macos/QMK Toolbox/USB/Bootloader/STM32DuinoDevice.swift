import Foundation

@objc
public class STM32DuinoDevice: BootloaderDevice {
    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "STM32Duino"
        type = .stm32duino
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        guard file.lowercased().hasSuffix(".bin") else {
            print(message: "Only firmware files in .bin format can be flashed with dfu-util!", type: .error)
            return
        }

        runProcess("dfu-util", args: ["-a", "2", "-d", "1EAF:0003", "-R", "-D", file])
    }
}
