#import "HIDConsoleDevice.h"

@implementation HIDConsoleDevice {
    uint8_t hidReportBuffer[64];
}

- (id)initWithDeviceRef:(IOHIDDeviceRef)deviceRef {
    if (self = [super init]) {
        self.deviceRef = deviceRef;

        self.manufacturerString = [self stringProperty:CFSTR(kIOHIDManufacturerKey)];
        self.productString = [self stringProperty:CFSTR(kIOHIDProductKey)];
        self.vendorID = [self ushortProperty:CFSTR(kIOHIDVendorIDKey)];
        self.productID = [self ushortProperty:CFSTR(kIOHIDProductIDKey)];
        self.revisionBCD = [self ushortProperty:CFSTR(kIOHIDVersionNumberKey)];

        IOHIDDeviceRegisterInputReportCallback(self.deviceRef, hidReportBuffer, sizeof(hidReportBuffer), reportReceived, (__bridge void *)self);
    }
    return self;
}

- (NSString *)description {
    return [NSString stringWithFormat:@"%@ %@ (%04X:%04X:%04X)", self.manufacturerString, self.productString, self.vendorID, self.productID, self.revisionBCD];
}

static void reportReceived(void *context, IOReturn result, void *sender, IOHIDReportType type, uint32_t reportID, uint8_t *report, CFIndex reportLength) {
    HIDConsoleDevice *const device = (__bridge HIDConsoleDevice *const)context;
    [device.delegate consoleDevice:device didReceiveReport:[NSString stringWithFormat:@"%s", (char *)report]];
}

- (NSString *)stringProperty:(CFStringRef)property {
    return (__bridge NSString *)IOHIDDeviceGetProperty(self.deviceRef, property);
}

- (ushort)ushortProperty:(CFStringRef)property {
    return [(__bridge NSNumber *)IOHIDDeviceGetProperty(self.deviceRef, property) unsignedShortValue];
}

- (nonnull id)copyWithZone:(nullable NSZone *)zone {
    HIDConsoleDevice *deviceCopy = [[[self class] allocWithZone:zone] init];

    if (deviceCopy) {
        deviceCopy.deviceRef = self.deviceRef;
        deviceCopy.manufacturerString = self.manufacturerString;
        deviceCopy.productString = self.productString;
        deviceCopy.vendorID = self.vendorID;
        deviceCopy.productID = self.productID;
        deviceCopy.revisionBCD = self.revisionBCD;
    }

    return deviceCopy;
}
@end
