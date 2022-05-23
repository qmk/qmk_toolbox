#import "AtmelSAMBADevice.h"

@implementation AtmelSAMBADevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"Atmel SAM-BA";
        self.type = BootloaderTypeAtmelSamBa;
        self.resettable = YES;
        while (self.serialPort == nil) {
            self.serialPort = [self findSerialPort];
        }
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"mdloader" withArgs:@[@"-p", self.serialPort, @"-D", file, @"--restart"]];
}

-(void)resetWithMCU:(NSString *)mcu {
    [self runProcess:@"mdloader" withArgs:@[@"-p", self.serialPort, @"--restart"]];
}

- (NSString *)description {
    return [NSString stringWithFormat:@"%@ [%@]", [super description], self.serialPort];
}

@end
