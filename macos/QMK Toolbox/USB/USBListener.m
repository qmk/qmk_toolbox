#import <IOKit/usb/IOUSBLib.h>

#import "USBListener.h"

#import "APM32DFUDevice.h"
#import "AtmelDFUDevice.h"
#import "AtmelSAMBADevice.h"
#import "AVRISPDevice.h"
#import "BootloadHIDDevice.h"
#import "CaterinaDevice.h"
#import "GD32VDFUDevice.h"
#import "HalfKayDevice.h"
#import "KiibohdDFUDevice.h"
#import "LUFAHIDDevice.h"
#import "LUFAMSDevice.h"
#import "STM32DFUDevice.h"
#import "STM32DuinoDevice.h"
#import "USBAspDevice.h"
#import "USBTinyISPDevice.h"
#import "WB32DFUDevice.h"

@implementation USBListener {
    mach_port_t masterPort;
    IONotificationPortRef notificationPort;
    CFRunLoopSourceRef runLoopSource;
    CFMutableDictionaryRef usbMatcher;

    io_iterator_t usbConnectedIter;
    io_iterator_t usbDisconnectedIter;
}

- (id)init {
    if (self = [super init]) {
        IOMasterPort(MACH_PORT_NULL, &masterPort);

        usbMatcher = IOServiceMatching(kIOUSBDeviceClassName);
        // Retains for additional IOServiceAddMatchingNotification calls
        usbMatcher = (CFMutableDictionaryRef) CFRetain(usbMatcher);
        usbMatcher = (CFMutableDictionaryRef) CFRetain(usbMatcher);
        usbMatcher = (CFMutableDictionaryRef) CFRetain(usbMatcher);

        notificationPort = IONotificationPortCreate(masterPort);
        runLoopSource = IONotificationPortGetRunLoopSource(notificationPort);
        CFRunLoopAddSource(CFRunLoopGetCurrent(), runLoopSource, kCFRunLoopDefaultMode);
    }
    return self;
}

- (void)bootloaderDevice:(BootloaderDevice *)device didReceiveCommandOutput:(NSString *)data messageType:(MessageType)type {
    [self.delegate bootloaderDevice:device didReceiveCommandOutput:data messageType:type];
}

- (void)start {
    if (self.devices == nil) {
        self.devices = [[NSMutableArray alloc] init];
    }

    IOServiceAddMatchingNotification(notificationPort, kIOFirstMatchNotification, usbMatcher, deviceConnected, (__bridge void *)self, &usbConnectedIter);
    deviceConnected((__bridge void *)self, usbConnectedIter);

    IOServiceAddMatchingNotification(notificationPort, kIOTerminatedNotification, usbMatcher, deviceDisconnected, (__bridge void *)self, &usbDisconnectedIter);
    deviceDisconnected((__bridge void *)self, usbDisconnectedIter);
}

static void deviceConnected(void *context, io_iterator_t iterator) {
    USBListener *const listener = (__bridge USBListener *const)context;

    io_service_t service;
    while ((service = IOIteratorNext(iterator))) {
        CFStringRef className = IOObjectCopyClass(service);
        if (CFEqual(className, CFSTR(kIOUSBDeviceClassName)) || CFEqual(className, CFSTR(kIOUSBHostDeviceClassName))) {
            BOOL alreadyListed = NO;
            for (USBDevice *d in listener.devices) {
                if (d.service == service) {
                    alreadyListed = YES;
                    break;
                }
            }
            if (!alreadyListed) {
                id<USBDevice> usbDevice = [listener createDevice:service];
                [listener.devices addObject:usbDevice];
                if ([usbDevice isKindOfClass:[BootloaderDevice class]]) {
                    [listener.delegate bootloaderDeviceDidConnect:usbDevice];
                    ((BootloaderDevice *)usbDevice).delegate = listener;
                } else {
                    [listener.delegate usbDeviceDidConnect:usbDevice];
                }
            }
        }
    }
}

static void deviceDisconnected(void *context, io_iterator_t iterator) {
    USBListener *const listener = (__bridge USBListener *const)context;

    io_service_t service;
    while ((service = IOIteratorNext(iterator))) {
        CFStringRef className = IOObjectCopyClass(service);
        if (CFEqual(className, CFSTR(kIOUSBDeviceClassName)) || CFEqual(className, CFSTR(kIOUSBHostDeviceClassName))) {
            NSMutableArray<id<USBDevice>> *discardedItems = [NSMutableArray array];
            for (id<USBDevice> d in listener.devices) {
                if (d.service == service) {
                    [discardedItems addObject:d];
                }
            }

            [listener.devices removeObjectsInArray:discardedItems];
            for (id<USBDevice> d in discardedItems) {
                if ([d isKindOfClass:[BootloaderDevice class]]) {
                    [listener.delegate bootloaderDeviceDidDisconnect:d];
                } else {
                    [listener.delegate usbDeviceDidDisconnect:d];
                }
            }
        }
    }
}

- (void)stop {
    // ??????
    IOServiceAddMatchingNotification(notificationPort, kIOFirstMatchNotification, usbMatcher, NULL, NULL, &usbConnectedIter);
    IOServiceAddMatchingNotification(notificationPort, kIOTerminatedNotification, usbMatcher, NULL, NULL, &usbConnectedIter);

    mach_port_deallocate(mach_task_self(), masterPort);
    masterPort = 0;
}

- (id<USBDevice>)createDevice:(io_service_t)service {
    USBDevice *usbDevice = [[USBDevice alloc] initWithService:service];

    switch ([self deviceTypeForVendorID:usbDevice.vendorID productID:usbDevice.productID revisionBCD:usbDevice.revisionBCD]) {
        case BootloaderTypeApm32Dfu:
            return [[APM32DFUDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeAtmelDfu:
        case BootloaderTypeQmkDfu:
            return [[AtmelDFUDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeAtmelSamBa:
            return [[AtmelSAMBADevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeAvrIsp:
            return [[AVRISPDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeBootloadHid:
            return [[BootloadHIDDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeCaterina:
            return [[CaterinaDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeGD32VDFU:
            return [[GD32VDFUDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeHalfKay:
            return [[HalfKayDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeKiibohdDfu:
            return [[KiibohdDFUDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeLufaHid:
        case BootloaderTypeQmkHid:
            return [[LUFAHIDDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeLufaMs:
            return [[LUFAMSDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeStm32Dfu:
            return [[STM32DFUDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeStm32duino:
            return [[STM32DuinoDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeUsbAsp:
            return [[USBAspDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeUsbTinyIsp:
            return [[USBTinyISPDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeWb32Dfu:
            return [[WB32DFUDevice alloc] initWithUSBDevice:usbDevice];
        case BootloaderTypeNone:
        default:
            return usbDevice;
    }
}

- (BootloaderType)deviceTypeForVendorID:(ushort)vendorID productID:(ushort)productID revisionBCD:(ushort)revisionBCD {
    switch (vendorID) {
        case 0x03EB: // Atmel Corporation
            switch (productID) {
                case 0x2045:
                    return BootloaderTypeLufaMs;
                case 0x2067:
                    if (revisionBCD == 0x0936) { // Unicode Ψ
                        return BootloaderTypeQmkHid;
                    }
                    return BootloaderTypeLufaHid;
                case 0x2FEF: // ATmega16U2
                case 0x2FF0: // ATmega32U2
                case 0x2FF3: // ATmega16U4
                case 0x2FF4: // ATmega32U4
                case 0x2FF9: // AT90USB64
                case 0x2FFA: // AT90USB162
                case 0x2FFB: // AT90USB128
                    if (revisionBCD == 0x0936) { // Unicode Ψ
                        return BootloaderTypeQmkDfu;
                    }

                    return BootloaderTypeAtmelDfu;
                case 0x6124:
                    return BootloaderTypeAtmelSamBa;
            }
            break;
        case 0x0483: // STMicroelectronics
            if (productID == 0xDF11) {
                return BootloaderTypeStm32Dfu;
            }
            break;
        case 0x1209: // pid.codes
            if (productID == 0x2302) { // Keyboardio Atreus 2 Bootloader
                return BootloaderTypeCaterina;
            }
            break;
        case 0x16C0: // Van Ooijen Technische Informatica
            switch (productID) {
                case 0x0478:
                    return BootloaderTypeHalfKay;
                case 0x0483:
                    return BootloaderTypeAvrIsp;
                case 0x05DC:
                    return BootloaderTypeUsbAsp;
                case 0x05DF:
                    return BootloaderTypeBootloadHid;
            }
            break;
        case 0x1781: // MECANIQUE
            if (productID == 0x0C9F) {
                return BootloaderTypeUsbTinyIsp;
            }
            break;
        case 0x1B4F: // Spark Fun Electronics
            switch (productID) {
                case 0x9203: // Pro Micro 3V3/8MHz
                case 0x9205: // Pro Micro 5V/16MHz
                case 0x9207: // LilyPad 3V3/8MHz (and some Pro Micro clones)
                    return BootloaderTypeCaterina;
            }
            break;
        case 0x1C11: // Input Club Inc.
            if (productID == 0xB007) {
                return BootloaderTypeKiibohdDfu;
            }
            break;
        case 0x1EAF: // Leaflabs
            if (productID == 0x0003) {
                return BootloaderTypeStm32duino;
            }
            break;
        case 0x1FFB: // Pololu Corporation
            if (productID == 0x0101) { // A-Star 32U4
                return BootloaderTypeCaterina;
            }
            break;
        case 0x2341: // Arduino SA
        case 0x2A03: // dog hunter AG
            switch (productID) {
                case 0x0036: // Leonardo
                case 0x0037: // Micro
                    return BootloaderTypeCaterina;
            }
            break;
        case 0x239A: // Adafruit
            switch (productID) {
                case 0x000C: // Feather 32U4
                case 0x000D: // ItsyBitsy 32U4 3V3/8MHz
                case 0x000E: // ItsyBitfy 32U4 5V/16MHz
                    return BootloaderTypeCaterina;
            }
            break;
        case 0x28E9: // GigaDevice Semiconductor (Beijing) Inc.
            if (productID == 0x0189) { // GD32VF103
                return BootloaderTypeGD32VDFU;
            }
            break;
        case 0x314B: // Geehy Semiconductor Co. Ltd.
            if (productID == 0x0106) {
                return BootloaderTypeApm32Dfu;
            }
            break;
        case 0x342D: // WestBerryTech
            if (productID == 0xDFA0) {
                return BootloaderTypeWb32Dfu;
            }
            break;
    }

    return BootloaderTypeNone;
}

@end
