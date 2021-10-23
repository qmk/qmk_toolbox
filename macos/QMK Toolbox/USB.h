#import <Foundation/Foundation.h>

#import "Printing.h"
#import "Flashing.h"

@class USB;

@protocol USBDelegate <NSObject>
@optional
- (void)deviceConnected:(Chipset)chipset;

- (void)deviceDisconnected:(Chipset)chipset;

- (void)setSerialPort:(NSString *)port;

- (void)setMountPoint:(NSString *)mountPoint;
@end

@interface USB : NSObject
+ (void)setupWithPrinter:(Printing *)printer andDelegate:(id<USBDelegate>)d;

+ (BOOL)areDevicesAvailable;

+ (BOOL)canFlash:(Chipset)chipset;
@end

static id<USBDelegate> delegate;
