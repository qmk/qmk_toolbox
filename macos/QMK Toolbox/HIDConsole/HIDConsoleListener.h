#import <IOKit/hid/IOHIDManager.h>

#import "HIDConsoleDevice.h"

@protocol HIDConsoleListenerDelegate <NSObject>
- (void)consoleDeviceDidConnect:(HIDConsoleDevice *)device;

- (void)consoleDeviceDidDisconnect:(HIDConsoleDevice *)device;

- (void)consoleDevice:(HIDConsoleDevice *)device didReceiveReport:(NSString *)report;
@end

@interface HIDConsoleListener : NSObject <HIDConsoleDeviceDelegate>
@property (nonatomic, weak) id <HIDConsoleListenerDelegate> delegate;

@property IOHIDManagerRef hidManagerRef;

@property NSMutableArray<HIDConsoleDevice *> *devices;

- (void)start;

- (void)stop;
@end
