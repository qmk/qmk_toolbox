#import <Cocoa/Cocoa.h>

#import "MessageType.h"

@interface LogTextView : NSTextView

- (void)logBootloader:(NSString *)message;

- (void)logCommand:(NSString *)message;

- (void)logCommandError:(NSString *)message;

- (void)logCommandOutput:(NSString *)message;

- (void)logError:(NSString *)message;

- (void)logHID:(NSString *)message;

- (void)logHIDOutput:(NSString *)message;

- (void)logInfo:(NSString *)message;

- (void)logUSB:(NSString *)message;

- (void)log:(NSString *)message withType:(MessageType)type;

- (void)appendString:(NSString *)string withColor:(NSColor *)color;

@end
