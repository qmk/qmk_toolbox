#import <IOKit/usb/IOUSBLib.h>

#import "USBDevice.h"

@implementation USBDevice

@synthesize service;

@synthesize manufacturerString;

@synthesize productString;

@synthesize vendorID;

@synthesize productID;

@synthesize revisionBCD;

- (id)initWithService:(io_service_t)service {
    if (self = [super init]) {
        self.service = service;

        self.manufacturerString = [self stringProperty:CFSTR(kUSBVendorString) forService:self.service];
        self.productString = [self stringProperty:CFSTR(kUSBProductString) forService:self.service];
        self.vendorID = [self vendorIDForService:self.service];
        self.productID = [self productIDForService:self.service];
        self.revisionBCD = [self ushortProperty:CFSTR(kUSBDeviceReleaseNumber) forService:self.service];
    }
    return self;
}

- (NSString *)stringProperty:(CFStringRef)property forService:(io_service_t)service {
    CFStringRef cfProperty = IORegistryEntryCreateCFProperty(service, property, kCFAllocatorDefault, kNilOptions);
    if (cfProperty != nil) {
        NSString *nsProperty = (__bridge NSString *)(cfProperty);
        CFRelease(cfProperty);
        return nsProperty;
    }
    return nil;
}

- (ushort)ushortProperty:(CFStringRef)property forService:(io_service_t)service {
    CFNumberRef cfProperty = IORegistryEntrySearchCFProperty(service, kIOServicePlane, property, kCFAllocatorDefault, kIORegistryIterateParents | kIORegistryIterateRecursively);
    if (cfProperty != nil) {
        NSNumber *nsProperty = (__bridge NSNumber *)(cfProperty);
        CFRelease(cfProperty);
        return [nsProperty unsignedShortValue];
    }
    return 0;
}

- (ushort)vendorIDForService:(io_service_t)service {
    return [self ushortProperty:CFSTR(kUSBVendorID) forService:service];
}

- (ushort)productIDForService:(io_service_t)service {
    return [self ushortProperty:CFSTR(kUSBProductID) forService:service];
}

- (NSString *)description {
    return [NSString stringWithFormat:@"%@ %@ (%04X:%04X:%04X)", self.manufacturerString, self.productString, self.vendorID, self.productID, self.revisionBCD];
}

@end
