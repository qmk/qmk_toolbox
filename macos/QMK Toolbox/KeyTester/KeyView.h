#import <Cocoa/Cocoa.h>

IB_DESIGNABLE
@interface KeyView : NSView
@property IBInspectable BOOL pressed;
@property IBInspectable BOOL tested;
@property IBInspectable NSString *legend;
@end
