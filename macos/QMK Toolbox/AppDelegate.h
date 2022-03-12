#import <Cocoa/Cocoa.h>

#import "Printing.h"

#define kShowAllDevices @"ShowAllDevices"

@interface AppDelegate : NSObject <NSApplicationDelegate>
@property BOOL autoFlashEnabled;
@property BOOL canFlash;
@property BOOL canReset;
@property BOOL canClearEEPROM;
@property BOOL showAllDevices;

@property(nonatomic) Printing *printer;

- (void)setFilePath:(NSURL *)url;
@end
