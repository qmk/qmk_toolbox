import Foundation

class STM32DFUDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "STM32 DFU"
        type = .stm32Dfu
        resettable = true
    }

    override func flash(_ mcu: String, file: String) {
        guard file.lowercased().hasSuffix(".bin") else {
            print(message: "Only firmware files in .bin format can be flashed with dfu-util!", type: .error)
            return
        }

        runProcess("dfu-util", args: ["-a", "0", "-d", "0483:DF11", "-s", "0x08000000:leave", "-D", file])
    }

    override func reset(_ mcu: String) {
        runProcess("dfu-util", args: ["-a", "0", "-d", "0483:DF11", "-s", "0x08000000:leave"])
    }
}
