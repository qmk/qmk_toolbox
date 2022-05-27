#import "BootloaderDevice.h"

@implementation BootloaderDevice

@synthesize service;

@synthesize manufacturerString;

@synthesize productString;

@synthesize vendorID;

@synthesize productID;

@synthesize revisionBCD;

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super init]) {
        self.usbDevice = usbDevice;
    }
    return self;
}

- (io_service_t)service {
    return self.usbDevice.service;
}

- (NSString *)manufacturerString {
    return self.usbDevice.manufacturerString;
}

- (NSString *)productString {
    return self.usbDevice.productString;
}

- (ushort)vendorID {
    return self.usbDevice.vendorID;
}

- (ushort)productID {
    return self.usbDevice.productID;
}

- (ushort)revisionBCD {
    return self.usbDevice.revisionBCD;
}

- (NSString *)description {
    return [self.usbDevice description];
}

- (void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [NSException raise:NSInternalInconsistencyException format:@"You must override %@ in a subclass", NSStringFromSelector(_cmd)];
}

- (void)flashEEPROMWithMCU:(NSString *)mcu file:(NSString *)file {
    [NSException raise:NSInternalInconsistencyException format:@"You must override %@ in a subclass", NSStringFromSelector(_cmd)];
}

- (void)resetWithMCU:(NSString *)mcu {
    [NSException raise:NSInternalInconsistencyException format:@"You must override %@ in a subclass", NSStringFromSelector(_cmd)];
}

- (void)runProcess:(NSString *)command withArgs:(NSArray<NSString *> *)args {
    [self printMessage:[NSString stringWithFormat:@"%@ %@", command, [args componentsJoinedByString:@" "]] withType:MessageTypeCommand];

    NSTask *task = [[NSTask alloc] init];
    task.executableURL = [[NSBundle mainBundle] URLForResource:command withExtension:@""];
    task.currentDirectoryURL = [[NSBundle mainBundle] resourceURL];
    task.arguments = args;

    NSPipe *outPipe = [NSPipe pipe];
    task.standardOutput = outPipe;
    NSPipe *errPipe = [NSPipe pipe];
    task.standardError = errPipe;

    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);

    outPipe.fileHandleForReading.readabilityHandler = ^(NSFileHandle *handle) {
        NSData *data = [handle readDataOfLength:NSUIntegerMax];
        if (data.length == 0) {
            handle.readabilityHandler = nil;
            dispatch_semaphore_signal(semaphore);
        } else {
            NSString *str = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
            [self printOutput:str];
        }
    };
    errPipe.fileHandleForReading.readabilityHandler = ^(NSFileHandle *handle) {
        NSData *data = [handle readDataOfLength:NSUIntegerMax];
        if (data.length == 0) {
            handle.readabilityHandler = nil;
            dispatch_semaphore_signal(semaphore);
        } else {
            NSString *str = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
            [self printErrorOutput:str];
        }
    };

    [task launchAndReturnError:nil];

    dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
    dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
}

- (void)printOutput:(NSString *)output {
    [self printMessage:output withType:MessageTypeCommandOutput];
}

- (void)printErrorOutput:(NSString *)output {
    [self printMessage:output withType:MessageTypeCommandError];
}

- (void)printMessage:(NSString *)message withType:(MessageType)type {
    [self.delegate bootloaderDevice:self didReceiveCommandOutput:message messageType:type];
}

- (NSString *)findSerialPort {
    CFMutableDictionaryRef serialMatcher = IOServiceMatching(kIOSerialBSDServiceValue);
    CFDictionarySetValue(serialMatcher, CFSTR(kIOSerialBSDTypeKey), CFSTR(kIOSerialBSDAllTypes));

    io_iterator_t serialIterator;
    IOServiceGetMatchingServices(kIOMasterPortDefault, serialMatcher, &serialIterator);

    io_service_t port;
    while ((port = IOIteratorNext(serialIterator))) {
        ushort parentVendorID = [self.usbDevice vendorIDForService:port];
        ushort parentProductID = [self.usbDevice productIDForService:port];

        if (parentVendorID == self.vendorID && parentProductID == self.productID) {
            return [self.usbDevice stringProperty:@kIOCalloutDeviceKey forService:port];
        }
    }
    return nil;
}

@end
