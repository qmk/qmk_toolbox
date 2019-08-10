//
//  HID.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import "HID.h"
#import <CoreFoundation/CoreFoundation.h>
#import <Carbon/Carbon.h>
#import <IOKit/hid/IOHIDLib.h>
#import <IOKit/usb/IOUSBLib.h>

static Printing * _printer;
static IOHIDManagerRef _hidManager;

@interface HID ()

@end

@implementation HID

+ (void)setupWithPrinter:(Printing *)printer {
    _printer = printer;	
    
    _hidManager = IOHIDManagerCreate(kCFAllocatorDefault, kIOHIDOptionsTypeNone);
    // Make sure we detect ANY type of 'game controller'
    NSArray *criteria = [NSArray arrayWithObjects:
            CFBridgingRelease(hu_CreateMatchingDictionaryUsagePageUsage(true, 0xFF31, 0x0074)),
        nil];
    
    IOHIDManagerSetDeviceMatchingMultiple(_hidManager, (__bridge CFArrayRef)criteria);

    IOHIDManagerScheduleWithRunLoop(_hidManager, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);
    IOHIDManagerOpen(_hidManager, kIOHIDOptionsTypeNone);

    // Register callbacks
    IOHIDManagerRegisterDeviceMatchingCallback(_hidManager, HIDConnected, (__bridge void *)self);
    IOHIDManagerRegisterDeviceRemovalCallback(_hidManager, HIDDisconnected, (__bridge void *)self);
    IOHIDManagerRegisterInputReportCallback(_hidManager, HIDReported, (__bridge void *)self);

}

static NSString * formatDevice(NSString * str, IOHIDDeviceRef device) {
    unsigned short vendorId = [(NSNumber *)IOHIDDeviceGetProperty(device, CFSTR(kIOHIDVendorIDKey)) shortValue];
    unsigned short productId = [(NSNumber *)IOHIDDeviceGetProperty(device, CFSTR(kIOHIDProductIDKey)) shortValue];
    return [NSString stringWithFormat:@"%@ - %@ %@ -- %04X:%04X",
        IOHIDDeviceGetProperty(device, CFSTR(kIOHIDManufacturerKey)),
        IOHIDDeviceGetProperty(device, CFSTR(kIOHIDProductKey)),
        str,
        vendorId,
        productId
    ];
}

static void HIDConnected(void *context, IOReturn result, void *sender, IOHIDDeviceRef device) {
    [_printer print:formatDevice(@"connected", device)  withType:MessageType_HID];
}

static void HIDDisconnected(void *context, IOReturn result, void *sender, IOHIDDeviceRef device) {
    [_printer print:formatDevice(@"disconnected", device)  withType:MessageType_HID];
}

static void HIDReported(void *context, IOReturn result, void *sender, IOHIDReportType type, uint32_t reportID, uint8_t *report, CFIndex reportLength) {
    [_printer printResponse:[NSString stringWithFormat:@"%s", (char *)report] withType:MessageType_HID];
}

static CFMutableDictionaryRef hu_CreateMatchingDictionaryUsagePageUsage( Boolean isDeviceNotElement,
                                                                        UInt32 inUsagePage,
                                                                        UInt32 inUsage )
{
    // create a dictionary to add usage page / usages to
    CFMutableDictionaryRef result = CFDictionaryCreateMutable( kCFAllocatorDefault,
                                                              0,
                                                              &kCFTypeDictionaryKeyCallBacks,
                                                              &kCFTypeDictionaryValueCallBacks );
    
    if ( result ) {
        if ( inUsagePage ) {
            // Add key for device type to refine the matching dictionary.
            CFNumberRef pageCFNumberRef = CFNumberCreate( kCFAllocatorDefault, kCFNumberIntType, &inUsagePage );
            
            if ( pageCFNumberRef ) {
                if ( isDeviceNotElement ) {
                    CFDictionarySetValue( result, CFSTR( kIOHIDDeviceUsagePageKey ), pageCFNumberRef );
                } else {
                    CFDictionarySetValue( result, CFSTR( kIOHIDElementUsagePageKey ), pageCFNumberRef );
                }
                CFRelease( pageCFNumberRef );
                
                // note: the usage is only valid if the usage page is also defined
                if ( inUsage ) {
                    CFNumberRef usageCFNumberRef = CFNumberCreate( kCFAllocatorDefault, kCFNumberIntType, &inUsage );
                    
                    if ( usageCFNumberRef ) {
                        if ( isDeviceNotElement ) {
                            CFDictionarySetValue( result, CFSTR( kIOHIDDeviceUsageKey ), usageCFNumberRef );
                        } else {
                            CFDictionarySetValue( result, CFSTR( kIOHIDElementUsageKey ), usageCFNumberRef );
                        }
                        CFRelease( usageCFNumberRef );
                    } else {
                        fprintf( stderr, "%s: CFNumberCreate( usage ) failed.", __PRETTY_FUNCTION__ );
                    }
                }
            } else {
                fprintf( stderr, "%s: CFNumberCreate( usage page ) failed.", __PRETTY_FUNCTION__ );
            }
        }
    } else {
        fprintf( stderr, "%s: CFDictionaryCreateMutable failed.", __PRETTY_FUNCTION__ );
    }
    return result;
}   // hu_CreateMatchingDictionaryUsagePageUsage


@end
