import Foundation

class WB32DFUDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "WB32 DFU"
        type = .wb32Dfu
        resettable = true
    }

    override func flash(_ mcu: String, file: String) {
        if file.lowercased().hasSuffix(".bin") {
            runProcess("wb32-dfu-updater_cli", args: ["--toolbox-mode", "--dfuse-address", "0x08000000", "--download", file])
        } else {
            runProcess("wb32-dfu-updater_cli", args: ["--toolbox-mode", "--download", file])
        }
    }

    override func reset(_ mcu: String) {
        runProcess("wb32-dfu-updater_cli", args: ["--reset"])
    }
}
