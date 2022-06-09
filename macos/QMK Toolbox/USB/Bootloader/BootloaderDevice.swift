import Foundation
import IOKit.serial

protocol BootloaderDeviceDelegate: AnyObject {
    func bootloaderDevice(_ device: BootloaderDevice, didReceiveCommandOutput data: String, type: MessageType)
}

class BootloaderDevice: USBDeviceProtocol, CustomStringConvertible {
    weak var delegate: BootloaderDeviceDelegate?

    let usbDevice: USBDevice

    var name: String = ""

    var type: BootloaderType = .none

    var eepromFlashable: Bool = false

    var resettable: Bool = false

    var service: io_service_t {
        usbDevice.service
    }

    var manufacturer: String? {
        usbDevice.manufacturer
    }

    var product: String? {
        usbDevice.product
    }

    var vendorID: UInt16 {
        usbDevice.vendorID
    }

    var productID: UInt16 {
        usbDevice.productID
    }

    var revisionBCD: UInt16 {
        usbDevice.revisionBCD
    }

    init(usbDevice: USBDevice) {
        self.usbDevice = usbDevice
    }

    func flash(_ mcu: String, file: String) {}

    func flashEEPROM(_ mcu: String, file: String) {}

    func reset(_ mcu: String) {}

    func runProcess(_ command: String, args: [String]) {
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

    func print(message: String, type: MessageType) {
        delegate?.bootloaderDevice(self, didReceiveCommandOutput: message, type: type)
    }

    var description: String {
        usbDevice.description
    }

    func findSerialPort() -> String? {
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
