#import "KiibohdDFUDevice.h"

@implementation KiibohdDFUDevice

- (id)initWithUSBDevice:(USBDevice *)usbDevice {
    if (self = [super initWithUSBDevice:usbDevice]) {
        self.name = @"Kiibohd DFU";
        self.type = BootloaderTypeKiibohdDFU;
        self.resettable = YES;
    }
    return self;
}

-(void)flashWithMCU:(NSString *)mcu file:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"1C11:B007", @"-D", file]];
    } else {
        [self printMessage:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageType_Error];
    }
}

-(void)resetWithMCU:(NSString *)mcu {
    [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"1C11:B007", @"-e"]];
}

@end
