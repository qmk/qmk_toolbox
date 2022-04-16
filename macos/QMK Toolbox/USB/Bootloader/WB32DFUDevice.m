#import "WB32DFUDevice.h"

@implementation WB32DFUDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"WB32 DFU";
        self.type = BootloaderTypeWB32DFU;
        self.resettable = YES;
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"wb32-dfu-updater_cli" withArgs:@[@"--toolbox-mode", @"--dfuse-address", @"0x08000000", @"--download", file]];
    } else if([[[file pathExtension] lowercaseString] isEqualToString:@"hex"]) {
        [self runProcess:@"wb32-dfu-updater_cli" withArgs:@[@"--toolbox-mode", @"--download", file]];
    } else {
        [self printMessage:@"This file format is not the type supported by wB32-dFU-updater_cli!" withType:MessageType_Error];
    }
}

-(void)resetWithMCU:(NSString *)mcu {
    [self runProcess:@"wb32-dfu-updater_cli" withArgs:@[@"--reset"]];
}

@end
