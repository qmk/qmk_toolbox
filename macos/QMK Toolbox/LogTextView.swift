import Cocoa

class LogTextView: NSTextView {
    override func awakeFromNib() {
        selectedTextAttributes = [
            .backgroundColor: NSColor(named: "LogBoxSelection")!,
            .foregroundColor: NSColor.white
        ]
    }

    func logBootloader(_ message: String) {
        log(message, type: .bootloader)
    }

    func logCommand(_ message: String) {
        log(message, type: .command)
    }

    func logCommandError(_ message: String) {
        log(message, type: .commandError)
    }

    func logCommandOutput(_ message: String) {
        log(message, type: .commandOutput)
    }

    func logError(_ message: String) {
        log(message, type: .error)
    }

    func logHID(_ message: String) {
        log(message, type: .hid)
    }

    func logHIDOutput(_ message: String) {
        log(message, type: .hidOutput)
    }

    func logInfo(_ message: String) {
        log(message, type: .info)
    }

    func logUSB(_ message: String) {
        log(message, type: .usb)
    }

    func log(_ message: String, type: MessageType) {
        var trimmedMessage = message
        if trimmedMessage.last == "\n" {
            trimmedMessage.removeLast()
        }

        trimmedMessage.split(separator: "\n").forEach { line in
            switch type {
            case .bootloader:
                append(string: "\(line)\n", color: NSColor(named: "LogMessageBootloader")!)
            case .command:
                append(string: "> \(line)\n", color: NSColor(named: "LogMessageDefault")!)
            case .commandError:
                append(string: "> ", color: NSColor(named: "LogMessageError")!)
                append(string: "\(line)\n", color: NSColor(named: "LogMessageInfo")!)
            case .commandOutput:
                append(string: "> ", color: NSColor(named: "LogMessageDefault")!)
                append(string: "\(line)\n", color: NSColor(named: "LogMessageInfo")!)
            case .error:
                append(string: "\(line)\n", color: NSColor(named: "LogMessageError")!)
            case .hid:
                append(string: "\(line)\n", color: NSColor(named: "LogMessageHID")!)
            case .hidOutput:
                append(string: "> ", color: NSColor(named: "LogMessageHID")!)
                append(string: "\(line)\n", color: NSColor(named: "LogMessageHIDOutput")!)
            case .info:
                append(string: "* ", color: NSColor(named: "LogMessageDefault")!)
                append(string: "\(line)\n", color: NSColor(named: "LogMessageInfo")!)
            case .usb:
                append(string: "\(line)\n", color: NSColor(named: "LogMessageDefault")!)
            }
        }
    }

    func append(string: String, color: NSColor) {
        textStorage?.append(NSMutableAttributedString(string: string, attributes: [
            .foregroundColor: color,
            .font: NSFont.userFixedPitchFont(ofSize: 12)!
        ]))
        scrollToEndOfDocument(self)
    }
}
