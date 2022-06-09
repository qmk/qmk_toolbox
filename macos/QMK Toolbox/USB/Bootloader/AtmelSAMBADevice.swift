import Foundation

class AtmelSAMBADevice: BootloaderDevice {
    private var serialPort: String?

    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "Atmel SAM-BA"
        type = .atmelSamBa
        resettable = true
        while serialPort == nil {
            serialPort = findSerialPort()
        }
    }

    override func flash(_ mcu: String, file: String) {
        runProcess("mdloader", args: ["-p", serialPort!, "-D", file, "--restart"])
    }

    override func reset(_ mcu: String) {
        runProcess("mdloader", args: ["-p", serialPort!, "--restart"])
    }

    override var description: String {
        "\(super.description) [\(serialPort!)]"
    }
}
