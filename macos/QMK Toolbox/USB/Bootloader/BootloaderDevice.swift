import Foundation
import IOKit.serial

@objc
public protocol BootloaderDeviceDelegate {
    @objc(bootloaderDevice:didReceiveCommandOutput:messageType:)
    func bootloaderDevice(_ device: BootloaderDevice, didReceiveCommandOutput data: String, type: MessageType)
}

@objc
public class BootloaderDevice: NSObject, USBDeviceProtocol {
    @objc
    public weak var delegate: BootloaderDeviceDelegate?

    @objc
    public var usbDevice: USBDevice

    @objc
    public var name: String = ""

    @objc
    public var type: BootloaderType = .none

    @objc
    public var eepromFlashable: Bool = false

    @objc
    public var resettable: Bool = false

    @objc
    public var service: io_service_t {
        usbDevice.service
    }

    @objc(manufacturerString)
    public var manufacturer: String? {
        usbDevice.manufacturer
    }

    @objc(productString)
    public var product: String? {
        usbDevice.product
    }

    @objc
    public var vendorID: UInt16 {
        usbDevice.vendorID
    }

    @objc
    public var productID: UInt16 {
        usbDevice.productID
    }

    @objc
    public var revisionBCD: UInt16 {
        usbDevice.revisionBCD
    }

    @objc(initWithUSBDevice:)
    public init(usbDevice: USBDevice) {
        self.usbDevice = usbDevice
    }

    @objc(flashWithMCU:file:)
    public func flash(_ mcu: String, file: String) {}

    @objc(flashEEPROMWithMCU:file:)
    public func flashEEPROM(_ mcu: String, file: String) {}

    @objc(resetWithMCU:)
    public func reset(_ mcu: String) {}

    @objc(runProcess:withArgs:)
    public func runProcess(_ command: String, args: [String]) {
        print(message: "\(command) \(args.joined(separator: " "))", type: .command)

        let task = Process()
        task.executableURL = Bundle.main.url(forResource: command, withExtension: nil)
        task.currentDirectoryURL = Bundle.main.resourceURL
        task.arguments = args

        let outPipe = Pipe()
        task.standardOutput = outPipe
        let errPipe = Pipe()
        task.standardError = errPipe

        let group = DispatchGroup()

        group.enter()
        outPipe.fileHandleForReading.readabilityHandler = { handle in
            let data = handle.readData(ofLength: Int.max)
            guard data.count > 0 else {
                handle.readabilityHandler = nil
                group.leave()
                return
            }

            self.printOutput(String(decoding: data, as: UTF8.self))
        }

        group.enter()
        errPipe.fileHandleForReading.readabilityHandler = { handle in
            let data = handle.readData(ofLength: Int.max)
            guard data.count > 0 else {
                handle.readabilityHandler = nil
                group.leave()
                return
            }

            self.printErrorOutput(String(decoding: data, as: UTF8.self))
        }

        do {
            try task.run()
        } catch let error {
            print(message: error.localizedDescription, type: .error)
            return
        }

        group.wait()
    }

    func printOutput(_ output: String) {
        print(message: output, type: .commandOutput)
    }

    func printErrorOutput(_ output: String) {
        print(message: output, type: .commandError)
    }

    @objc
    public func print(message: String, type: MessageType) {
        delegate?.bootloaderDevice(self, didReceiveCommandOutput: message, type: type)
    }

    @objc
    public override var description: String {
        usbDevice.description
    }

    @objc
    public func findSerialPort() -> String? {
        let serialMatcher = IOServiceMatching(kIOSerialBSDServiceValue)
        var serialIterator: io_iterator_t = 0

        guard IOServiceGetMatchingServices(kIOMasterPortDefault, serialMatcher, &serialIterator) == KERN_SUCCESS else { return nil }

        repeat {
            let port = IOIteratorNext(serialIterator)
            guard port != 0 else { break }

            let parentVendorID = usbDevice.vendorID(service: port)
            let parentProductID = usbDevice.productID(service: port)

            if parentVendorID == vendorID && parentProductID == productID {
                return usbDevice.stringProperty(kIOCalloutDeviceKey, service: port)
            }
        } while true

        return nil
    }
}
