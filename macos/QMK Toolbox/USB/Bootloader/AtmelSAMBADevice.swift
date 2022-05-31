import Foundation

@objc
public class AtmelSAMBADevice: BootloaderDevice {
    private var serialPort: String?

    @objc
    public override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "Atmel SAM-BA"
        type = .atmelSamBa
        resettable = true
        while serialPort == nil {
            serialPort = findSerialPort()
        }
    }

    @objc
    public override func flash(_ mcu: String, file: String) {
        runProcess("mdloader", args: ["-p", serialPort!, "-D", file, "--restart"])
    }

    @objc
    public override func reset(_ mcu: String) {
        runProcess("mdloader", args: ["-p", serialPort!, "--restart"])
    }

    @objc
    public override var description: String {
        "\(super.description) [\(serialPort!)]"
    }
}
