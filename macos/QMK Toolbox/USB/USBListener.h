#import <Foundation/Foundation.h>

#import "BootloaderDevice.h"
#import "USBDevice.h"

@protocol USBListenerDelegate <NSObject>
- (void)usbDeviceDidConnect:(USBDevice *)device;

- (void)usbDeviceDidDisconnect:(USBDevice *)device;

- (void)bootloaderDeviceDidConnect:(BootloaderDevice *)device;

- (void)bootloaderDeviceDidDisconnect:(BootloaderDevice *)device;

- (void)bootloaderDevice:(BootloaderDevice *)device didReceiveCommandOutput:(NSString *)data messageType:(MessageType)type;
@end

@interface USBListener : NSObject <BootloaderDeviceDelegate>

@property (nonatomic, weak) id<USBListenerDelegate> delegate;

@property NSMutableArray<id<USBDevice>> *devices;

- (void)start;

- (void)stop;

@end
