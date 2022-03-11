#import <Cocoa/Cocoa.h>
#import "MessageType.h"

@interface Printing : NSObject

- (id)initWithTextView:(NSTextView *)view;

- (void)print:(NSString *)str withType:(MessageType)type;
- (void)printResponse:(NSString *)str withType:(MessageType)type;

@end
