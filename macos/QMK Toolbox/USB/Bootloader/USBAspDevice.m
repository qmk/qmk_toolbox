#import "USBAspDevice.h"

@implementation USBAspDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"USBasp";
        self.type = BootloaderTypeUsbAsp;
        self.eepromFlashable = YES;
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"usbasp", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-C", @"avrdude.conf"]];
}

-(void)flashEEPROMWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"usbasp", @"-U", [NSString stringWithFormat:@"eeprom:w:%@:i", file], @"-C", @"avrdude.conf"]];
}

@end
