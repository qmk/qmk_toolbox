#import "CaterinaDevice.h"

@implementation CaterinaDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"Caterina";
        self.type = BootloaderTypeCaterina;
        self.eepromFlashable = YES;
        while (self.serialPort == nil) {
            self.serialPort = [self.usbDevice findSerialPort];
        }
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-P", self.serialPort, @"-C", @"avrdude.conf"]];
}

-(void)flashEEPROMWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", [NSString stringWithFormat:@"eeprom:w:%@:i", file], @"-P", self.serialPort, @"-C", @"avrdude.conf"]];
}

- (NSString *)description {
    return [NSString stringWithFormat:@"%@ [%@]", [super description], self.serialPort];
}

@end
