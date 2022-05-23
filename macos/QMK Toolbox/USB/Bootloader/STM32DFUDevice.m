#import "STM32DFUDevice.h"

@implementation STM32DFUDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"STM32 DFU";
        self.type = BootloaderTypeStm32Dfu;
        self.resettable = YES;
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"0483:DF11", @"-s", @"0x8000000:leave", @"-D", file]];
    } else {
        [self printMessage:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageTypeError];
    }
}

-(void)resetWithMCU:(NSString *)mcu {
    [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"0483:DF11", @"-s", @"0x8000000:leave"]];
}

@end
