#import <Foundation/Foundation.h>
#import <IOKit/serial/IOSerialKeys.h>

#import "BootloaderType.h"
#import "USBDevice.h"
#import "MessageType.h"

@class BootloaderDevice;

@protocol BootloaderDeviceDelegate <NSObject>
- (void)bootloaderDevice:(BootloaderDevice *)device didReceiveCommandOutput:(NSString *)data messageType:(MessageType)type;
@end

@interface BootloaderDevice : NSObject<USBDevice>
@property (nonatomic, assign) id<BootloaderDeviceDelegate> delegate;

@property USBDevice *usbDevice;

@property NSString *name;

@property BootloaderType type;

@property bool eepromFlashable;

@property bool resettable;

- (id)initWithUSBDevice:(USBDevice *)usbDevice;

- (void)flashWithMCU:(NSString *)mcu file:(NSString *)file;

- (void)flashEEPROMWithMCU:(NSString *)mcu file:(NSString *)file;

- (void)resetWithMCU:(NSString *)mcu;

- (void)runProcess:(NSString *)command withArgs:(NSArray<NSString *> *)args;

- (void)printMessage:(NSString *)message withType:(MessageType)type;

- (NSString *)findSerialPort;

@end
