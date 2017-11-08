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
 
//Global variables
static IONotificationPortRef    gNotifyPort;
static io_iterator_t            gDFUAddedIter;
static io_iterator_t            gDFURemovedIter;
static io_iterator_t            gCaterinaAddedIter;
static io_iterator_t            gCaterinaRemovedIter;
static io_iterator_t            gHalfkayAddedIter;
static io_iterator_t            gHalfkayRemovedIter;
static io_iterator_t            gSTM32AddedIter;
static io_iterator_t            gSTM32RemovedIter;
static io_iterator_t            gKiibohdAddedIter;
static io_iterator_t            gKiibohdRemovedIter;
static Printing * _printer;

@interface USB ()

@end

@implementation USB

+ (void)setupWithPrinter:(Printing *)printer andDelegate:(id<USBDelegate>)d {

    delegate = d;
    
    // https://developer.apple.com/library/content/documentation/DeviceDrivers/Conceptual/USBBook/USBDeviceInterfaces/USBDevInterfaces.html#//apple_ref/doc/uid/TP40002645-TPXREF101
    
    _printer = printer;	
    mach_port_t             masterPort;
    CFMutableDictionaryRef  DFUMatchingDict;
    CFMutableDictionaryRef  CaterinaMatchingDict;
    CFMutableDictionaryRef  HalfkayMatchingDict;
    CFMutableDictionaryRef  STM32MatchingDict;
    CFMutableDictionaryRef  KiibohdMatchingDict;
    CFRunLoopSourceRef      runLoopSource;
    kern_return_t           kr;
    SInt32                  usbVendor;
    SInt32                  usbProduct;
 
    //Create a master port for communication with the I/O Kit
    kr = IOMasterPort(MACH_PORT_NULL, &masterPort);

    gNotifyPort = IONotificationPortCreate(masterPort);
    runLoopSource = IONotificationPortGetRunLoopSource(gNotifyPort);
    CFRunLoopAddSource(CFRunLoopGetCurrent(), runLoopSource, kCFRunLoopDefaultMode);
 
 
 
    // DFU
    usbVendor = 0x03EB;
    
    DFUMatchingDict = IOServiceMatching(kIOUSBDeviceClassName);
    DFUMatchingDict = (CFMutableDictionaryRef) CFRetain(DFUMatchingDict);
    DFUMatchingDict = (CFMutableDictionaryRef) CFRetain(DFUMatchingDict);
 
    CFDictionarySetValue(DFUMatchingDict, CFSTR(kUSBVendorID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbVendor));
    CFDictionarySetValue(DFUMatchingDict, CFSTR(kUSBProductID), CFSTR("*"));

    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOFirstMatchNotification, DFUMatchingDict, DFUDeviceAdded, NULL, &gDFUAddedIter);
    DFUDeviceAdded(NULL, gDFUAddedIter);
 
    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOTerminatedNotification, DFUMatchingDict, DFUDeviceRemoved, NULL, &gDFURemovedIter);
    DFUDeviceRemoved(NULL, gDFURemovedIter);
    
    // Caterina
    usbVendor = 0x2341;
    
    CaterinaMatchingDict = IOServiceMatching(kIOUSBDeviceClassName);
    CaterinaMatchingDict = (CFMutableDictionaryRef) CFRetain(CaterinaMatchingDict);
    CaterinaMatchingDict = (CFMutableDictionaryRef) CFRetain(CaterinaMatchingDict);
 
    CFDictionarySetValue(CaterinaMatchingDict, CFSTR(kUSBVendorID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbVendor));
    CFDictionarySetValue(CaterinaMatchingDict, CFSTR(kUSBProductID), CFSTR("*"));

    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOFirstMatchNotification, CaterinaMatchingDict, CaterinaDeviceAdded, NULL, &gCaterinaAddedIter);
    CaterinaDeviceAdded(NULL, gCaterinaAddedIter);
 
    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOTerminatedNotification, CaterinaMatchingDict, CaterinaDeviceRemoved, NULL, &gCaterinaRemovedIter);
    CaterinaDeviceRemoved(NULL, gCaterinaRemovedIter);
    
    
    // Halfkay
    usbVendor = 0x16C0;
    usbProduct = 0x0478;

    HalfkayMatchingDict = IOServiceMatching(kIOUSBDeviceClassName);
    HalfkayMatchingDict = (CFMutableDictionaryRef) CFRetain(HalfkayMatchingDict);
    HalfkayMatchingDict = (CFMutableDictionaryRef) CFRetain(HalfkayMatchingDict);
 
    CFDictionarySetValue(HalfkayMatchingDict, CFSTR(kUSBVendorID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbVendor));
    CFDictionarySetValue(HalfkayMatchingDict, CFSTR(kUSBProductID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbProduct));

    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOFirstMatchNotification, HalfkayMatchingDict, HalfkayDeviceAdded, NULL, &gHalfkayAddedIter);
    HalfkayDeviceAdded(NULL, gHalfkayAddedIter);
 
    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOTerminatedNotification, HalfkayMatchingDict, HalfkayDeviceRemoved, NULL, &gHalfkayRemovedIter);
    HalfkayDeviceRemoved(NULL, gHalfkayRemovedIter);
    
    
    // STM32
    usbVendor = 0x0483;
    usbProduct = 0xDF11;

    STM32MatchingDict = IOServiceMatching(kIOUSBDeviceClassName);
    STM32MatchingDict = (CFMutableDictionaryRef) CFRetain(STM32MatchingDict);
    STM32MatchingDict = (CFMutableDictionaryRef) CFRetain(STM32MatchingDict);
 
    CFDictionarySetValue(STM32MatchingDict, CFSTR(kUSBVendorID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbVendor));
    CFDictionarySetValue(STM32MatchingDict, CFSTR(kUSBProductID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbProduct));

    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOFirstMatchNotification, STM32MatchingDict, STM32DeviceAdded, NULL, &gSTM32AddedIter);
    STM32DeviceAdded(NULL, gSTM32AddedIter);
 
    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOTerminatedNotification, STM32MatchingDict, STM32DeviceRemoved, NULL, &gSTM32RemovedIter);
    STM32DeviceRemoved(NULL, gSTM32RemovedIter);
 
 
    // Kiibohd
    
    usbVendor = 0x1C11;
    usbProduct = 0xB007;
    
    KiibohdMatchingDict = IOServiceMatching(kIOUSBDeviceClassName);
    KiibohdMatchingDict = (CFMutableDictionaryRef) CFRetain(KiibohdMatchingDict);
    KiibohdMatchingDict = (CFMutableDictionaryRef) CFRetain(KiibohdMatchingDict);
 
    CFDictionarySetValue(KiibohdMatchingDict, CFSTR(kUSBVendorID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbVendor));
    CFDictionarySetValue(KiibohdMatchingDict, CFSTR(kUSBProductID), CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &usbProduct));

    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOFirstMatchNotification, KiibohdMatchingDict, KiibohdDeviceAdded, NULL, &gKiibohdAddedIter);
    STM32DeviceAdded(NULL, gSTM32AddedIter);
 
    kr = IOServiceAddMatchingNotification(gNotifyPort, kIOTerminatedNotification, KiibohdMatchingDict, KiibohdDeviceRemoved, NULL, &gKiibohdRemovedIter);
    STM32DeviceRemoved(NULL, gSTM32RemovedIter);
 
 
 
    //Finished with master port
    mach_port_deallocate(mach_task_self(), masterPort);
    masterPort = 0;
 
    //Start the run loop so notifications will be received
    //CFRunLoopRun();
}

static void DFUDeviceAdded(void *refCon, io_iterator_t iterator) {
    io_service_t    object;
    while ((object = IOIteratorNext(iterator))) {
        [_printer print:@"DFU device connected" withType:MessageType_Bootloader];
        [delegate deviceConnected:DFU];
    }
}

static void DFUDeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"DFU device disconnected" withType:MessageType_Bootloader];
        [delegate deviceDisconnected:DFU];
        kr = IOObjectRelease(object);
        if (kr != kIOReturnSuccess)
        {
            printf("Couldn’t release raw device object: %08x\n", kr);
            continue;
        }
    }
}

static void CaterinaDeviceAdded(void *refCon, io_iterator_t iterator) {
    io_service_t    object;
    while ((object = IOIteratorNext(iterator))) {
        double delayInSeconds = 1.;
        dispatch_time_t popTime = dispatch_time(DISPATCH_TIME_NOW, (int64_t)(delayInSeconds * NSEC_PER_SEC));
        dispatch_after(popTime, dispatch_get_main_queue(), ^(void){
            [_printer print:@"Caterina device connected" withType:MessageType_Bootloader];
            [delegate deviceConnected:Caterina];
            
            io_iterator_t   serialPortIterator;
            char        deviceFilePath[64];
            MyFindModems(&serialPortIterator);
            MyGetModemPath(serialPortIterator, deviceFilePath, sizeof(deviceFilePath));
            if (!deviceFilePath[0]) {
                printf("No modem port found.\n");
            } else {
                [delegate setCaterinaPort:[NSString stringWithFormat:@"%s", deviceFilePath]];
                [_printer printResponse:[NSString stringWithFormat:@"Found port: %s", deviceFilePath] withType:MessageType_Bootloader];
            }
            IOObjectRelease(serialPortIterator);
        });
    }

}

static void CaterinaDeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"Caterina device disconnected" withType:MessageType_Bootloader];
        [delegate deviceDisconnected:Caterina];
        kr = IOObjectRelease(object);
        if (kr != kIOReturnSuccess)
        {
            printf("Couldn’t release raw device object: %08x\n", kr);
            continue;
        }
    }
}

static void HalfkayDeviceAdded(void *refCon, io_iterator_t iterator) {
    io_service_t    object;
    while ((object = IOIteratorNext(iterator))) {
        [_printer print:@"Halfkay device connected" withType:MessageType_Bootloader];
        [delegate deviceConnected:Halfkay];
    }
}

static void HalfkayDeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"Halfkay device disconnected" withType:MessageType_Bootloader];
        [delegate deviceDisconnected:Halfkay];
        kr = IOObjectRelease(object);
        if (kr != kIOReturnSuccess)
        {
            printf("Couldn’t release raw device object: %08x\n", kr);
            continue;
        }
    }
}

static void STM32DeviceAdded(void *refCon, io_iterator_t iterator) {
    io_service_t    object;
    while ((object = IOIteratorNext(iterator))) {
        [_printer print:@"STM32 device connected" withType:MessageType_Bootloader];
        [delegate deviceConnected:STM32];
    }
}

static void STM32DeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"STM32 device disconnected" withType:MessageType_Bootloader];
        [delegate deviceDisconnected:STM32];
        kr = IOObjectRelease(object);
        if (kr != kIOReturnSuccess)
        {
            printf("Couldn’t release raw device object: %08x\n", kr);
            continue;
        }
    }
}

static void KiibohdDeviceAdded(void *refCon, io_iterator_t iterator) {
    io_service_t    object;
    while ((object = IOIteratorNext(iterator))) {
        [_printer print:@"Kiibohd device connected" withType:MessageType_Bootloader];
        [delegate deviceConnected:Kiibohd];
    }
}

static void KiibohdDeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"Kiibohd device disconnected" withType:MessageType_Bootloader];
        [delegate deviceDisconnected:Kiibohd];
        kr = IOObjectRelease(object);
        if (kr != kIOReturnSuccess)
        {
            printf("Couldn’t release raw device object: %08x\n", kr);
            continue;
        }
    }
}

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
    Boolean     modemFound = false;
 
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
 
        result = CFStringGetCString(deviceFilePathAsCFString,
                                        deviceFilePath,
                                        maxPathSize,
                                        kCFStringEncodingASCII);
            CFRelease(deviceFilePathAsCFString);
 
            if (result)
            {
                printf("BSD path: %s", deviceFilePath);
                modemFound = true;
                kernResult = KERN_SUCCESS;
            }
        }
 
        printf("\n");
 
        // Release the io_service_t now that we are done with it.
 
    (void) IOObjectRelease(modemService);
    }
 
    return kernResult;
}

@end
