#import "GD32VDFUDevice.h"

@implementation GD32VDFUDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"GD32V DFU";
        self.type = BootloaderTypeGD32VDFU;
        self.resettable = YES;
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"28E9:0189", @"-s", @"0x8000000:leave", @"-D", file]];
    } else {
        [self printMessage:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageType_Error];
    }
}

-(void)resetWithMCU:(NSString *)mcu {
    [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"28E9:0189", @"-s", @"0x8000000:leave"]];
}

@end
