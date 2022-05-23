#import "AtmelDFUDevice.h"

@implementation AtmelDFUDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        if ([self revisionBCD] == 0x0936) {
            self.name = @"QMK DFU";
            self.type = BootloaderTypeQmkDfu;
        } else {
            self.name = @"Atmel DFU";
            self.type = BootloaderTypeAtmelDfu;
        }
        self.eepromFlashable = YES;
        self.resettable = YES;
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"erase", @"--force"]];
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"flash", @"--force", file]];
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"reset"]];
}

-(void)flashEEPROMWithMCU:(NSString *)mcu file:(NSString *)file {
    if (self.type == BootloaderTypeAtmelDfu) {
        [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"erase", @"--force"]];
    }

    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"flash", @"--force", @"--suppress-validation", @"--eeprom", file]];

    if (self.type == BootloaderTypeAtmelDfu) {
        [self printMessage:@"Please reflash device with firmware now" withType:MessageTypeBootloader];
    }
}

-(void)resetWithMCU:(NSString *)mcu {
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"reset"]];
}

@end
