#import <Foundation/Foundation.h>

#import <IOKit/hid/IOHIDDevice.h>

@class HIDConsoleDevice;

@protocol HIDConsoleDeviceDelegate <NSObject>
- (void)consoleDevice:(HIDConsoleDevice *)device didReceiveReport:(NSString *)report;
@end

@interface HIDConsoleDevice : NSObject <NSCopying>
@property (nonatomic, assign) id <HIDConsoleDeviceDelegate> delegate;

@property IOHIDDeviceRef deviceRef;

@property NSString *manufacturerString;

@property NSString *productString;

@property ushort vendorID;

@property ushort productID;

@property ushort revisionBCD;

-(id)initWithDeviceRef:(IOHIDDeviceRef)device;
@end
