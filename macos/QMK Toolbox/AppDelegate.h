#import <Cocoa/Cocoa.h>

#import "Printing.h"

@interface AppDelegate : NSObject <NSApplicationDelegate>
@property BOOL autoFlashEnabled;
@property BOOL canFlash;
@property BOOL canReset;
@property BOOL canClearEEPROM;

@property(nonatomic) Printing *printer;

- (void)setFilePath:(NSURL *)url;
@end
