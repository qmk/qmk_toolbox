#import <Cocoa/Cocoa.h>

#define kMicrocontroller @"Microcontroller"

@interface MicrocontrollerSelector : NSComboBox
- (NSString *)keyForSelectedItem;
@end
