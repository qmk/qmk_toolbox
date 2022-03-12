#import "AppDelegate.h"

#import "HIDConsoleListener.h"
#import "MicrocontrollerSelector.h"
#import "QMKWindow.h"
#import "USBListener.h"

@interface AppDelegate () <HIDConsoleListenerDelegate, USBListenerDelegate>
@property (weak) IBOutlet QMKWindow *window;
@property IBOutlet NSTextView *textView;
@property IBOutlet NSMenuItem *clearMenuItem;
@property IBOutlet NSComboBox *filepathBox;
@property IBOutlet NSButton *openButton;
@property IBOutlet MicrocontrollerSelector *mcuBox;
@property IBOutlet NSButton *flashButton;
@property IBOutlet NSButton *resetButton;
@property IBOutlet NSButton *clearEEPROMButton;
@property IBOutlet NSComboBox *consoleListBox;

@property NSWindowController *keyTesterWindowController;

@property HIDConsoleListener *consoleListener;

@property HIDConsoleDevice *lastReportedDevice;

@property USBListener *usbListener;
@end

@implementation AppDelegate
#pragma mark App Delegate
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

- (void)applicationWillFinishLaunching:(NSNotification *)notification {
    NSAppleEventManager *appleEventManager = [NSAppleEventManager sharedAppleEventManager];
    [appleEventManager setEventHandler:self andSelector:@selector(handleGetURLEvent:withReplyEvent:) forEventClass:kInternetEventClass andEventID:kAEGetURL];
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
    [self.window setup];

    self.printer = [[Printing alloc] initWithTextView:self.textView];

    [[self.textView menu] addItem:[NSMenuItem separatorItem]];
    [[self.textView menu] addItem:self.clearMenuItem];

    [self loadRecentDocuments];
    self.showAllDevices = [[NSUserDefaults standardUserDefaults] boolForKey:kShowAllDevices];

    NSString *version = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"];
    [self.printer print:[NSString stringWithFormat:@"QMK Toolbox %@ (http://qmk.fm/toolbox)", version] withType:MessageType_Info];
    [self.printer printResponse:@"Supported bootloaders:\n" withType:MessageType_Info];
    [self.printer printResponse:@" - ARM DFU (APM32, Kiibohd, STM32, STM32duino) via dfu-util (http://dfu-util.sourceforge.net/)\n" withType:MessageType_Info];
    [self.printer printResponse:@" - Atmel/LUFA/QMK DFU via dfu-programmer (http://dfu-programmer.github.io/)\n" withType:MessageType_Info];
    [self.printer printResponse:@" - Atmel SAM-BA (Massdrop) via Massdrop Loader (https://github.com/massdrop/mdloader)\n" withType:MessageType_Info];
    [self.printer printResponse:@" - BootloadHID (Atmel, PS2AVRGB) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)\n" withType:MessageType_Info];
    [self.printer printResponse:@" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)\n" withType:MessageType_Info];
    [self.printer printResponse:@" - HalfKay (Teensy, Ergodox EZ) via Teensy Loader (https://pjrc.com/teensy/loader_cli.html)\n" withType:MessageType_Info];
    [self.printer printResponse:@" - LUFA Mass Storage\n" withType:MessageType_Info];
    [self.printer printResponse:@"Supported ISP flashers:\n" withType:MessageType_Info];
    [self.printer printResponse:@" - AVRISP (Arduino ISP)\n" withType:MessageType_Info];
    [self.printer printResponse:@" - USBasp (AVR ISP)\n" withType:MessageType_Info];
    [self.printer printResponse:@" - USBTiny (AVR Pocket)\n" withType:MessageType_Info];

    self.usbListener = [[USBListener alloc] init];
    self.usbListener.delegate = self;
    [self.usbListener start];

    self.consoleListener = [[HIDConsoleListener alloc] init];
    self.consoleListener.delegate = self;
    [self.consoleListener start];
}

- (void)handleGetURLEvent:(NSAppleEventDescriptor *)event withReplyEvent:(NSAppleEventDescriptor *)reply {
    [self setFilePath:[NSURL URLWithString:[[event paramDescriptorForKeyword:keyDirectObject] stringValue]]];
}

- (void)loadRecentDocuments {
    NSArray<NSURL *> *recentDocuments = [[NSDocumentController sharedDocumentController] recentDocumentURLs];
    for (NSURL *document in recentDocuments) {
        [self.filepathBox addItemWithObjectValue:document.path];
    }
    if (self.filepathBox.numberOfItems > 0) {
        [self.filepathBox selectItemAtIndex:0];
    }
}

- (BOOL)applicationShouldTerminateAfterLastWindowClosed:(NSApplication *)theApplication {
    return YES;
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
    [self.usbListener stop];
    [self.consoleListener stop];
}

#pragma mark HID Console
- (void)consoleDeviceDidConnect:(HIDConsoleDevice *)device {
    self.lastReportedDevice = device;
    [self updateConsoleList];
    NSString *deviceConnectedString = [NSString stringWithFormat:@"HID console connected: %@", device];
    [self.printer print:deviceConnectedString withType:MessageType_HID];
}

- (void)consoleDeviceDidDisconnect:(HIDConsoleDevice *)device {
    self.lastReportedDevice = nil;
    [self updateConsoleList];
    NSString *deviceDisconnectedString = [NSString stringWithFormat:@"HID console disconnected: %@", device];
    [self.printer print:deviceDisconnectedString withType:MessageType_HID];
}

- (void)consoleDevice:(HIDConsoleDevice *)device didReceiveReport:(NSString *)report {
    NSInteger selectedDevice = [self.consoleListBox indexOfSelectedItem];
    if (selectedDevice == 0 || self.consoleListener.devices[selectedDevice - 1] == device) {
        if (self.lastReportedDevice != device) {
             [self.printer print:[NSString stringWithFormat:@"%@ %@:", device.manufacturerString, device.productString] withType:MessageType_HID];
            self.lastReportedDevice = device;
        }
        [self.printer printResponse:report withType:MessageType_HID];
    }
}

- (void)updateConsoleList {
    NSInteger selected = [self.consoleListBox indexOfSelectedItem] != -1 ? [self.consoleListBox indexOfSelectedItem] : 0;
    [self.consoleListBox deselectItemAtIndex:selected];
    [self.consoleListBox removeAllItems];

    for (HIDConsoleDevice *device in self.consoleListener.devices) {
        [self.consoleListBox addItemWithObjectValue:[device description]];
    }

    if ([self.consoleListBox numberOfItems] > 0) {
        [self.consoleListBox insertItemWithObjectValue:@"(All connected devices)" atIndex:0];
        [self.consoleListBox selectItemAtIndex:([self.consoleListBox numberOfItems] > selected) ? selected : 0];
    }
}

#pragma mark USB Devices & Bootloaders
- (void)bootloaderDeviceDidConnect:(BootloaderDevice *)device {
    [self.printer print:[NSString stringWithFormat:@"%@ device connected: %@", device.name, device] withType:MessageType_Bootloader];
    [self enableUI];
}

- (void)bootloaderDeviceDidDisconnect:(BootloaderDevice *)device {
    [self.printer print:[NSString stringWithFormat:@"%@ device disconnnected: %@", device.name, device] withType:MessageType_Bootloader];
    [self enableUI];
}

-(void)bootloaderDevice:(BootloaderDevice *)device didReceiveCommandOutput:(NSString *)data messageType:(MessageType)type {
    dispatch_sync(dispatch_get_main_queue(), ^{
        [self.printer printResponse:data withType:type];
    });
}

- (void)usbDeviceDidConnect:(USBDevice *)device {
    if (self.showAllDevices) {
        [self.printer print:[NSString stringWithFormat:@"USB device connected: %@", device] withType:MessageType_Info];
    }
}

- (void)usbDeviceDidDisconnect:(USBDevice *)device {
    if (self.showAllDevices) {
        [self.printer print:[NSString stringWithFormat:@"USB device disconnected: %@", device] withType:MessageType_Info];
    }
}

#pragma mark UI Interaction
@synthesize autoFlashEnabled = _autoFlashEnabled;

- (BOOL)autoFlashEnabled {
    return _autoFlashEnabled;
}

- (void)setAutoFlashEnabled:(BOOL)autoFlashEnabled {
    _autoFlashEnabled = autoFlashEnabled;
    if (autoFlashEnabled) {
        [self.printer print:@"Auto-flash enabled" withType:MessageType_Info];
        [self disableUI];
    } else {
        [self.printer print:@"Auto-flash disabled" withType:MessageType_Info];
        [self enableUI];
    }
}

@synthesize showAllDevices = _showAllDevices;

- (BOOL)showAllDevices {
    return _showAllDevices;
}

- (void)setShowAllDevices:(BOOL)showAllDevices {
    _showAllDevices = showAllDevices;
    [[NSUserDefaults standardUserDefaults] setBool:showAllDevices forKey:kShowAllDevices];
}

- (IBAction)flashButtonClick:(id)sender {
    NSString *file = [self.filepathBox stringValue];

    if ([file length] > 0) {
        if ([self.mcuBox indexOfSelectedItem] >= 0) {
            NSString *mcu = [self.mcuBox keyForSelectedItem];

            dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
                dispatch_sync(dispatch_get_main_queue(), ^{
                    if (!self.autoFlashEnabled) {
                        [self disableUI];
                    }

                    [self.printer print:@"Attempting to flash, please don't remove device" withType:MessageType_Bootloader];
                });

                for (BootloaderDevice *b in [self findBootloaders]) {
                    [b flashWithMCU:mcu file:file];
                }

                dispatch_sync(dispatch_get_main_queue(), ^{
                    [self.printer print:@"Flash complete" withType:MessageType_Bootloader];

                    if (!self.autoFlashEnabled) {
                        [self enableUI];
                    }
                });
            });
        } else {
            [self.printer print:@"Please select a microcontroller" withType:MessageType_Error];
        }
    } else {
        [self.printer print:@"Please select a file" withType:MessageType_Error];
    }
}

- (IBAction)resetButtonClick:(id)sender {
    if ([self.mcuBox indexOfSelectedItem] >= 0) {
        NSString *mcu = [self.mcuBox keyForSelectedItem];

        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
            dispatch_sync(dispatch_get_main_queue(), ^{
                if (!self.autoFlashEnabled) {
                    [self disableUI];
                }
            });

            for (BootloaderDevice *b in [self findBootloaders]) {
                if ([b resettable]) {
                    [b resetWithMCU:mcu];
                }
            }

            dispatch_sync(dispatch_get_main_queue(), ^{
                if (!self.autoFlashEnabled) {
                    [self enableUI];
                }
            });
        });
    } else {
        [self.printer print:@"Please select a microcontroller" withType:MessageType_Error];
    }
}

- (IBAction)clearEEPROMButtonClick:(id)sender {
    if ([self.mcuBox indexOfSelectedItem] >= 0) {
        NSString *mcu = [self.mcuBox keyForSelectedItem];

        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
            dispatch_sync(dispatch_get_main_queue(), ^{
                if (!self.autoFlashEnabled) {
                    [self disableUI];
                }

                [self.printer print:@"Attempting to clear EEPROM, please don't remove device" withType:MessageType_Bootloader];
            });

            for (BootloaderDevice *b in [self findBootloaders]) {
                if ([b eepromFlashable]) {
                    [b flashEEPROMWithMCU:mcu file:@"reset.eep"];
                }
            }

            dispatch_sync(dispatch_get_main_queue(), ^{
                [self.printer print:@"EEPROM clear complete" withType:MessageType_Bootloader];

                if (!self.autoFlashEnabled) {
                    [self enableUI];
                }
            });
        });
    } else {
        [self.printer print:@"Please select a microcontroller" withType:MessageType_Error];
    }
}

- (IBAction)setHandednessButtonClick:(id)sender {
    if ([self.mcuBox indexOfSelectedItem] >= 0) {
        NSString *mcu = [self.mcuBox keyForSelectedItem];
        NSString *file = [sender tag] == 0 ? @"left.eep" : @"right.eep";

        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
            dispatch_sync(dispatch_get_main_queue(), ^{
                if (!self.autoFlashEnabled) {
                    [self disableUI];
                }

                [self.printer print:@"Attempting to set handedness, please don't remove device" withType:MessageType_Bootloader];
            });

            for (BootloaderDevice *b in [self findBootloaders]) {
                if ([b eepromFlashable]) {
                    [b flashEEPROMWithMCU:mcu file:file];
                }
            }

            dispatch_sync(dispatch_get_main_queue(), ^{
                [self.printer print:@"EEPROM write complete" withType:MessageType_Bootloader];

                if (!self.autoFlashEnabled) {
                    [self enableUI];
                }
            });
        });
    } else {
        [self.printer print:@"Please select a microcontroller" withType:MessageType_Error];
    }
}

-(NSMutableArray<BootloaderDevice *> *)findBootloaders {
    NSMutableArray<BootloaderDevice *> *bootloaders = [[NSMutableArray alloc] init];

    for (USBDevice *d in self.usbListener.devices) {
        if ([d isKindOfClass:[BootloaderDevice class]]) {
            [bootloaders addObject:(BootloaderDevice *)d];
        }
    }

    return bootloaders;
}

- (IBAction)openButtonClick:(id)sender {
    NSOpenPanel *panel = [NSOpenPanel openPanel];
    [panel setCanChooseDirectories:NO];
    [panel setAllowsMultipleSelection:NO];
    [panel setMessage:@"Select firmware to load"];
    NSArray *types = @[@"qmk", @"bin", @"hex"];
    [panel setAllowedFileTypes:types];

    [panel beginWithCompletionHandler:^(NSInteger result){
        if (result == NSModalResponseOK) {
            [self setFilePath:[[panel URLs] objectAtIndex:0]];
        }
    }];
}

- (IBAction)updateFilePath:(id)sender {
    if (![[self.filepathBox objectValue] isEqualToString:@""])
        [self setFilePath:[NSURL URLWithString:[self.filepathBox objectValue]]];
}

- (void)setFilePath:(NSURL *)path {
    NSString *filename = @"";
    if ([path.scheme isEqualToString:@"file"]) {
        filename = [[path.absoluteString stringByRemovingPercentEncoding] stringByReplacingOccurrencesOfString:@"file://" withString:@""];
    }
    if ([path.scheme isEqualToString:@"qmk"]) {
        NSURL *url;
        if ([path.absoluteString containsString:@"qmk://"]) {
            url = [NSURL URLWithString:[path.absoluteString stringByReplacingOccurrencesOfString:@"qmk://" withString:@""]];
        } else {
            url = [NSURL URLWithString:[path.absoluteString stringByReplacingOccurrencesOfString:@"qmk:" withString:@""]];
        }

        [self.printer print:[NSString stringWithFormat:@"Downloading the file: %@", url.absoluteString] withType:MessageType_Info];
        NSData *data = [NSData dataWithContentsOfURL:url];
        if (!data) {
            // Try .bin extension if .hex 404'd
            url = [[url URLByDeletingPathExtension] URLByAppendingPathExtension:@"bin"];
            [self.printer print:[NSString stringWithFormat:@"No .hex file found, trying %@", url.absoluteString] withType:MessageType_Info];
            data = [NSData dataWithContentsOfURL:url];
        }
        if (data) {
            NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDownloadsDirectory, NSUserDomainMask, YES);
            NSString *downloadsDirectory = [paths objectAtIndex:0];
            NSString *name = [url.lastPathComponent stringByReplacingOccurrencesOfString:@"." withString:[NSString stringWithFormat:@"_%@.", [[[NSProcessInfo processInfo] globallyUniqueString] substringToIndex:8]]];
            filename = [NSString stringWithFormat:@"%@/%@", downloadsDirectory, name];
            [data writeToFile:filename atomically:YES];
            [self.printer printResponse:[NSString stringWithFormat:@"File saved to: %@", filename] withType:MessageType_Info];
        }
    }
    if (![filename isEqualToString:@""]) {
        if ([self.filepathBox indexOfItemWithObjectValue:filename] == NSNotFound) {
            [self.filepathBox addItemWithObjectValue:filename];
        }
        [self.filepathBox selectItemWithObjectValue:filename];
        [[NSDocumentController sharedDocumentController] noteNewRecentDocumentURL:[[NSURL alloc] initFileURLWithPath:filename]];
    }
}

- (void)enableUI {
    NSArray<BootloaderDevice *> *bootloaders = [self findBootloaders];
    self.canFlash = [bootloaders count] > 0;
    self.canReset = NO;
    self.canClearEEPROM = NO;
    for (BootloaderDevice *b in bootloaders) {
        if (b.resettable) {
            self.canReset = YES;
            break;
        }
    }
    for (BootloaderDevice *b in bootloaders) {
        if (b.eepromFlashable) {
            self.canClearEEPROM = YES;
            break;
        }
    }
}

- (void)disableUI {
    self.canFlash = NO;
    self.canReset = NO;
    self.canClearEEPROM = NO;
}

- (IBAction)keyTesterButtonClick:(id)sender {
    if (!self.keyTesterWindowController) {
        self.keyTesterWindowController = [[NSWindowController alloc] initWithWindowNibName:@"KeyTesterWindow"];
    }
    [self.keyTesterWindowController showWindow:self];
}

#pragma mark Uncategorized
- (IBAction)clearButtonClick:(id)sender {
    [[self textView] setString:@""];
}
@end
