#import "HalfKayDevice.h"

@implementation HalfKayDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"HalfKay";
        self.type = BootloaderTypeHalfKay;
        self.resettable = YES;
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"teensy_loader_cli" withArgs:@[[NSString stringWithFormat:@"-mmcu=%@", mcu], file, @"-v"]];
}

-(void)resetWithMCU:(NSString *)mcu {
    [self runProcess:@"teensy_loader_cli" withArgs:@[[NSString stringWithFormat:@"-mmcu=%@", mcu], @"-bv"]];
}

@end
