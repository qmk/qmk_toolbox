import Foundation

@objc
public enum BootloaderType: Int {
    case apm32Dfu = 0
    case atmelDfu = 1
    case atmelSamBa = 2
    case avrIsp = 3
    case bootloadHid = 4
    case caterina = 5
    case gd32vDfu = 6
    case halfKay = 7
    case kiibohdDfu = 8
    case lufaHid = 9
    case lufaMs = 10
    case qmkDfu = 11
    case qmkHid = 12
    case stm32Dfu = 13
    case stm32duino = 14
    case usbAsp = 15
    case usbTinyIsp = 16
    case wb32Dfu = 17
    case none = 18
}
