#import "LUFAHIDDevice.h"

@implementation LUFAHIDDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        if ([self revisionBCD] == 0x0936) {
            self.name = @"QMK HID";
            self.type = BootloaderTypeQmkHid;
        } else {
            self.name = @"LUFA HID";
            self.type = BootloaderTypeLufaHid;
        }
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"hid_bootloader_cli" withArgs:@[[NSString stringWithFormat:@"-mmcu=%@", mcu], file, @"-v"]];
}

// hid_bootloader_cli 210130 lacks -b flag
// Next LUFA release should have it thanks to abcminiuser/lufa#173
//-(void)resetWithMCU:(NSString *)mcu {
//    [self runProcess:@"hid_bootloader_cli" withArgs:@[[NSString stringWithFormat:@"-mmcu=%@", mcu], @"-bv"]];
//}

@end
