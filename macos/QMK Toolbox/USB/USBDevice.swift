import Foundation
import IOKit.usb

protocol USBDeviceProtocol {
    var service: io_service_t { get }

    var manufacturer: String? { get }
    var product: String? { get }

    var vendorID: UInt16 { get }
    var productID: UInt16 { get }
    var revisionBCD: UInt16 { get }
}

class USBDevice: USBDeviceProtocol, CustomStringConvertible {
    let service: io_service_t

    var manufacturer: String? {
        stringProperty(kUSBVendorString, service: service)
    }
    var product: String? {
        stringProperty(kUSBProductString, service: service)
    }

    var vendorID: UInt16 {
        vendorID(service: service)
    }
    var productID: UInt16 {
        productID(service: service)
    }
    var revisionBCD: UInt16 {
        ushortProperty(kUSBDeviceReleaseNumber, service: service)
    }

    init(service: io_service_t) {
        self.service = service
    }

    func stringProperty(_ property: String, service: io_service_t) -> String? {
        let cfProperty = IORegistryEntrySearchCFProperty(service, kIOServicePlane, property as CFString, kCFAllocatorDefault, IOOptionBits(kIORegistryIterateParents | kIORegistryIterateRecursively))
        return (cfProperty as? NSString) as? String
    }

    func ushortProperty(_ property: String, service: io_service_t) -> UInt16 {
        let cfProperty = IORegistryEntrySearchCFProperty(service, kIOServicePlane, property as CFString, kCFAllocatorDefault, IOOptionBits(kIORegistryIterateParents | kIORegistryIterateRecursively))
        return (cfProperty as? NSNumber)?.uint16Value ?? 0
    }

    func vendorID(service: io_service_t) -> UInt16 {
        ushortProperty(kUSBVendorID, service: service)
    }

    func productID(service: io_service_t) -> UInt16 {
        ushortProperty(kUSBProductID, service: service)
    }

    var description: String {
        String(format: "%@ %@ (%04X:%04X:%04X)", manufacturer ?? "", product ?? "", vendorID, productID, revisionBCD)
    }
}
