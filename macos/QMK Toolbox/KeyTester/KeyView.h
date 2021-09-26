#import <Cocoa/Cocoa.h>

IB_DESIGNABLE
@interface KeyView : NSView
@property IBInspectable (nonatomic) BOOL pressed;
@property IBInspectable (nonatomic) BOOL tested;
@property IBInspectable (nonatomic) NSString *legend;
@end
