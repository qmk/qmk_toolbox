import Foundation

enum MessageType {
    case bootloader
    case command
    case commandError
    case commandOutput
    case error
    case hid
    case hidOutput
    case info
    case usb
}
