//
//  AppDelegate.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "AppDelegate.h"
#import "QMKWindow.h"

@interface AppDelegate () <NSTextViewDelegate, NSComboBoxDelegate>

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

- (IBAction) openButtonClick:(id) sender {
   NSOpenPanel* panel = [NSOpenPanel openPanel];
   [panel setCanChooseDirectories:NO];
   [panel setAllowsMultipleSelection:NO];
   [panel setMessage:@"Select firmware to load"];
   NSArray* types = @[@"qmk", @"bin", @"hex"];
   [panel setAllowedFileTypes:types];
 
   // This method displays the panel and returns immediately.
   // The completion handler is called when the user selects an
   // item or cancels the panel.
   [panel beginWithCompletionHandler:^(NSInteger result){
      if (result == NSFileHandlingPanelOKButton) {
         [self setFilePath:[[panel URLs] objectAtIndex:0]];
 
         // Open  the document.
      }
 
   }];
}

- (IBAction) flashButtonClick:(id) sender {

}

- (IBAction) resetButtonClick:(id) sender {

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

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
    // Insert code here to initialize your application
    
    
    //[_window registerForDraggedTypes:[NSArray arrayWithObject:NSFilenamesPboardType]];
    [_window setup];
    
    _printer = [[Printing alloc] initWithTextView:_textView];
    _flasher = [[Flashing alloc] initWithPrinter:_printer];
    
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
