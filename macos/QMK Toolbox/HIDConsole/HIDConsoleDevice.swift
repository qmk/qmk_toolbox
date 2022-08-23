import Foundation

protocol HIDConsoleDeviceDelegate: AnyObject {
    func consoleDevice(_ device: HIDConsoleDevice, didReceiveReport report: String)
}

class HIDConsoleDevice: Equatable, CustomStringConvertible {
    weak var delegate: HIDConsoleDeviceDelegate?

    private var reportBuffer: UnsafeMutablePointer<UInt8>

    private var reportBufferSize: Int = 0

    let hidDevice: IOHIDDevice

    var manufacturer: String? {
        HIDConsoleDevice.stringProperty(kIOHIDManufacturerKey, for: hidDevice)
    }

    var product: String? {
        HIDConsoleDevice.stringProperty(kIOHIDProductKey, for: hidDevice)
    }

    var vendorID: UInt16 {
        HIDConsoleDevice.uint16Property(kIOHIDVendorIDKey, for: hidDevice)
    }

    var productID: UInt16 {
        HIDConsoleDevice.uint16Property(kIOHIDProductIDKey, for: hidDevice)
    }

    var revisionBCD: UInt16 {
        HIDConsoleDevice.uint16Property(kIOHIDVersionNumberKey, for: hidDevice)
    }

    init(_ device: IOHIDDevice) {
        hidDevice = device
        reportBufferSize = IOHIDDeviceGetProperty(device, kIOHIDMaxInputReportSizeKey as CFString) as! Int
        reportBuffer = UnsafeMutablePointer<UInt8>.allocate(capacity: reportBufferSize)

        let inputReportCallback: IOHIDReportCallback = { context, result, sender, type, reportID, report, length in
            let device = Unmanaged<HIDConsoleDevice>.fromOpaque(context!).takeUnretainedValue()
            let reportData = Data(bytes: report, count: device.reportBufferSize)
            device.reportReceived(reportData)
        }

        let unsafeSelf = Unmanaged.passRetained(self).toOpaque()
        IOHIDDeviceRegisterInputReportCallback(hidDevice, reportBuffer, reportBufferSize, inputReportCallback, unsafeSelf)
    }

    private var currentLine = Data()

    func reportReceived(_ report: Data) {
        // Check if we have a completed line queued
        var lineEnd = currentLine.firstIndex(of: UInt8(ascii: "\n"))
        if lineEnd == nil {
            // Partial line or nothing - append incoming report to current line
            for b in report {
                // Trim trailing null bytes
                if b == 0 {
                    break
                }
                currentLine.append(b)
            }
        }

        // Check again for a completed line
        lineEnd = currentLine.firstIndex(of: UInt8(ascii: "\n"))
        while lineEnd != nil {
            // Fire delegate with completed lines until we have none left
            // Only convert to string at the last possible moment in case there is a UTF-8 sequence split across reports
            guard let completedLine = String(data: currentLine[..<lineEnd!], encoding: .utf8) else { return }
            currentLine = currentLine[currentLine.index(after: lineEnd!)...]
            lineEnd = currentLine.firstIndex(of: UInt8(ascii: "\n"))
            delegate?.consoleDevice(self, didReceiveReport: completedLine)
        }
    }

    var description: String {
        String(format: "%@ %@ (%04X:%04X:%04X)", manufacturer ?? "", product ?? "", vendorID, productID, revisionBCD)
    }

    static func == (lhs: HIDConsoleDevice, rhs: HIDConsoleDevice) -> Bool {
        return lhs.hidDevice === rhs.hidDevice
    }

    static func stringProperty(_ propertyName: String, for device: IOHIDDevice) -> String? {
        return IOHIDDeviceGetProperty(device, propertyName as CFString) as! String?
    }

    static func uint16Property(_ propertyName: String, for device: IOHIDDevice) -> UInt16 {
        return (IOHIDDeviceGetProperty(device, propertyName as CFString) as! NSNumber?)!.uint16Value
    }
}
