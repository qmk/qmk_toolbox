//
//  USB.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "USB.h"
#import <IOKit/usb/IOUSBLib.h>
 
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

static Printing * _printer;

@interface USB ()

@end

@implementation USB

+ (void)setupWithPrinter:(Printing *)printer {

    // https://developer.apple.com/library/content/documentation/DeviceDrivers/Conceptual/USBBook/USBDeviceInterfaces/USBDevInterfaces.html#//apple_ref/doc/uid/TP40002645-TPXREF101
    
    _printer = printer;	
    mach_port_t             masterPort;
    CFMutableDictionaryRef  DFUMatchingDict;
    CFMutableDictionaryRef  CaterinaMatchingDict;
    CFMutableDictionaryRef  HalfkayMatchingDict;
    CFMutableDictionaryRef  STM32MatchingDict;
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
    }
}

static void DFUDeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"DFU device disconnected" withType:MessageType_Bootloader];
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
        [_printer print:@"Caterina device connected" withType:MessageType_Bootloader];
    }
}

static void CaterinaDeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"Caterina device disconnected" withType:MessageType_Bootloader];
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
    }
}

static void HalfkayDeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"Halfkay device disconnected" withType:MessageType_Bootloader];
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
    }
}

static void STM32DeviceRemoved(void *refCon, io_iterator_t iterator) {
    kern_return_t   kr;
    io_service_t    object;
 
    while ((object = IOIteratorNext(iterator)))
    {
        [_printer print:@"STM32 device disconnected" withType:MessageType_Bootloader];
        kr = IOObjectRelease(object);
        if (kr != kIOReturnSuccess)
        {
            printf("Couldn’t release raw device object: %08x\n", kr);
            continue;
        }
    }
}

@end
