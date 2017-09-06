//
//  HID.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "HID.h"
#import <CoreFoundation/CoreFoundation.h>
#import <Carbon/Carbon.h>
#import <IOKit/hid/IOHIDLib.h>

@interface HID ()

@end

@implementation HID

Printing * _printer;
IOHIDManagerRef _hidManager;

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
    return [NSString stringWithFormat:@"%@ %@ - %@ -- 0x%X:0x%X",
        str,
        IOHIDDeviceGetProperty(device, CFSTR(kIOHIDManufacturerKey)),
        IOHIDDeviceGetProperty(device, CFSTR(kIOHIDProductKey)),
        (int)IOHIDDeviceGetProperty(device, CFSTR(kIOHIDVendorIDKey)) / 256,
        (int)IOHIDDeviceGetProperty(device, CFSTR(kIOHIDProductIDKey)) / 256
    ];
}

static void HIDConnected(void *context, IOReturn result, void *sender, IOHIDDeviceRef device) {
    [_printer print:formatDevice(@"Connected:   ", device)  withType:MessageType_HID];
}

static void HIDDisconnected(void *context, IOReturn result, void *sender, IOHIDDeviceRef device) {
    [_printer print:formatDevice(@"Disconnected:", device)  withType:MessageType_HID];
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
