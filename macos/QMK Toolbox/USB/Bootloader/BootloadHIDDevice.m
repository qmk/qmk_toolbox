#import "BootloadHIDDevice.h"

@implementation BootloadHIDDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"BootloadHID";
        self.type = BootloaderTypeBootloadHID;
        self.resettable = YES;
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    [self runProcess:@"bootloadHID" withArgs:@[@"-r", file]];
}

-(void)resetWithMCU:(NSString *)mcu {
    [self runProcess:@"bootloadHID" withArgs:@[@"-r"]];
}

@end
