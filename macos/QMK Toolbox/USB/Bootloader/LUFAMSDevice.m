#import <IOKit/storage/IOMedia.h>

#import "LUFAMSDevice.h"

@implementation LUFAMSDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"LUFA MS";
        self.type = BootloaderTypeLUFAMS;
        while (self.mountPoint == nil) {
            self.mountPoint = [self findMountPoint];
        }
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    if (self.mountPoint != nil) {
        if ([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
            NSString *destFile = [NSString stringWithFormat:@"%@/FLASH.BIN", self.mountPoint];
            NSError *error;

            [self printMessage:[NSString stringWithFormat:@"Deleting %@...", destFile] withType:MessageTypeCommand];
            if (![[NSFileManager defaultManager] removeItemAtPath:destFile error:&error]) {
                [self printMessage:[NSString stringWithFormat:@"IO ERROR: %@", [error localizedDescription]] withType:MessageTypeError];
                return;
            }

            [self printMessage:[NSString stringWithFormat:@"Copying %@ to %@...", file, destFile] withType:MessageTypeCommand];
            if (![[NSFileManager defaultManager] copyItemAtPath:file toPath:destFile error:&error]) {
                [self printMessage:[NSString stringWithFormat:@"IO ERROR: %@", [error localizedDescription]] withType:MessageTypeError];
                return;
            }

            [self printMessage:@"Done, please eject drive now." withType:MessageTypeBootloader];
        } else {
            [self printMessage:@"Only firmware files in .bin format can be flashed with this bootloader!" withType:MessageTypeError];
        }
    } else {
        [self printMessage:@"Could not find mount path for device!" withType:MessageTypeError];
    }
}

- (NSString *)description {
    return [NSString stringWithFormat:@"%@ [%@]", [super description], self.mountPoint];
}

- (NSString *)findMountPoint {
    CFMutableDictionaryRef massStorageMatcher = IOServiceMatching(kIOMediaClass);
    CFDictionarySetValue(massStorageMatcher, CFSTR(kIOMediaRemovableKey), kCFBooleanTrue);

    io_iterator_t massStorageIterator;
    IOServiceGetMatchingServices(kIOMasterPortDefault, massStorageMatcher, &massStorageIterator);

    io_service_t media;
    while ((media = IOIteratorNext(massStorageIterator))) {
        ushort parentVendorID = [self.usbDevice vendorIDForService:media];
        ushort parentProductID = [self.usbDevice productIDForService:media];

        if (parentVendorID == self.vendorID && parentProductID == self.productID) {
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
    IOObjectRelease(media);

    return nil;
}

@end
