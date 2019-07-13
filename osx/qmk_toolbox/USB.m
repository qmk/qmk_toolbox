//
//  USB.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import "USB.h"
#import <IOKit/usb/IOUSBLib.h>
#include <IOKit/IOKitLib.h>
#include <IOKit/serial/IOSerialKeys.h>
#include <IOKit/IOBSD.h>

#define FILEPATH_SIZE 64
#define DEFINE_ITER(type) \
static io_iterator_t            g##type##AddedIter; \
static io_iterator_t            g##type##RemovedIter

//Global variables
static IONotificationPortRef    gNotifyPort;
DEFINE_ITER(DFU);
DEFINE_ITER(Caterina);
DEFINE_ITER(Halfkay);
DEFINE_ITER(STM32);
DEFINE_ITER(Kiibohd);
DEFINE_ITER(AVRISP);
DEFINE_ITER(USBAsp);
DEFINE_ITER(USBTiny);
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
    mach_port_t             masterPort;
    CFMutableDictionaryRef  DFUMatchingDict;
    CFMutableDictionaryRef  CaterinaMatchingDict;
    CFMutableDictionaryRef  CaterinaAltMatchingDict;
    CFMutableDictionaryRef  FeatherBLE32u4MatchingDict;
    CFMutableDictionaryRef  HalfkayMatchingDict;
    CFMutableDictionaryRef  STM32MatchingDict;
    CFMutableDictionaryRef  KiibohdMatchingDict;
    CFMutableDictionaryRef  AVRISPMatchingDict;
    CFMutableDictionaryRef  USBAspMatchingDict;
    CFMutableDictionaryRef  USBTinyMatchingDict;
    CFRunLoopSourceRef      runLoopSource;
    kern_return_t           kr;
    SInt32                  usbVendor;
    SInt32                  usbProduct;
 
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
dest##DeviceRemoved(NULL, g##dest##RemovedIter) \
    
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
dest##DeviceRemoved(NULL, g##dest##RemovedIter) \
    
    
    VID_MATCH(0x03EB, DFU);
    VID_MATCH(0x2341, Caterina);
    VID_MATCH_MAP(0x1B4F, CaterinaAlt, Caterina);
    VID_MATCH_MAP(0x239a, FeatherBLE32u4, Caterina);
    VID_PID_MATCH(0x16C0, 0x0478, Halfkay);
    VID_PID_MATCH(0x0483, 0xDF11, STM32);
    VID_PID_MATCH(0x1C11, 0xB007, Kiibohd);
    VID_PID_MATCH(0x16C0, 0x0483, AVRISP);
    VID_PID_MATCH(0x16C0, 0x05DC, USBAsp);
    VID_PID_MATCH(0x1781, 0x0C9F, USBTiny);
 
 
    //Finished with master port
    mach_port_deallocate(mach_task_self(), masterPort);
    masterPort = 0;
 
    //Start the run loop so notifications will be received
    //CFRunLoopRun();
}

#define STR2(x) #x
#define STR(x) STR2(x)

#define DEVICE_EVENTS(type) \
static void type##DeviceAdded(void *refCon, io_iterator_t iterator) { \
    io_service_t object; \
    while ((object = IOIteratorNext(iterator))) { \
        [_printer print:[NSString stringWithFormat:@"%@ %@", @(STR(type)), @"device connected"] withType:MessageType_Bootloader]; \
        deviceConnected(type); \
    } \
} \
static void type##DeviceRemoved(void *refCon, io_iterator_t iterator) { \
    kern_return_t   kr; \
    io_service_t    object; \
    while ((object = IOIteratorNext(iterator))) \
    { \
        [_printer print:[NSString stringWithFormat:@"%@ %@", @(STR(type)), @"device disconnected"] withType:MessageType_Bootloader]; \
        [delegate deviceDisconnected:type]; \
        kr = IOObjectRelease(object); \
        if (kr != kIOReturnSuccess) \
        { \
            printf("Couldn’t release raw device object: %08x\n", kr); \
            continue; \
        } \
    } \
}
#define DEVICE_EVENTS_PORT(type) \
static void type##DeviceAdded(void *refCon, io_iterator_t iterator) { \
    io_service_t    object; \
    while ((object = IOIteratorNext(iterator))) { \
        double delayInSeconds = 1.; \
        dispatch_time_t popTime = dispatch_time(DISPATCH_TIME_NOW, (int64_t)(delayInSeconds * NSEC_PER_SEC)); \
        dispatch_after(popTime, dispatch_get_main_queue(), ^(void){ \
            [_printer print:[NSString stringWithFormat:@"%@ %@", @(STR(type)), @"device connected"] withType:MessageType_Bootloader]; \
            deviceConnected(type); \
            io_iterator_t serialPortIterator; \
            char deviceFilePath[FILEPATH_SIZE]; \
            MyFindModems(&serialPortIterator); \
            MyGetModemPath(serialPortIterator, deviceFilePath, sizeof(deviceFilePath)); \
            if (!deviceFilePath[0]) { \
                printf("No modem port found.\n"); \
                [_printer printResponse:@"No modem port found, try again." withType:MessageType_Bootloader]; \
            } else { \
                [delegate setCaterinaPort:[NSString stringWithFormat:@"%s", deviceFilePath]]; \
                [_printer printResponse:[NSString stringWithFormat:@"Found port: %s", deviceFilePath] withType:MessageType_Bootloader]; \
            } \
            IOObjectRelease(serialPortIterator); \
        }); \
    } \
} \
static void type##DeviceRemoved(void *refCon, io_iterator_t iterator) { \
    kern_return_t   kr; \
    io_service_t    object; \
    while ((object = IOIteratorNext(iterator))) \
    { \
        [_printer print:[NSString stringWithFormat:@"%@ %@", @(STR(type)), @"device disconnected"] withType:MessageType_Bootloader]; \
        deviceDisconnected(type); \
        kr = IOObjectRelease(object); \
        if (kr != kIOReturnSuccess) \
        { \
            printf("Couldn’t release raw device object: %08x\n", kr); \
            continue; \
        } \
    } \
}

DEVICE_EVENTS(DFU);
DEVICE_EVENTS_PORT(Caterina);
DEVICE_EVENTS(Halfkay);
DEVICE_EVENTS(STM32);
DEVICE_EVENTS(Kiibohd);
DEVICE_EVENTS_PORT(AVRISP);
DEVICE_EVENTS(USBAsp);
DEVICE_EVENTS_PORT(USBTiny);

static kern_return_t MyFindModems(io_iterator_t *matchingServices)
{
    kern_return_t       kernResult;
    mach_port_t         masterPort;
    CFMutableDictionaryRef  classesToMatch;
 
    kernResult = IOMasterPort(MACH_PORT_NULL, &masterPort);
    if (KERN_SUCCESS != kernResult)
    {
        printf("IOMasterPort returned %d\n", kernResult);
    goto exit;
    }
 
    // Serial devices are instances of class IOSerialBSDClient.
    classesToMatch = IOServiceMatching(kIOSerialBSDServiceValue);
    if (classesToMatch == NULL)
    {
        printf("IOServiceMatching returned a NULL dictionary.\n");
    }
    else {
        CFDictionarySetValue(classesToMatch,
                             CFSTR(kIOSerialBSDTypeKey),
                             CFSTR(kIOSerialBSDAllTypes));
 
        // Each serial device object has a property with key
        // kIOSerialBSDTypeKey and a value that is one of
        // kIOSerialBSDAllTypes, kIOSerialBSDModemType,
        // or kIOSerialBSDRS232Type. You can change the
        // matching dictionary to find other types of serial
        // devices by changing the last parameter in the above call
        // to CFDictionarySetValue.
    }
 
    kernResult = IOServiceGetMatchingServices(masterPort, classesToMatch, matchingServices);
    if (KERN_SUCCESS != kernResult)
    {
        printf("IOServiceGetMatchingServices returned %d\n", kernResult);
    goto exit;
    }
 
exit:
    return kernResult;
}

static kern_return_t MyGetModemPath(io_iterator_t serialPortIterator, char *deviceFilePath, CFIndex maxPathSize)
{
    io_object_t     modemService;
    kern_return_t   kernResult = KERN_FAILURE;
 
    // Initialize the returned path
    *deviceFilePath = '\0';
 
    // Iterate across all modems found. In this example, we exit after
    // finding the first modem.
 
    while ((modemService = IOIteratorNext(serialPortIterator)))
    {
        CFTypeRef   deviceFilePathAsCFString;
 
        // Get the callout device's path (/dev/cu.xxxxx).
        // The callout device should almost always be
        // used. You would use the dialin device (/dev/tty.xxxxx) when
        // monitoring a serial port for
        // incoming calls, for example, a fax listener.
 
        deviceFilePathAsCFString = IORegistryEntryCreateCFProperty(modemService,
                                                                   CFSTR(kIOCalloutDeviceKey),
                                                                   kCFAllocatorDefault,
                                                                   0);
        if (deviceFilePathAsCFString)
        {
            Boolean result;
 
            // Convert the path from a CFString to a NULL-terminated C string
            // for use with the POSIX open() call.
            char testDeviceFilePath[FILEPATH_SIZE];
            result = CFStringGetCString(deviceFilePathAsCFString,
                                        testDeviceFilePath,
                                        maxPathSize,
                                        kCFStringEncodingASCII);
            CFRelease(deviceFilePathAsCFString);
 
            if (result)
            {
                NSString *testDevice = [NSString stringWithUTF8String:testDeviceFilePath];
                if ([testDevice rangeOfString:@"Bluetooth"].location == NSNotFound) {
                    memcpy(deviceFilePath, testDeviceFilePath, FILEPATH_SIZE);
                    printf("BSD path: %s\n", deviceFilePath);
                    kernResult = KERN_SUCCESS;
                } else {
                    printf("BSD path (ignored): %s\n", testDeviceFilePath);
                    continue;
                }
            }
        }
        // Release the io_service_t now that we are done with it.
        IOObjectRelease(modemService);
    }
 
    return kernResult;
}

static void deviceConnected(Chipset chipset) {
    devicesAvailable[chipset]+=1;
    [delegate deviceConnected:chipset];
}

static void deviceDisconnected(Chipset chipset) {
    devicesAvailable[chipset]-=1;
    [delegate deviceDisconnected:chipset];
}

+ (BOOL) areDevicesAvailable {
    BOOL available = NO;
    for (int i = 0; i < NumberOfChipsets; i++) {
        available |= devicesAvailable[i];
    }
    return available;
}

+ (BOOL) canFlash:(Chipset) chipset {
    return (devicesAvailable[chipset] > 0);
}

@end
