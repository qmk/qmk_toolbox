#import <Cocoa/Cocoa.h>

#define kShowAllDevices @"ShowAllDevices"

@interface AppDelegate : NSObject <NSApplicationDelegate>
@property BOOL autoFlashEnabled;
@property BOOL canFlash;
@property BOOL canReset;
@property BOOL canClearEEPROM;
@property BOOL showAllDevices;

- (void)setFilePath:(NSURL *)url;
@end
