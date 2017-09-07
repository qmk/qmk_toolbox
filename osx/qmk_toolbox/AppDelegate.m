//
//  AppDelegate.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "AppDelegate.h"
#import "QMKWindow.h"

@interface AppDelegate () <NSTextViewDelegate, NSComboBoxDelegate, FlashingDelegate, USBDelegate>

@property (weak) IBOutlet QMKWindow *window;
@property IBOutlet NSTextView * textView;
@property IBOutlet NSComboBox * filepathBox;
@property IBOutlet NSButton * openButton;
@property IBOutlet NSComboBox * mcuBox;
@property IBOutlet NSButton * flashButton;
@property IBOutlet NSButton * resetButton;
@property IBOutlet NSButton * autoFlashButton;

@property Flashing * flasher;

@end

@implementation AppDelegate

int devicesAvailable[4] = {0, 0, 0, 0};

- (IBAction) openButtonClick:(id) sender {
   NSOpenPanel* panel = [NSOpenPanel openPanel];
   [panel setCanChooseDirectories:NO];
   [panel setAllowsMultipleSelection:NO];
   [panel setMessage:@"Select firmware to load"];
   NSArray* types = @[@"qmk", @"bin", @"hex"];
   [panel setAllowedFileTypes:types];
 
   [panel beginWithCompletionHandler:^(NSInteger result){
      if (result == NSFileHandlingPanelOKButton) {
         [self setFilePath:[[panel URLs] objectAtIndex:0]];
 
         // Open  the document.
      }
 
   }];
}

- (IBAction) flashButtonClick:(id) sender {
    if ([self areDevicesAvailable]) {
        int error = 0;
        if ([[_mcuBox objectValue] isEqualToString:@""]) {
            [_printer print:@"Please select a microcontroller" withType:MessageType_Error];
            error++;
        }
        if ([[_filepathBox objectValue] isEqualToString:@""]) {
            [_printer print:@"Please select a file" withType:MessageType_Error];
            error++;
        }
        if (error == 0) {
            [_printer print:@"Attempting to flash, please don't remove deice" withType:MessageType_Bootloader];
            [_flasher performSelector:@selector(flash:withFile:) withObject:[_mcuBox objectValue] withObject:[_filepathBox objectValue]];
        }
    } else {
        [_printer print:@"There are no devices available" withType:MessageType_Error];
    }
}

- (IBAction) resetButtonClick:(id) sender {
    if ([[_mcuBox objectValue] isEqualToString:@""]) {
        [_printer print:@"Please select a microcontroller" withType:MessageType_Error];
    } else {
        [_flasher reset:(NSString *)[_mcuBox objectValue]];
    }
}

- (BOOL)areDevicesAvailable {
    BOOL available = NO;
    for (int i = 0; i < NumberOfChipsets; i++) {
        available |= devicesAvailable[i];
    }
    return available;
}

- (BOOL)canFlash:(Chipset)chipset {
    return (devicesAvailable[chipset] > 0);
}

- (void)deviceConnected:(Chipset)chipset {
    devicesAvailable[chipset]+=1;
}

- (void)deviceDisconnected:(Chipset)chipset {
    devicesAvailable[chipset]-=1;

}

- (BOOL)application:(NSApplication *)sender openFile:(NSString *)filename {
    if ([[[filename pathExtension] lowercaseString] isEqualToString:@"qmk"] ||
    [[[filename pathExtension] lowercaseString] isEqualToString:@"hex"] ||
    [[[filename pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self setFilePath:[NSURL URLWithString:filename]];
        return true;
    } else {
        return false;
    }
}

- (void)setFilePath:(NSURL *)url {
    NSString * filename = [[NSString stringWithString:url.absoluteString] stringByReplacingOccurrencesOfString:@"file://" withString:@""];
    if ([_filepathBox indexOfItemWithObjectValue:filename] == NSNotFound)
        [_filepathBox addItemWithObjectValue:filename];
    [_filepathBox selectItemWithObjectValue:filename];
}

- (BOOL)readFromURL:(NSURL *)inAbsoluteURL ofType:(NSString *)inTypeName error:(NSError **)outError {
    [self setFilePath:inAbsoluteURL];
    return YES;
}

- (void)applicationWillFinishLaunching:(NSNotification *)notification {
    
    NSAppleEventManager *appleEventManager = [NSAppleEventManager sharedAppleEventManager];
    [appleEventManager setEventHandler:self andSelector:@selector(handleGetURLEvent:withReplyEvent:) forEventClass:kInternetEventClass andEventID:kAEGetURL];
    
}

- (void)handleGetURLEvent:(NSAppleEventDescriptor *)event withReplyEvent:(NSAppleEventDescriptor *)reply
{
    NSLog(@"%@", [[event paramDescriptorForKeyword:keyDirectObject] stringValue]);
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
    // Insert code here to initialize your application

    [_window setup];
    
    _printer = [[Printing alloc] initWithTextView:_textView];
    _flasher = [[Flashing alloc] initWithPrinter:_printer];
    _flasher.delegate = self;
    
    NSString * fileRoot = [[NSBundle mainBundle] pathForResource:@"mcu-list" ofType:@"txt"];
    NSString * fileContents = [NSString stringWithContentsOfFile:fileRoot encoding:NSUTF8StringEncoding error:nil];

    // first, separate by new line
    NSArray * allLinedStrings = [fileContents componentsSeparatedByCharactersInSet:[NSCharacterSet newlineCharacterSet]];

    // choose whatever input identity you have decided. in this case ;
    for (NSString * str in allLinedStrings) {
        [_mcuBox addItemWithObjectValue:str];
    }

    [_printer print:@"QMK Toolbox" withType:MessageType_Info];
    [_printer printResponse:@"Supports the following bootloaders:" withType:MessageType_Info];
    [_printer printResponse:@" - DFU" withType:MessageType_Info];
    [_printer printResponse:@" - Halfkay" withType:MessageType_Info];
    [_printer printResponse:@" - Caterina" withType:MessageType_Info];
    [_printer printResponse:@" - STM32" withType:MessageType_Info];
    
//    [_flasher runProcess:@"dfu-programmer" withArgs:@[@"--help"]];
//    [_flasher runProcess:@"avrdude" withArgs:@[@"-C", [[NSBundle mainBundle] pathForResource:@"avrdude.conf" ofType:@""]]];
//    [_flasher runProcess:@"teensy_loader_cli" withArgs:@[@"-v"]];
//    [_flasher runProcess:@"dfu-util" withArgs:@[@""]];
    
    [HID setupWithPrinter:_printer];
    [USB setupWithPrinter:_printer andDelegate:self];
}

- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication *)theApplication {
    return YES;
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
    // Insert code here to tear down your application
}

@end
