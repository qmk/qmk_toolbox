#import "HIDConsoleListener.h"

const ushort CONSOLE_USAGE_PAGE = 0xFF31;
const ushort CONSOLE_USAGE = 0x0074;

@implementation HIDConsoleListener
- (id)init {
    if (self = [super init]) {
        self.hidManagerRef = IOHIDManagerCreate(kCFAllocatorDefault, kIOHIDOptionsTypeNone);

        CFMutableDictionaryRef consoleMatcher = [self matchingDictionaryForUsagePage:CONSOLE_USAGE_PAGE usage:CONSOLE_USAGE];
        consoleMatcher = (CFMutableDictionaryRef) CFRetain(consoleMatcher);
        IOHIDManagerSetDeviceMatching(self.hidManagerRef, consoleMatcher);
        CFRelease(consoleMatcher);
    }
    return self;
}

- (void)start {
    if (self.devices == nil) {
        self.devices = [[NSMutableArray alloc] init];
    }

    IOHIDManagerScheduleWithRunLoop(self.hidManagerRef, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);
    IOHIDManagerOpen(self.hidManagerRef, kIOHIDOptionsTypeNone);

    IOHIDManagerRegisterDeviceMatchingCallback(self.hidManagerRef, deviceConnected, (__bridge void *)self);
    IOHIDManagerRegisterDeviceRemovalCallback(self.hidManagerRef, deviceDisconnected, (__bridge void *)self);
}

- (CFMutableDictionaryRef)matchingDictionaryForUsagePage:(ushort)usagePage usage:(ushort)usage {
    CFMutableDictionaryRef result = CFDictionaryCreateMutable(kCFAllocatorDefault, 0, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);

    CFNumberRef cfUsagePage = CFNumberCreate(kCFAllocatorDefault, kCFNumberShortType, &usagePage);
    CFDictionarySetValue(result, CFSTR(kIOHIDDeviceUsagePageKey), cfUsagePage);
    CFRelease(cfUsagePage);

    CFNumberRef cfUsage = CFNumberCreate(kCFAllocatorDefault, kCFNumberShortType, &usage);
    CFDictionarySetValue(result, CFSTR(kIOHIDDeviceUsageKey), cfUsage);
    CFRelease(cfUsage);

    return result;
}

static void deviceConnected(void *context, IOReturn result, void *sender, IOHIDDeviceRef device) {
    HIDConsoleListener *const listener = (__bridge HIDConsoleListener *const)context;

    BOOL alreadyListed = NO;
    for (HIDConsoleDevice *d in listener.devices) {
        if (d.deviceRef == device) {
            alreadyListed = YES;
            break;
        }
    }
    if (!alreadyListed) {
        HIDConsoleDevice *consoleDevice = [[HIDConsoleDevice alloc] initWithDeviceRef:device];
        consoleDevice.delegate = listener;
        [listener.devices addObject:consoleDevice];
        [listener.delegate consoleDeviceDidConnect:consoleDevice];
    }
}

static void deviceDisconnected(void *context, IOReturn result, void *sender, IOHIDDeviceRef device) {
    HIDConsoleListener *const listener = (__bridge HIDConsoleListener *const)context;

    for (HIDConsoleDevice *d in listener.devices) {
        if (d.deviceRef == device) {
            HIDConsoleDevice *tempDevice = [d copy];
            [listener.devices removeObject:d];
            [listener.delegate consoleDeviceDidDisconnect:tempDevice];
            break;
        }
    }
}

-(void)consoleDevice:(HIDConsoleDevice *)device didReceiveReport:(NSString *)report {
    [self.delegate consoleDevice:device didReceiveReport:report];
}

- (void)stop {
    IOHIDManagerRegisterDeviceMatchingCallback(self.hidManagerRef, NULL, (__bridge void *)self);
    IOHIDManagerRegisterDeviceRemovalCallback(self.hidManagerRef, NULL, (__bridge void *)self);
    IOHIDManagerUnscheduleFromRunLoop(self.hidManagerRef, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);
    IOHIDManagerClose(self.hidManagerRef, kIOHIDManagerOptionNone);
}
@end
