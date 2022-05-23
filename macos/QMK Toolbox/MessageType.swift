import Foundation

@objc
public enum MessageType: Int {
    case bootloader = 0
    case command = 1
    case commandError = 2
    case commandOutput = 3
    case error = 4
    case hid = 5
    case hidOutput = 6
    case info = 7
    case usb = 8
}
