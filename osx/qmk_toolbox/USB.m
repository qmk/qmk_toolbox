//
//  USB.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import "USB.h"
#include <IOKit/usb/IOUSBLib.h>
#include <IOKit/serial/IOSerialKeys.h>

#define DEFINE_ITER(type) \
static io_iterator_t g##type##AddedIter; \
static io_iterator_t g##type##RemovedIter

//Global variables
static IONotificationPortRef gNotifyPort;
DEFINE_ITER(AtmelSAMBA);
DEFINE_ITER(AtmelDFU);
DEFINE_ITER(Caterina);
DEFINE_ITER(Halfkay);
DEFINE_ITER(STM32DFU);
DEFINE_ITER(Kiibohd);
DEFINE_ITER(STM32Duino);
DEFINE_ITER(AVRISP);
DEFINE_ITER(USBAsp);
DEFINE_ITER(USBTiny);
DEFINE_ITER(BootloadHID);
DEFINE_ITER(APM32DFU);
static Printing * _printer;

@interface USB () <USBDelegate>

@end

@implementation USB

static int devicesAvailable[NumberOfChipsets];

+ (void)setupWithPrinter:(Printing *)printer andDelegate:(id<USBDelegate>)d {
    delegate = d;
    [self setupWithPrinter:printer];
}

+ (void)setupWithPrinter:(Printing *)printer {
    // https://developer.apple.com/library/content/documentation/DeviceDrivers/Conceptual/USBBook/USBDeviceInterfaces/USBDevInterfaces.html#//apple_ref/doc/uid/TP40002645-TPXREF101

    _printer = printer;
    mach_port_t            masterPort;
    CFMutableDictionaryRef AtmelSAMBAMatchingDict;
    CFMutableDictionaryRef AtmelDFUMatchingDict;
    CFMutableDictionaryRef CaterinaMatchingDict;
    CFMutableDictionaryRef SparkfunVIDMatchingDict;
    CFMutableDictionaryRef DogHunterVIDMatchingDict;
    CFMutableDictionaryRef PololuVIDMatchingDict;
    CFMutableDictionaryRef AdafruitVIDMatchingDict;
    CFMutableDictionaryRef HalfkayMatchingDict;
    CFMutableDictionaryRef STM32DFUMatchingDict;
    CFMutableDictionaryRef STM32DuinoMatchingDict;
    CFMutableDictionaryRef KiibohdMatchingDict;
    CFMutableDictionaryRef AVRISPMatchingDict;
    CFMutableDictionaryRef USBAspMatchingDict;
    CFMutableDictionaryRef USBTinyMatchingDict;
    CFMutableDictionaryRef BootloadHIDMatchingDict;
    CFMutableDictionaryRef APM32DFUMatchingDict;
    CFRunLoopSourceRef     runLoopSource;
    kern_return_t          kr;
    SInt32                 usbVendor;
    SInt32                 usbProduct;

    //Create a master port for communication with the I/O Kit
    kr = IOMasterPort(MACH_PORT_NULL, &masterPort);

    gNotifyPort = IONotificationPortCreate(masterPort);
    runLoopSource = IONotificationPortGetRunLoopSource(gNotifyPort);
    CFRunLoopAddSource(CFRunLoopGetCurrent(), runLoopSource, kCFRunLoopDefaultMode);

#define VID_MATCH(VID, type) VID_MATCH_MAP(VID, type, type)
#define VID_MATCH_MAP(VID, type, dest) \
usbVendor = VID; \
type##MatchingDict = IOServiceMatching(kIOUSBDeviceClassName); \
type##MatchingDict = (CFMutableDictionaryRef) CFRetain(type##MatchingDict); \
type##MatchingDict = (CFMutableDictionaryRef) CFRetain(type##MatchingDict); \
\
CFDictionarySetValue(type##MatchingDict, CFSTR(kUSBVendorID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbVendor)); \
CFDictionarySetValue(type##MatchingDict, CFSTR(kUSBProductID), CFSTR("*")); \
\
kr = IOServiceAddMatchingNotification(gNotifyPort, kIOFirstMatchNotification, type##MatchingDict, dest##DeviceAdded, NULL, &g##dest##AddedIter); \
dest##DeviceAdded(NULL, g##dest##AddedIter); \
\
kr = IOServiceAddMatchingNotification(gNotifyPort, kIOTerminatedNotification, type##MatchingDict, dest##DeviceRemoved, NULL, &g##dest##RemovedIter); \
dest##DeviceRemoved(NULL, g##dest##RemovedIter)

#define VID_PID_MATCH(VID, PID, type) VID_PID_MATCH_MAP(VID, PID, type, type)
#define VID_PID_MATCH_MAP(VID, PID, type, dest) \
usbVendor = VID; \
usbProduct = PID; \
\
type##MatchingDict = IOServiceMatching(kIOUSBDeviceClassName); \
type##MatchingDict = (CFMutableDictionaryRef) CFRetain(type##MatchingDict); \
type##MatchingDict = (CFMutableDictionaryRef) CFRetain(type##MatchingDict); \
\
CFDictionarySetValue(type##MatchingDict, CFSTR(kUSBVendorID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbVendor)); \
CFDictionarySetValue(type##MatchingDict, CFSTR(kUSBProductID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbProduct)); \
\
kr = IOServiceAddMatchingNotification(gNotifyPort, kIOFirstMatchNotification, type##MatchingDict, dest##DeviceAdded, NULL, &g##dest##AddedIter); \
dest##DeviceAdded(NULL, g##dest##AddedIter); \
\
kr = IOServiceAddMatchingNotification(gNotifyPort, kIOTerminatedNotification, type##MatchingDict, dest##DeviceRemoved, NULL, &g##dest##RemovedIter); \
dest##DeviceRemoved(NULL, g##dest##RemovedIter)

    VID_PID_MATCH(0x03EB, 0x6124, AtmelSAMBA);
    VID_MATCH(0x03EB, AtmelDFU);
    VID_MATCH(0x2341, Caterina);
    VID_MATCH_MAP(0x1B4F, SparkfunVID, Caterina);
    VID_MATCH_MAP(0x1FFB, PololuVID, Caterina);
    VID_MATCH_MAP(0x2A03, DogHunterVID, Caterina);
    VID_MATCH_MAP(0x239A, AdafruitVID, Caterina);
    VID_PID_MATCH(0x16C0, 0x0478, Halfkay);
    VID_PID_MATCH(0x0483, 0xDF11, STM32DFU);
    VID_PID_MATCH(0x1C11, 0xB007, Kiibohd);
    VID_PID_MATCH(0x1EAF, 0x0003, STM32Duino);
    VID_PID_MATCH(0x16C0, 0x0483, AVRISP);
    VID_PID_MATCH(0x16C0, 0x05DC, USBAsp);
    VID_PID_MATCH(0x1781, 0x0C9F, USBTiny);
    VID_PID_MATCH(0x16C0, 0x05DF, BootloadHID);
    VID_PID_MATCH(0x314B, 0x0106, APM32DFU);

    //Finished with master port
    mach_port_deallocate(mach_task_self(), masterPort);
    masterPort = 0;

    //Start the run loop so notifications will be received
    //CFRunLoopRun();
}

#define STR2(x) #x
#define STR(x) STR2(x)

#define DEVICE_EVENTS(type, name) \
static void type##DeviceAdded(void *refCon, io_iterator_t iterator) { \
    io_service_t object; \
    while ((object = IOIteratorNext(iterator))) { \
        [USB eventMessageForDevice:object withName:name connected:YES]; \
        deviceConnected(type); \
    } \
} \
static void type##DeviceRemoved(void *refCon, io_iterator_t iterator) { \
    kern_return_t kr; \
    io_service_t object; \
    while ((object = IOIteratorNext(iterator))) { \
        [USB eventMessageForDevice:object withName:name connected:NO]; \
        deviceDisconnected(type); \
        kr = IOObjectRelease(object); \
        if (kr != kIOReturnSuccess) { \
            printf("Couldn’t release raw device object: %08x\n", kr); \
            continue; \
        } \
    } \
}
#define DEVICE_EVENTS_PORT(type, name) \
static void type##DeviceAdded(void *refCon, io_iterator_t iterator) { \
    io_service_t object; \
    while ((object = IOIteratorNext(iterator))) { \
        double delayInSeconds = 1.; \
        dispatch_time_t popTime = dispatch_time(DISPATCH_TIME_NOW, (int64_t)(delayInSeconds * NSEC_PER_SEC)); \
        dispatch_after(popTime, dispatch_get_main_queue(), ^(void){ \
            NSString *devicePort = [USB calloutDeviceForDevice:object]; \
            [USB eventMessageForDevice:object withName:name withPort:devicePort connected:YES]; \
            [delegate setSerialPort:devicePort]; \
            deviceConnected(type); \
        }); \
    } \
} \
static void type##DeviceRemoved(void *refCon, io_iterator_t iterator) { \
    kern_return_t kr; \
    io_service_t object; \
    while ((object = IOIteratorNext(iterator))) { \
        [USB eventMessageForDevice:object withName:name connected:NO]; \
        deviceDisconnected(type); \
        kr = IOObjectRelease(object); \
        if (kr != kIOReturnSuccess) { \
            printf("Couldn’t release raw device object: %08x\n", kr); \
            continue; \
        } \
    } \
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

    io_iterator_t serialIterator;
    IOServiceGetMatchingServices(kIOMasterPortDefault, serialMatcher, &serialIterator);

    io_service_t port;
    while ((port = IOIteratorNext(serialIterator))) {
        io_service_t parent;
        IORegistryEntryGetParentEntry(port, kIOServicePlane, &parent);

        NSNumber *parentVendorID = [USB vendorIDForDevice:parent];
        NSNumber *childVendorID = [USB vendorIDForDevice:device];
        NSNumber *parentProductID = [USB productIDForDevice:parent];
        NSNumber *childProductID = [USB productIDForDevice:device];

        if (parentVendorID != nil) {
            if ([parentVendorID isEqualTo:childVendorID] && [parentProductID isEqualTo:childProductID]) {
                return [USB stringProperty:CFSTR(kIOCalloutDeviceKey) forDevice:port];
            }
        }
    }
    return nil;
}

+ (NSNumber *)shortProperty:(CFStringRef)property forDevice:(io_service_t)device {
    CFNumberRef cfProperty = IORegistryEntryCreateCFProperty(device, property, kCFAllocatorDefault, kNilOptions);
    if (cfProperty != nil) {
        NSNumber *nsProperty = (__bridge NSNumber *)(cfProperty);
        CFRelease(cfProperty);
        return nsProperty;
    }
    return nil;
}

+ (NSNumber *)vendorIDForDevice:(io_service_t)device {
    return [USB shortProperty:CFSTR(kUSBVendorID) forDevice:device];
}

+ (NSNumber *)productIDForDevice:(io_service_t)device {
    return [USB shortProperty:CFSTR(kUSBProductID) forDevice:device];
}

+ (NSNumber *)revisionBCDForDevice:(io_service_t)device {
    return [USB shortProperty:CFSTR(kUSBDeviceReleaseNumber) forDevice:device];
}

+ (void)eventMessageForDevice:(io_service_t)device withName:(NSString *)name connected:(BOOL)connected {
    [USB eventMessageForDevice:device withName:name withPort:nil connected:connected];
}

+ (void)eventMessageForDevice:(io_service_t)device withName:(NSString *)name withPort:(NSString *)port connected:(BOOL)connected {
    NSString *portString = @"";
    if (port != nil) {
        portString = [NSString stringWithFormat:@" [%@]", port];
    }

    [_printer print:[NSString stringWithFormat:
        @"%@ device %@: %@ %@ (%04X:%04X:%04X)%@",
        name,
        connected ? @"connected" : @"disconnected",
        [USB vendorStringForDevice:device],
        [USB productStringForDevice:device],
        [[USB vendorIDForDevice:device] unsignedShortValue],
        [[USB productIDForDevice:device] unsignedShortValue],
        [[USB revisionBCDForDevice:device] unsignedShortValue],
        portString
    ] withType:MessageType_Bootloader];
}

DEVICE_EVENTS_PORT(AtmelSAMBA, @"Atmel SAM-BA");
DEVICE_EVENTS(AtmelDFU, @"Atmel DFU");
DEVICE_EVENTS_PORT(Caterina, @"Caterina");
DEVICE_EVENTS(Halfkay, @"Halfkay");
DEVICE_EVENTS(STM32DFU, @"STM32 DFU");
DEVICE_EVENTS(Kiibohd, @"Kiibohd");
DEVICE_EVENTS(STM32Duino, @"STM32Duino");
DEVICE_EVENTS_PORT(AVRISP, @"AVRISP");
DEVICE_EVENTS(USBAsp, @"USBAsp");
DEVICE_EVENTS(USBTiny, @"USBTiny");
DEVICE_EVENTS(BootloadHID, @"BootloadHID");
DEVICE_EVENTS(APM32DFU, @"APM32 DFU");

static void deviceConnected(Chipset chipset) {
    devicesAvailable[chipset]+=1;
    [delegate deviceConnected:chipset];
}

static void deviceDisconnected(Chipset chipset) {
    devicesAvailable[chipset]-=1;
    [delegate deviceDisconnected:chipset];
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
