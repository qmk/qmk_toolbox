import Foundation
import IOKit.hid

let CONSOLE_USAGE_PAGE: UInt16 = 0xFF31
let CONSOLE_USAGE: UInt16      = 0x0074
let RAW_USAGE_PAGE: UInt16     = 0xFF60
let RAW_USAGE: UInt16          = 0x0061

protocol HIDListenerDelegate: AnyObject {
    func hidDeviceDidConnect(_ device: HIDDevice)

    func hidDeviceDidDisconnect(_ device: HIDDevice)

    func consoleDevice(_ device: HIDConsoleDevice, didReceiveReport report: String)
}

class HIDListener: HIDConsoleDeviceDelegate {
    weak var delegate: HIDListenerDelegate?

    private var hidManager: IOHIDManager

    var devices: [HIDDevice] = []

    init() {
        hidManager = IOHIDManagerCreate(kCFAllocatorDefault, IOOptionBits(kIOHIDOptionsTypeNone))
        IOHIDManagerSetDeviceMatching(hidManager, nil)
    }

    func start() {
        IOHIDManagerScheduleWithRunLoop(hidManager, RunLoop.current.getCFRunLoop(), CFRunLoopMode.defaultMode.rawValue)
        IOHIDManagerOpen(hidManager, IOOptionBits(kIOHIDOptionsTypeNone))

        let matchingCallback: IOHIDDeviceCallback = { context, result, sender, device in
            let listener: HIDListener = Unmanaged<HIDListener>.fromOpaque(context!).takeUnretainedValue()
            listener.deviceConnected(device)
        }

        let removalCallback: IOHIDDeviceCallback = { context, result, sender, device in
            let listener: HIDListener = Unmanaged<HIDListener>.fromOpaque(context!).takeUnretainedValue()
            listener.deviceDisconnected(device)
        }

        let unsafeSelf = Unmanaged.passRetained(self).toOpaque()
        IOHIDManagerRegisterDeviceMatchingCallback(hidManager, matchingCallback, unsafeSelf)
        IOHIDManagerRegisterDeviceRemovalCallback(hidManager, removalCallback, unsafeSelf)
    }

    func deviceConnected(_ device: IOHIDDevice) {
        if devices.contains(where: { $0.hidDevice === device }) {
            return
        }

        guard let hidDevice = createDevice(device) else {
            return
        }

        devices.append(hidDevice)

        if hidDevice is HIDConsoleDevice {
            (hidDevice as! HIDConsoleDevice).delegate = self
        }

        delegate?.hidDeviceDidConnect(hidDevice)
    }

    func deviceDisconnected(_ device: IOHIDDevice) {
        let discardedItems = devices.filter { $0.hidDevice === device }
        devices = devices.filter { !discardedItems.contains($0) }

        for d in discardedItems {
            delegate?.hidDeviceDidDisconnect(d)
        }
    }

    func consoleDevice(_ device: HIDConsoleDevice, didReceiveReport report: String) {
        delegate?.consoleDevice(device, didReceiveReport: report)
    }

    func stop() {
        let unsafeSelf = Unmanaged.passRetained(self).toOpaque()
        IOHIDManagerRegisterDeviceMatchingCallback(hidManager, nil, unsafeSelf)
        IOHIDManagerRegisterDeviceRemovalCallback(hidManager, nil, unsafeSelf)
        IOHIDManagerUnscheduleFromRunLoop(hidManager, CFRunLoopGetCurrent(), CFRunLoopMode.defaultMode.rawValue)
        IOHIDManagerClose(hidManager, IOOptionBits(kIOHIDOptionsTypeNone))
    }

    func createDevice(_ d: IOHIDDevice) -> HIDDevice? {
        let usagePage = HIDDevice.uint16Property(kIOHIDPrimaryUsagePageKey, for: d)
        let usage = HIDDevice.uint16Property(kIOHIDPrimaryUsageKey, for: d)

        if usagePage == CONSOLE_USAGE_PAGE && usage == CONSOLE_USAGE {
            return HIDConsoleDevice(d)
        } else if usagePage == RAW_USAGE_PAGE && usage == RAW_USAGE {
            return RawDevice(d)
        }

        return nil
    }
}
