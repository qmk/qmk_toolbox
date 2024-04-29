import Foundation

protocol HIDConsoleDeviceDelegate: AnyObject {
    func consoleDevice(_ device: HIDConsoleDevice, didReceiveReport report: String)
}

class HIDConsoleDevice: HIDDevice {
    weak var delegate: HIDConsoleDeviceDelegate?

    private var reportBuffer: UnsafeMutablePointer<UInt8>

    private var reportBufferSize: Int = 0

    override init(_ device: IOHIDDevice) {
        reportBufferSize = IOHIDDeviceGetProperty(device, kIOHIDMaxInputReportSizeKey as CFString) as! Int
        reportBuffer = UnsafeMutablePointer<UInt8>.allocate(capacity: reportBufferSize)

        let inputReportCallback: IOHIDReportCallback = { context, result, sender, type, reportID, report, length in
            let device = Unmanaged<HIDConsoleDevice>.fromOpaque(context!).takeUnretainedValue()
            let reportData = Data(bytes: report, count: device.reportBufferSize)
            device.reportReceived(reportData)
        }

        super.init(device)

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
}
