#import "AVRISPDevice.h"

@implementation AVRISPDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"AVR ISP";
        self.type = BootloaderTypeAVRISP;
        while (self.serialPort == nil) {
            self.serialPort = [self.usbDevice findSerialPort];
        }
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avrisp", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-P", self.serialPort, @"-C", @"avrdude.conf"]];
}

-(void)flashEEPROMWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avrisp", @"-U", [NSString stringWithFormat:@"eeprom:w:%@:i", file], @"-P", self.serialPort, @"-C", @"avrdude.conf"]];
}

- (NSString *)description {
    return [NSString stringWithFormat:@"%@ [%@]", [super description], self.serialPort];
}

@end
