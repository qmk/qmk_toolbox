#import <Cocoa/Cocoa.h>

@interface Printing : NSObject

typedef enum {
    MessageType_Info,
    MessageType_Error,
    MessageType_HID,
    MessageType_Bootloader,
    MessageType_Command
} MessageType;

- (id)initWithTextView:(NSTextView *)view;

- (void)print:(NSString *)str withType:(MessageType)type;
- (void)printResponse:(NSString *)str withType:(MessageType)type;

@end
