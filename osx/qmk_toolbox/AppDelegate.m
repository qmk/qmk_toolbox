//
//  AppDelegate.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
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
@property IBOutlet NSButton * eepromResetButton;

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
            [_printer print:@"Attempting to flash, please don't remove device" withType:MessageType_Bootloader];
            
            // this is dumb, but the delay is required to let the previous print command show up
            double delayInSeconds = .01;
            dispatch_time_t popTime = dispatch_time(DISPATCH_TIME_NOW, (int64_t)(delayInSeconds * NSEC_PER_SEC));
            dispatch_after(popTime, dispatch_get_main_queue(), ^(void){
                [_flasher performSelector:@selector(flash:withFile:) withObject:[_mcuBox objectValue] withObject:[_filepathBox objectValue]];
            });
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

- (IBAction) eepromResetButtonClick:(id) sender {
    if ([[_mcuBox objectValue] isEqualToString:@""]) {
        [_printer print:@"Please select a microcontroller" withType:MessageType_Error];
    } else {
        [_flasher eepromReset:(NSString *)[_mcuBox objectValue]];
    }
}

- (void)setCaterinaPort:(NSString *)port {
    _flasher.caterinaPort = port;
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
    if ([_autoFlashButton state] == NSOnState) {
        [self flashButtonClick:NULL];
    }
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

- (IBAction)updateFilePath:(id)sender {
    if (![[_filepathBox objectValue] isEqualToString:@""])
        [self setFilePath:[NSURL URLWithString:[_filepathBox objectValue]]];
}

- (void)setFilePath:(NSURL *)path {
    NSString * filename = @"";
    if ([path.scheme isEqualToString:@"file"])
        filename = [[NSString stringWithString:path.absoluteString] stringByReplacingOccurrencesOfString:@"file://" withString:@""];
    if ([path.scheme isEqualToString:@"qmk"]) {
        NSURL * url;
        if ([path.absoluteString containsString:@"qmk://"])
            url = [NSURL URLWithString:[path.absoluteString stringByReplacingOccurrencesOfString:@"qmk://" withString:@""]];
        else
            url = [NSURL URLWithString:[path.absoluteString stringByReplacingOccurrencesOfString:@"qmk:" withString:@""]];
        
        [_printer print:[NSString stringWithFormat:@"Downloading the file: %@", url.absoluteString] withType:MessageType_Info];
        NSData * data = [NSData dataWithContentsOfURL:url];
        if (data) {
            NSArray * paths = NSSearchPathForDirectoriesInDomains(NSDownloadsDirectory, NSUserDomainMask, YES);
            NSString * downloadsDirectory = [paths objectAtIndex:0];
            NSString * name = [url.lastPathComponent stringByReplacingOccurrencesOfString:@"." withString:[NSString stringWithFormat:@"_%@.", [[[NSProcessInfo processInfo] globallyUniqueString] substringToIndex:8]]];
            filename = [NSString stringWithFormat:@"%@/%@", downloadsDirectory, name];
            [data writeToFile:filename atomically:YES];
            [_printer printResponse:[NSString stringWithFormat:@"File saved to: %@", filename] withType:MessageType_Info];
        }
    }
    if (![filename isEqualToString:@""]) {
        if ([_filepathBox indexOfItemWithObjectValue:filename] == NSNotFound)
            [_filepathBox addItemWithObjectValue:filename];
        [_filepathBox selectItemWithObjectValue:filename];
    }
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
    [self setFilePath:[NSURL URLWithString:[[event paramDescriptorForKeyword:keyDirectObject] stringValue]]];
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
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

    [_printer print:@"QMK Toolbox (http://qmk.fm/toolbox)" withType:MessageType_Info];
    [_printer printResponse:@"Supporting following bootloaders:\n" withType:MessageType_Info];
    [_printer printResponse:@" - DFU (Atmel, LUFA) via dfu-programmer (http://dfu-programmer.github.io/)\n" withType:MessageType_Info];
    [_printer printResponse:@" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)\n" withType:MessageType_Info];
    [_printer printResponse:@" - Halfkay (Teensy, Ergodox EZ) via teensy_loader_cli (https://pjrc.com/teensy/loader_cli.html)\n" withType:MessageType_Info];
    [_printer printResponse:@" - STM32 (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n" withType:MessageType_Info];

    
    
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
