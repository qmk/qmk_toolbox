//
//  AppDelegate.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "AppDelegate.h"

@interface AppDelegate () <NSTextViewDelegate>

@property (weak) IBOutlet NSWindow *window;
@property IBOutlet NSTextView * textView;
@property Flashing * flasher;

@end

@implementation AppDelegate


- (BOOL)application:(NSApplication *)sender openFile:(NSString *)filename {
    if ([[[filename pathExtension] lowercaseString] isEqualToString:@"qmk"] ||
    [[[filename pathExtension] lowercaseString] isEqualToString:@"hex"] ||
    [[[filename pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self setFilePath:filename];
        return true;
    } else {
        return false;
    }
}

- (void)setFilePath:(NSString *)str {

}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
    // Insert code here to initialize your application
    
    _printer = [[Printing alloc] initWithTextView:_textView];
    _flasher = [[Flashing alloc] initWithPrinter:_printer];
    

    [_printer print:@"QMK Toolbox" withType:MessageType_Info];
    [_printer printResponse:@"Supports the following bootloaders:" withType:MessageType_Info];
    [_printer printResponse:@" - DFU" withType:MessageType_Info];
    [_printer printResponse:@" - Halfkay" withType:MessageType_Info];
    [_printer printResponse:@" - Caterina" withType:MessageType_Info];
    [_printer printResponse:@" - STM32" withType:MessageType_Info];
    
    [_flasher runProcess:@"dfu-programmer" withArgs:@[@"--help"]];
    [_flasher runProcess:@"avrdude" withArgs:@[@"-C", [[NSBundle mainBundle] pathForResource:@"avrdude.conf" ofType:@""]]];
    [_flasher runProcess:@"teensy_loader_cli" withArgs:@[@"-v"]];
    [_flasher runProcess:@"dfu-util" withArgs:@[@""]];
    
    [HID setupWithPrinter:_printer];
    [USB setupWithPrinter:_printer];
}



- (void)applicationWillTerminate:(NSNotification *)aNotification {
    // Insert code here to tear down your application
}

@end
