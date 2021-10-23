#import <IOKit/usb/IOUSBLib.h>
#import <IOKit/serial/IOSerialKeys.h>
#import <IOKit/storage/IOMedia.h>

#import "USB.h"

static io_iterator_t usbConnectedIter;
static io_iterator_t usbDisconnectedIter;

//Global variables
static IONotificationPortRef gNotifyPort;
static Printing *_printer;

NSArray *caterinaVids;
NSArray *caterinaPids;
NSArray *atmelDfuPids;

@interface USB () <USBDelegate>
@end

@implementation USB
static int devicesAvailable[NumberOfChipsets];

+ (void)setupWithPrinter:(Printing *)printer andDelegate:(id<USBDelegate>)d {
    delegate = d;
    _printer = printer;
    mach_port_t masterPort;
    CFRunLoopSourceRef runLoopSource;

    caterinaVids = @[
        @0x1B4F, // Spark Fun Electronics
        @0x1FFB, // Pololu Electronics
        @0x2341, // Arduino SA
        @0x239A, // Adafruit Industries LLC
        @0x2A03  // dog hunter AG
    ];
    caterinaPids = @[
        // Adafruit Industries LLC
        @0x000C, // Feather 32U4
        @0x000D, // ItsyBitsy 32U4 3V3/8MHz
        @0x000E, // ItsyBitsy 32U4 5V/16MHz
        // Arduino SA / dog hunter AG
        @0x0036, // Leonardo
        @0x0037, // Micro
        // Pololu Electronics
        @0x0101, // A-Star 32U4
        // Spark Fun Electronics
        @0x9203, // Pro Micro 3V3/8MHz
        @0x9205, // Pro Micro 5V/16MHz
        @0x9207  // LilyPad 3V3/8MHz (and some Pro Micro clones)
    ];
    atmelDfuPids = @[
        @0x2FEF, // ATmega16U2
        @0x2FF0, // ATmega32U2
        @0x2FF3, // ATmega16U4
        @0x2FF4, // ATmega32U4
        @0x2FF9, // AT90USB64
        @0x2FFA, // AT90USB162
        @0x2FFB  // AT90USB128
    ];

    //Create a master port for communication with the I/O Kit
    IOMasterPort(MACH_PORT_NULL, &masterPort);

    gNotifyPort = IONotificationPortCreate(masterPort);
    runLoopSource = IONotificationPortGetRunLoopSource(gNotifyPort);
    CFRunLoopAddSource(CFRunLoopGetCurrent(), runLoopSource, kCFRunLoopDefaultMode);

    CFMutableDictionaryRef usbMatcher = IOServiceMatching(kIOUSBDeviceClassName);
    usbMatcher = (CFMutableDictionaryRef) CFRetain(usbMatcher);

    IOServiceAddMatchingNotification(gNotifyPort, kIOFirstMatchNotification, usbMatcher, deviceConnectedEvent, NULL, &usbConnectedIter);
    deviceConnectedEvent(NULL, usbConnectedIter);

    IOServiceAddMatchingNotification(gNotifyPort, kIOTerminatedNotification, usbMatcher, deviceDisconnectedEvent, NULL, &usbDisconnectedIter);
    deviceDisconnectedEvent(NULL, usbDisconnectedIter);

    //Finished with master port
    mach_port_deallocate(mach_task_self(), masterPort);
    masterPort = 0;
}

+ (NSString *)stringProperty:(CFStringRef)property forDevice:(io_service_t)device {
    CFStringRef cfProperty = IORegistryEntryCreateCFProperty(device, property, kCFAllocatorDefault, kNilOptions);
    if (cfProperty != nil) {
        NSString *nsProperty = (__bridge NSString *)(cfProperty);
        CFRelease(cfProperty);
        return nsProperty;
    }
    return nil;
}

+ (NSString *)vendorStringForDevice:(io_service_t)device {
    return [USB stringProperty:CFSTR(kUSBVendorString) forDevice:device];
}

+ (NSString *)productStringForDevice:(io_service_t)device {
    return [USB stringProperty:CFSTR(kUSBProductString) forDevice:device];
}

+ (NSString *)calloutDeviceForDevice:(io_service_t)device {
    CFMutableDictionaryRef serialMatcher = IOServiceMatching(kIOSerialBSDServiceValue);
    CFDictionarySetValue(serialMatcher, CFSTR(kIOSerialBSDTypeKey), CFSTR(kIOSerialBSDAllTypes));

    NSNumber *childVendorID = [USB vendorIDForDevice:device];
    NSNumber *childProductID = [USB productIDForDevice:device];

    io_iterator_t serialIterator;
    IOServiceGetMatchingServices(kIOMasterPortDefault, serialMatcher, &serialIterator);

    io_service_t port;
    while ((port = IOIteratorNext(serialIterator))) {
        NSNumber *parentVendorID = [USB vendorIDForDevice:port];
        NSNumber *parentProductID = [USB productIDForDevice:port];

        if (parentVendorID != nil) {
            if ([parentVendorID isEqualTo:childVendorID] && [parentProductID isEqualTo:childProductID]) {
                return [USB stringProperty:CFSTR(kIOCalloutDeviceKey) forDevice:port];
            }
        }
    }
    return nil;
}

+ (NSString *)mountPointForDevice:(io_service_t)device {
    CFMutableDictionaryRef massStorageMatcher = IOServiceMatching(kIOMediaClass);
    CFDictionarySetValue(massStorageMatcher, CFSTR(kIOMediaRemovableKey), kCFBooleanTrue);

    NSNumber *childVendorID = [USB vendorIDForDevice:device];
    NSNumber *childProductID = [USB productIDForDevice:device];

    io_iterator_t massStorageIterator;
    IOServiceGetMatchingServices(kIOMasterPortDefault, massStorageMatcher, &massStorageIterator);

    io_service_t media;
    while ((media = IOIteratorNext(massStorageIterator))) {
        NSNumber *parentVendorID = [USB vendorIDForDevice:media];
        NSNumber *parentProductID = [USB productIDForDevice:media];

        if (parentVendorID != nil) {
            if ([parentVendorID isEqualTo:childVendorID] && [parentProductID isEqualTo:childProductID]) {
                DASessionRef sessionRef = DASessionCreate(kCFAllocatorDefault);
                if (sessionRef) {
                    DADiskRef diskRef = DADiskCreateFromIOMedia(kCFAllocatorDefault, sessionRef, media);
                    if (diskRef) {
                        CFDictionaryRef diskProperties = DADiskCopyDescription(diskRef);
                        if (diskProperties) {
                            NSURL *mountPointUrl = [(__bridge NSDictionary*)diskProperties objectForKey:(NSString *)kDADiskDescriptionVolumePathKey];
                            return [mountPointUrl path];
                        }
                    }
                }
            }
        }
    }
    IOObjectRelease(media);

    return nil;
}

+ (NSNumber *)numberProperty:(CFStringRef)property forDevice:(io_service_t)device {
    CFNumberRef cfProperty = IORegistryEntrySearchCFProperty(device, kIOServicePlane, property, kCFAllocatorDefault, kIORegistryIterateParents | kIORegistryIterateRecursively);
    if (cfProperty != nil) {
        NSNumber *nsProperty = (__bridge NSNumber *)(cfProperty);
        CFRelease(cfProperty);
        return nsProperty;
    }
    return nil;
}

+ (NSNumber *)vendorIDForDevice:(io_service_t)device {
    return [USB numberProperty:CFSTR(kUSBVendorID) forDevice:device];
}

+ (NSNumber *)productIDForDevice:(io_service_t)device {
    return [USB numberProperty:CFSTR(kUSBProductID) forDevice:device];
}

+ (NSNumber *)revisionBCDForDevice:(io_service_t)device {
    return [USB numberProperty:CFSTR(kUSBDeviceReleaseNumber) forDevice:device];
}

+ (BOOL)isSerialDevice:(io_service_t)device {
    return [[USB numberProperty:CFSTR(kUSBDeviceClass) forDevice:device] isEqualTo:[NSNumber numberWithInt:kUSBCommunicationClass]];
}

static void deviceConnectedEvent(void *refCon, io_iterator_t iterator) {
    io_service_t device;
    while ((device = IOIteratorNext(iterator))) {
        [USB deviceEvent:device connected:YES];
    }
}

static void deviceDisconnectedEvent(void *refCon, io_iterator_t iterator) {
    io_service_t device;
    while ((device = IOIteratorNext(iterator))) {
        [USB deviceEvent:device connected:NO];
        IOObjectRelease(device);
    }
}

+ (void)deviceConnectedEvent:(io_service_t)device {
    [USB deviceEvent:device connected:YES];
}

+ (void)deviceDisconnectedEvent:(io_service_t)device {
    [USB deviceEvent:device connected:NO];
}

+ (void)deviceEvent:(io_service_t)device connected:(BOOL)connected {
    unsigned short vendorID = [[USB vendorIDForDevice:device] unsignedShortValue];
    unsigned short productID = [[USB productIDForDevice:device] unsignedShortValue];
    unsigned short revisionBCD = [[USB revisionBCDForDevice:device] unsignedShortValue];
    NSString *vendorString = [USB vendorStringForDevice:device];
    NSString *productString = [USB productStringForDevice:device];

    NSString *deviceName;
    NSString *calloutDevice;
    NSString *mountPoint;
    Chipset deviceType;

    if ([USB isSerialDevice:device]) {
        if (vendorID == 0x03EB && productID == 0x6124) { // Atmel SAM-BA
            deviceName = @"Atmel SAM-BA";
            deviceType = AtmelSAMBA;
        } else if ([caterinaVids containsObject:[NSNumber numberWithUnsignedShort:vendorID]] && [caterinaPids containsObject:[NSNumber numberWithUnsignedShort:productID]]) { // Caterina
            deviceName = @"Caterina";
            deviceType = Caterina;
        } else if (vendorID == 0x16C0 && productID == 0x0483) { // ArduinoISP/AVRISP
            deviceName = @"AVR-ISP";
            deviceType = AVRISP;
        } else {
            return;
        }

        if (connected) {
            while (calloutDevice == nil) {
                calloutDevice = [USB calloutDeviceForDevice:device];
            }
            [delegate setSerialPort:calloutDevice];
        }
    } else if (vendorID == 0x03EB) {
        if ([atmelDfuPids containsObject:[NSNumber numberWithUnsignedShort:productID]]) {
            if (revisionBCD == 0x0936) { // QMK DFU
                deviceName = @"QMK DFU";
                deviceType = QMKDFU;
            } else { // Atmel DFU
                deviceName = @"Atmel DFU";
                deviceType = AtmelDFU;
            }
        } else if (productID == 0x2045) { // LUFA MS
            deviceName = @"LUFA Mass Storage";
            deviceType = LUFAMS;

            if (connected) {
                while (mountPoint == nil) {
                    mountPoint = [USB mountPointForDevice:device];
                }
                [delegate setMountPoint:mountPoint];
            }
        } else {
            return;
        }
    } else if (vendorID == 0x16C0 && productID == 0x0478) { // PJRC Teensy
        deviceName = @"Halfkay";
        deviceType = Halfkay;
    } else if (vendorID == 0x0483 && productID == 0xDF11) { // STM32 DFU
        deviceName = @"STM32 DFU";
        deviceType = STM32DFU;
    } else if (vendorID == 0x314B && productID == 0x0106) { // APM32 DFU
        deviceName = @"APM32 DFU";
        deviceType = APM32DFU;
    } else if (vendorID == 0x1C11 && productID == 0xB007) { // Kiibohd
        deviceName = @"Kiibohd";
        deviceType = Kiibohd;
    } else if (vendorID == 0x16C0 && productID == 0x05DF) { // Objective Development BootloadHID
        deviceName = @"BootloadHID";
        deviceType = BootloadHID;
    } else if (vendorID == 0x16C0 && productID == 0x05DC) { // USBAsp and USBAspLoader
        deviceName = @"USBAsp";
        deviceType = USBAsp;
    } else if (vendorID == 0x1791 && productID == 0x0C9F) { // USB Tiny
        deviceName = @"USB Tiny";
        deviceType = USBTiny;
    } else if (vendorID == 0x1EAF && productID == 0x0003) { // STM32Duino
        deviceName = @"STM32Duino";
        deviceType = STM32Duino;
    } else {
        return;
    }

    NSString *calloutDeviceString = @"";
    NSString *mountPointString = @"";
    if (calloutDevice != nil) {
        calloutDeviceString = [NSString stringWithFormat:@" [%@]", calloutDevice];
    }
    if (mountPoint != nil) {
        mountPointString = [NSString stringWithFormat:@" [%@]", mountPoint];
    }

    [_printer print:[NSString stringWithFormat:
        @"%@ device %@: %@ %@ (%04X:%04X:%04X)%@%@",
        deviceName,
        connected ? @"connected" : @"disconnected",
        vendorString,
        productString,
        vendorID,
        productID,
        revisionBCD,
        calloutDeviceString,
        mountPointString
    ] withType:MessageType_Bootloader];

    if (connected) {
        devicesAvailable[deviceType]++;
        [delegate deviceConnected:deviceType];
    } else {
        devicesAvailable[deviceType]--;
        [delegate deviceDisconnected:deviceType];
    }
}

+ (BOOL)areDevicesAvailable {
    BOOL available = NO;
    for (int i = 0; i < NumberOfChipsets; i++) {
        available |= devicesAvailable[i];
    }
    return available;
}

+ (BOOL)canFlash:(Chipset)chipset {
    return (devicesAvailable[chipset] > 0);
}
@end
