import Foundation
import IOKit.usb

@objc
protocol USBDeviceProtocol: NSObjectProtocol {
    @objc
    var service: io_service_t { get }

    @objc(manufacturerString)
    var manufacturer: String? { get }
    @objc(productString)
    var product: String? { get }

    @objc
    var vendorID: UInt16 { get }
    @objc
    var productID: UInt16 { get }
    @objc
    var revisionBCD: UInt16 { get }
}

@objc
public class USBDevice: NSObject, USBDeviceProtocol {
    @objc
    var service: io_service_t

    @objc(manufacturerString)
    var manufacturer: String? {
        stringProperty(kUSBVendorString, service: service)
    }
    @objc(productString)
    var product: String? {
        stringProperty(kUSBProductString, service: service)
    }

    @objc
    var vendorID: UInt16 {
        vendorID(service: service)
    }
    @objc
    var productID: UInt16 {
        productID(service: service)
    }
    @objc
    var revisionBCD: UInt16 {
        ushortProperty(kUSBDeviceReleaseNumber, service: service)
    }

    @objc
    init(service: io_service_t) {
        self.service = service
    }

    @objc(stringProperty:forService:)
    public func stringProperty(_ property: String, service: io_service_t) -> String? {
        let cfProperty = IORegistryEntrySearchCFProperty(service, kIOServicePlane, property as CFString, kCFAllocatorDefault, IOOptionBits(kIORegistryIterateParents | kIORegistryIterateRecursively))
        return (cfProperty as? NSString) as? String
    }

    func ushortProperty(_ property: String, service: io_service_t) -> UInt16 {
        let cfProperty = IORegistryEntrySearchCFProperty(service, kIOServicePlane, property as CFString, kCFAllocatorDefault, IOOptionBits(kIORegistryIterateParents | kIORegistryIterateRecursively))
        return (cfProperty as? NSNumber)?.uint16Value ?? 0
    }

    @objc(vendorIDForService:)
    func vendorID(service: io_service_t) -> UInt16 {
        ushortProperty(kUSBVendorID, service: service)
    }

    @objc(productIDForService:)
    func productID(service: io_service_t) -> UInt16 {
        ushortProperty(kUSBProductID, service: service)
    }

    @objc
    public override var description: String {
        String(format: "%@ %@ (%04X:%04X:%04X)", manufacturer ?? "", product ?? "", vendorID, productID, revisionBCD)
    }
}
