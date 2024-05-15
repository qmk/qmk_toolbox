import Foundation

class HIDDevice: Equatable, CustomStringConvertible {
    let hidDevice: IOHIDDevice

    var usagePage: UInt16 {
        HIDDevice.uint16Property(kIOHIDDeviceUsagePageKey, for: hidDevice)!
    }

    var usage: UInt16 {
        HIDDevice.uint16Property(kIOHIDDeviceUsageKey, for: hidDevice)!
    }

    var manufacturer: String? {
        HIDDevice.stringProperty(kIOHIDManufacturerKey, for: hidDevice)
    }

    var product: String? {
        HIDDevice.stringProperty(kIOHIDProductKey, for: hidDevice)
    }

    var vendorID: UInt16 {
        HIDDevice.uint16Property(kIOHIDVendorIDKey, for: hidDevice)!
    }

    var productID: UInt16 {
        HIDDevice.uint16Property(kIOHIDProductIDKey, for: hidDevice)!
    }

    var revisionBCD: UInt16 {
        HIDDevice.uint16Property(kIOHIDVersionNumberKey, for: hidDevice)!
    }

    init(_ device: IOHIDDevice) {
        hidDevice = device
    }

    var description: String {
        String(format: "%@ %@ (%04X:%04X:%04X)", manufacturer ?? "", product ?? "", vendorID, productID, revisionBCD)
    }

    static func == (lhs: HIDDevice, rhs: HIDDevice) -> Bool {
        return lhs.hidDevice === rhs.hidDevice
    }

    static func stringProperty(_ propertyName: String, for device: IOHIDDevice) -> String? {
        return IOHIDDeviceGetProperty(device, propertyName as CFString) as! String?
    }

    static func uint16Property(_ propertyName: String, for device: IOHIDDevice) -> UInt16? {
        return (IOHIDDeviceGetProperty(device, propertyName as CFString) as! NSNumber?)!.uint16Value
    }
}
