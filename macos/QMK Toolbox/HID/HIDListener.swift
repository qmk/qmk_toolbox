import Foundation
import IOKit.hid

let CONSOLE_USAGE_PAGE: UInt16 = 0xFF31
let CONSOLE_USAGE: UInt16      = 0x0074

protocol HIDListenerDelegate: AnyObject {
    func hidDeviceDidConnect(_ device: HIDConsoleDevice)

    func hidDeviceDidDisconnect(_ device: HIDConsoleDevice)

    func consoleDevice(_ device: HIDConsoleDevice, didReceiveReport report: String)
}

class HIDListener: HIDConsoleDeviceDelegate {
    weak var delegate: HIDListenerDelegate?

    private var hidManager: IOHIDManager

    var devices: [HIDConsoleDevice] = []

    init() {
        hidManager = IOHIDManagerCreate(kCFAllocatorDefault, IOOptionBits(kIOHIDOptionsTypeNone))
        let consoleMatcher = [kIOHIDDeviceUsagePageKey: CONSOLE_USAGE_PAGE, kIOHIDDeviceUsageKey: CONSOLE_USAGE]
        IOHIDManagerSetDeviceMatching(hidManager, consoleMatcher as CFDictionary?)
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

        let consoleDevice = HIDConsoleDevice(device)
        consoleDevice.delegate = self
        devices.append(consoleDevice)
        delegate?.hidDeviceDidConnect(consoleDevice)
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
}
