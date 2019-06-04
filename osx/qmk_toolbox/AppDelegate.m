//
//  AppDelegate.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import "AppDelegate.h"
#import "Constants.h"
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
@property IBOutlet NSComboBox * keyboardBox;
@property IBOutlet NSComboBox * keymapBox;
@property IBOutlet NSButton * loadButton;

@property Flashing * flasher;

@end

@implementation AppDelegate

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
    if ([USB areDevicesAvailable]) {
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

- (void)deviceConnected:(Chipset)chipset {
    if ([_autoFlashButton state] == NSOnState) {
        [self flashButtonClick:NULL];
    }
}

- (void)deviceDisconnected:(Chipset)chipset {

}

- (BOOL)application:(NSApplication *)sender openFile:(NSString *)filename {
    if ([[[filename pathExtension] lowercaseString] isEqualToString:@"qmk"] ||
    [[[filename pathExtension] lowercaseString] isEqualToString:@"hex"] ||
    [[[filename pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self setFilePath:[NSURL fileURLWithPath:filename]];
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
        filename = [[path.absoluteString stringByRemovingPercentEncoding] stringByReplacingOccurrencesOfString:@"file://" withString:@""];
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
        [[NSDocumentController sharedDocumentController] noteNewRecentDocumentURL:[[NSURL alloc] initFileURLWithPath:filename]];
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
    
    [self loadMicrocontrollers];
    [self loadKeyboards];
    [self loadKeymaps];
    [self loadRecentDocuments];

    [_printer print:@"QMK Toolbox (http://qmk.fm/toolbox)" withType:MessageType_Info];
    [_printer printResponse:@"Supporting following bootloaders:\n" withType:MessageType_Info];
    [_printer printResponse:@" - DFU (Atmel, LUFA) via dfu-programmer (http://dfu-programmer.github.io/)\n" withType:MessageType_Info];
    [_printer printResponse:@" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)\n" withType:MessageType_Info];
    [_printer printResponse:@" - Halfkay (Teensy, Ergodox EZ) via teensy_loader_cli (https://pjrc.com/teensy/loader_cli.html)\n" withType:MessageType_Info];
    [_printer printResponse:@" - STM32 (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n" withType:MessageType_Info];
    [_printer printResponse:@" - Kiibohd (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n" withType:MessageType_Info];
    [_printer printResponse:@"And the following ISP flasher protocols:\n" withType:MessageType_Info];
    [_printer printResponse:@" - USBTiny (AVR Pocket)\n" withType:MessageType_Info];
    [_printer printResponse:@" - AVRISP (Arduino ISP)\n" withType:MessageType_Info];
    [_printer printResponse:@" - USBASP (AVR ISP)\n" withType:MessageType_Info];
    
    
//    [_flasher runProcess:@"dfu-programmer" withArgs:@[@"--help"]];
//    [_flasher runProcess:@"avrdude" withArgs:@[@"-C", [[NSBundle mainBundle] pathForResource:@"avrdude.conf" ofType:@""]]];
//    [_flasher runProcess:@"teensy_loader_cli" withArgs:@[@"-v"]];
//    [_flasher runProcess:@"dfu-util" withArgs:@[@""]];
    
    [HID setupWithPrinter:_printer];
    [USB setupWithPrinter:_printer andDelegate:self];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(mcuSelectionChanged) name:NSComboBoxSelectionDidChangeNotification object:_mcuBox];
}

- (void)mcuSelectionChanged {
    [[NSUserDefaults standardUserDefaults] setValue:self.mcuBox.objectValueOfSelectedItem forKey:QMKMicrocontrollerKey];
}

- (void)loadMicrocontrollers {
    NSString * fileRoot = [[NSBundle mainBundle] pathForResource:@"mcu-list" ofType:@"txt"];
    NSString * fileContents = [NSString stringWithContentsOfFile:fileRoot encoding:NSUTF8StringEncoding error:nil];
    
    // first, separate by new line
    NSArray * allLinedStrings = [fileContents componentsSeparatedByCharactersInSet:[NSCharacterSet newlineCharacterSet]];
    
    // choose whatever input identity you have decided. in this case ;
    for (NSString * str in allLinedStrings) {
        [_mcuBox addItemWithObjectValue:str];
    }
    [_mcuBox selectItemAtIndex:0];
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    NSString *lastUsedMCU = [defaults stringForKey:QMKMicrocontrollerKey];
    if (lastUsedMCU) {
        [self.mcuBox selectItemWithObjectValue:lastUsedMCU];
    }
}

- (void)loadKeyboards {
    NSData * data = [NSData dataWithContentsOfURL:[NSURL URLWithString:@"http://compile.qmk.fm/v1/keyboards"]];
    NSError * error = nil;
    NSArray * keyboards = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
    [_keyboardBox removeAllItems];
    [_keyboardBox addItemsWithObjectValues:keyboards];
    [_keyboardBox selectItemAtIndex:0];
    _keyboardBox.enabled = YES;
}


- (void)loadKeymaps {
//    NSData * data = [NSData dataWithContentsOfURL:[NSURL URLWithString:@"http://compile.qmk.fm/v1/keyboards"]];
//    NSError * error = nil;
//    NSArray * keyboards = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
    [_keymapBox removeAllItems];
    [_keymapBox addItemWithObjectValue:@"default"];
    [_keymapBox selectItemAtIndex:0];
//    for (NSString * keyboard in keyboards) {
//        [_keyboardBox addItemsWithObjectValues:keyboards];
//    }
//    _keymapBox.enabled = YES;
    _loadButton.enabled = YES;
}

- (void)loadRecentDocuments {
    NSArray<NSURL *> *recentDocuments = [[NSDocumentController sharedDocumentController] recentDocumentURLs];
    for (NSURL *document in recentDocuments) {
        [self.filepathBox addItemWithObjectValue:document.path];
    }
    if (self.filepathBox.numberOfItems > 0)
        [self.filepathBox selectItemAtIndex:0];
}

- (IBAction)loadKeymapClick:(id)sender {
    NSString * keyboard = [[_keyboardBox objectValue] stringByReplacingOccurrencesOfString:@"/" withString:@"_"];
    [self setFilePath:[NSURL URLWithString:[NSString stringWithFormat:@"qmk:http://qmk.fm/compiled/%@_default.hex", keyboard]]];
}

- (IBAction)clearButtonClick:(id)sender {
    [[self textView] setString: @""];
}

- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication *)theApplication {
    return YES;
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
    // Insert code here to tear down your application
}

@end
