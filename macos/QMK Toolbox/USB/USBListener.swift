import Foundation
import IOKit.usb

protocol USBListenerDelegate: AnyObject {
    func usbDeviceDidConnect(_ device: USBDevice)

    func usbDeviceDidDisconnect(_ device: USBDevice)

    func bootloaderDeviceDidConnect(_ device: BootloaderDevice)

    func bootloaderDeviceDidDisconnect(_ device: BootloaderDevice)

    func bootloaderDevice(_ device: BootloaderDevice, didReceiveCommandOutput data: String, type: MessageType)
}

class USBListener: BootloaderDeviceDelegate {
    weak var delegate: USBListenerDelegate?

    var devices: [USBDeviceProtocol] = []

    private var notificationPort: IONotificationPortRef?

    func bootloaderDevice(_ device: BootloaderDevice, didReceiveCommandOutput data: String, type: MessageType) {
        delegate?.bootloaderDevice(device, didReceiveCommandOutput: data, type: type)
    }

    func start() {
        notificationPort = IONotificationPortCreate(kIOMasterPortDefault)
        let runLoopSource = IONotificationPortGetRunLoopSource(notificationPort).takeUnretainedValue()
        CFRunLoopAddSource(RunLoop.current.getCFRunLoop(), runLoopSource, .defaultMode)

        let matchingCallback: IOServiceMatchingCallback = { context, iterator in
            let listener = Unmanaged<USBListener>.fromOpaque(context!).takeUnretainedValue()
            listener.deviceConnected(iterator)
        }
        let removalCallback: IOServiceMatchingCallback = { context, iterator in
            let listener = Unmanaged<USBListener>.fromOpaque(context!).takeUnretainedValue()
            listener.deviceDisconnected(iterator)
        }

        let unsafeSelf = Unmanaged.passRetained(self).toOpaque()
        let usbMatcher = IOServiceMatching(kIOUSBDeviceClassName) as NSDictionary
        var usbConnectedIter: io_iterator_t = 0
        var usbDisconnectedIter: io_iterator_t = 0

        IOServiceAddMatchingNotification(notificationPort, kIOFirstMatchNotification, usbMatcher, matchingCallback, unsafeSelf, &usbConnectedIter)
        deviceConnected(usbConnectedIter)
        IOServiceAddMatchingNotification(notificationPort, kIOTerminatedNotification, usbMatcher, removalCallback, unsafeSelf, &usbDisconnectedIter)
        deviceDisconnected(usbDisconnectedIter)
    }

    func stop() {
        devices = []
        IONotificationPortDestroy(notificationPort)
    }

    func deviceConnected(_ iterator: io_iterator_t) {
        while case let service = IOIteratorNext(iterator), service != 0 {
            // Skip root hubs and other things that are not actual USB devices
            let className = IOObjectCopyClass(service).takeRetainedValue() as String
            if ![kIOUSBDeviceClassName, kIOUSBHostDeviceClassName].contains(className) {
                continue
            }

            // Skip if the device is already in the list (shouldn't ever happen?)
            if devices.contains(where: { $0.service == service }) {
                continue
            }

            let usbDevice = createDevice(service: service)
            devices.append(usbDevice)
            if usbDevice is BootloaderDevice {
                (usbDevice as! BootloaderDevice).delegate = self
                delegate?.bootloaderDeviceDidConnect(usbDevice as! BootloaderDevice)
            } else {
                delegate?.usbDeviceDidConnect(usbDevice as! USBDevice)
            }
        }
    }

    func deviceDisconnected(_ iterator: io_iterator_t) {
        while case let service = IOIteratorNext(iterator), service != 0 {
            let discardedItems = devices.filter { $0.service == service }
            devices = devices.filter({ d1 in
                !discardedItems.contains(where: { d2 in
                    d1.service == d2.service
                })
            })

            discardedItems.forEach { d in
                if d is BootloaderDevice {
                    delegate?.bootloaderDeviceDidDisconnect(d as! BootloaderDevice)
                } else {
                    delegate?.usbDeviceDidDisconnect(d as! USBDevice)
                }
            }
        }
    }

    func createDevice(service: io_service_t) -> USBDeviceProtocol {
        let usbDevice = USBDevice(service: service)
        let deviceType = deviceType(vendorID: usbDevice.vendorID, productID: usbDevice.productID, revisionBCD: usbDevice.revisionBCD)

        switch deviceType {
        case .apm32Dfu:
            return APM32DFUDevice(usbDevice: usbDevice)
        case .atmelDfu, .qmkDfu:
            return AtmelDFUDevice(usbDevice: usbDevice)
        case .atmelSamBa:
            return AtmelSAMBADevice(usbDevice: usbDevice)
        case .avrIsp:
            return AVRISPDevice(usbDevice: usbDevice)
        case .bootloadHid:
            return BootloadHIDDevice(usbDevice: usbDevice)
        case .caterina:
            return CaterinaDevice(usbDevice: usbDevice)
        case .gd32vDfu:
            return GD32VDFUDevice(usbDevice: usbDevice)
        case .halfKay:
            return HalfKayDevice(usbDevice: usbDevice)
        case .kiibohdDfu:
            return KiibohdDFUDevice(usbDevice: usbDevice)
        case .lufaHid, .qmkHid:
            return LUFAHIDDevice(usbDevice: usbDevice)
        case .lufaMs:
            return LUFAMSDevice(usbDevice: usbDevice)
        case .stm32Dfu:
            return STM32DFUDevice(usbDevice: usbDevice)
        case .stm32duino:
            return STM32DuinoDevice(usbDevice: usbDevice)
        case .usbAsp:
            return USBAspDevice(usbDevice: usbDevice)
        case .usbTinyIsp:
            return USBTinyISPDevice(usbDevice: usbDevice)
        case .wb32Dfu:
            return WB32DFUDevice(usbDevice: usbDevice)
        case .none:
            return usbDevice
        }
    }

    func deviceType(vendorID: UInt16, productID: UInt16, revisionBCD: UInt16) -> BootloaderType {
        switch vendorID {
        case 0x03EB: // Atmel Corporation
            switch productID {
            case 0x2045:
                return .lufaMs
            case 0x2067:
                // Unicode Ψ
                return revisionBCD == 0x0936 ? .qmkHid : .lufaHid
            // ATmega16U2, ATmega32U2, ATmega16U4, ATmega32U4, AT90USB64, AT90USB162, AT90USB128
            case 0x2FEF, 0x2FF0, 0x2FF3, 0x2FF4, 0x2FF9, 0x2FFA, 0x2FFB:
                // Unicode Ψ
                return revisionBCD == 0x0936 ? .qmkDfu : .atmelDfu
            case 0x6124:
                return .atmelSamBa
            default:
                break
            }
        case 0x0483: // STMicroelectronics
            if productID == 0xDF11 {
                return .stm32Dfu
            }
        case 0x1209: // pid.codes
            if productID == 0x2302 { // Keyboardio Atreus 2 Bootloader
                return .caterina
            }
        case 0x16C0: // Van Ooijen Technische Informatica
            switch productID {
            case 0x0478:
                return .halfKay
            case 0x0483:
                return .avrIsp
            case 0x05DC:
                return .usbAsp
            case 0x05DF:
                return .bootloadHid
            default:
                break
            }
        case 0x1781: // MECANIQUE
            if productID == 0x0C9F {
                return .usbTinyIsp
            }
        case 0x1B4F: // Spark Fun Electronics
            switch productID {
            case 0x9203, 0x9205, 0x9207: // Pro Micro 3V3/8MHz, Pro Micro 5V/16MHz, LilyPad 3V3/8MHz (and some Pro Micro clones)
                return .caterina
            default:
                break
            }
        case 0x1C11: // Input Club Inc.
            if productID == 0xB007 {
                return .kiibohdDfu
            }
            break;
        case 0x1EAF: // Leaflabs
            if productID == 0x0003 {
                return .stm32duino
            }
        case 0x1FFB: // Pololu Corporation
            if productID == 0x0101 { // A-Star 32U4
                return .caterina
            }
        case 0x2341, 0x2A03: // Arduino SA, dog hunter AG
            switch productID {
            case 0x0036, 0x0037: // Leonardo, Micro
                return .caterina
            default:
                break
            }
        case 0x239A: // Adafruit
            switch productID {
            case 0x000C, 0x000D, 0x000E: // Feather 32U4, ItsyBitsy 32U4 3V3/8MHz, ItsyBitsy 32U4 5V/16MHz
                return .caterina
            default:
                break
            }
        case 0x28E9:
            if productID == 0x0189 {
                return .gd32vDfu
            }
        case 0x314B: // Geehy Semiconductor Co. Ltd.
            if productID == 0x0106 {
                return .apm32Dfu
            }
        case 0x342D: // WestBerryTech
            if productID == 0xDFA0 {
                return .wb32Dfu
            }
        default:
            break
        }

        return .none
    }
}
