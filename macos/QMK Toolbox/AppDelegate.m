#import "AppDelegate.h"

#import "HIDConsoleListener.h"
#import "LogTextView.h"
#import "MicrocontrollerSelector.h"
#import "QMKWindow.h"
#import "USBListener.h"

@interface AppDelegate () <HIDConsoleListenerDelegate, USBListenerDelegate>
@property (weak) IBOutlet QMKWindow *window;
@property IBOutlet LogTextView *logTextView;
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
    [[self.logTextView menu] addItem:[NSMenuItem separatorItem]];
    [[self.logTextView menu] addItem:self.clearMenuItem];

    [self loadRecentDocuments];
    self.showAllDevices = [[NSUserDefaults standardUserDefaults] boolForKey:kShowAllDevices];

    NSString *version = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"];
    [self.logTextView logInfo:[NSString stringWithFormat:@"QMK Toolbox %@ (https://qmk.fm/toolbox)", version]];
    [self.logTextView logInfo:@"Supported bootloaders:"];
    [self.logTextView logInfo:@" - ARM DFU (APM32, Kiibohd, STM32, STM32duino) via dfu-util (http://dfu-util.sourceforge.net/)"];
    [self.logTextView logInfo:@" - Atmel/LUFA/QMK DFU via dfu-programmer (http://dfu-programmer.github.io/)"];
    [self.logTextView logInfo:@" - Atmel SAM-BA (Massdrop) via Massdrop Loader (https://github.com/massdrop/mdloader)"];
    [self.logTextView logInfo:@" - BootloadHID (Atmel, PS2AVRGB) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)"];
    [self.logTextView logInfo:@" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)"];
    [self.logTextView logInfo:@" - HalfKay (Teensy, Ergodox EZ) via Teensy Loader (https://pjrc.com/teensy/loader_cli.html)"];
    [self.logTextView logInfo:@" - WB32 DFU via wb32-dfu-updater_cli (https://github.com/WestberryTech/wb32-dfu-updater)"];
    [self.logTextView logInfo:@" - LUFA Mass Storage"];
    [self.logTextView logInfo:@"Supported ISP flashers:"];
    [self.logTextView logInfo:@" - AVRISP (Arduino ISP)"];
    [self.logTextView logInfo:@" - USBasp (AVR ISP)"];
    [self.logTextView logInfo:@" - USBTiny (AVR Pocket)"];

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
    [self.logTextView logHID:[NSString stringWithFormat:@"HID console connected: %@", device]];
}

- (void)consoleDeviceDidDisconnect:(HIDConsoleDevice *)device {
    self.lastReportedDevice = nil;
    [self updateConsoleList];
    [self.logTextView logHID:[NSString stringWithFormat:@"HID console disconnected: %@", device]];
}

- (void)consoleDevice:(HIDConsoleDevice *)device didReceiveReport:(NSString *)report {
    NSInteger selectedDevice = [self.consoleListBox indexOfSelectedItem];
    if (selectedDevice == 0 || self.consoleListener.devices[selectedDevice - 1] == device) {
        if (self.lastReportedDevice != device) {
            [self.logTextView logHID:[NSString stringWithFormat:@"%@ %@:", device.manufacturerString, device.productString]];
            self.lastReportedDevice = device;
        }
        [self.logTextView logHIDOutput:report];
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
    [self.logTextView logBootloader:[NSString stringWithFormat:@"%@ device connected: %@", device.name, device]];
    [self enableUI];
}

- (void)bootloaderDeviceDidDisconnect:(BootloaderDevice *)device {
    [self.logTextView logBootloader:[NSString stringWithFormat:@"%@ device disconnnected: %@", device.name, device]];
    [self enableUI];
}

-(void)bootloaderDevice:(BootloaderDevice *)device didReceiveCommandOutput:(NSString *)data messageType:(MessageType)type {
    dispatch_sync(dispatch_get_main_queue(), ^{
        [self.logTextView log:data withType:type];
    });
}

- (void)usbDeviceDidConnect:(USBDevice *)device {
    if (self.showAllDevices) {
        [self.logTextView logUSB:[NSString stringWithFormat:@"USB device connected: %@", device]];
    }
}

- (void)usbDeviceDidDisconnect:(USBDevice *)device {
    if (self.showAllDevices) {
        [self.logTextView logUSB:[NSString stringWithFormat:@"USB device disconnected: %@", device]];
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
        [self.logTextView logInfo:@"Auto-flash enabled"];
        [self disableUI];
    } else {
        [self.logTextView logInfo:@"Auto-flash disabled"];
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

                    [self.logTextView logBootloader:@"Attempting to flash, please don't remove device"];
                });

                for (BootloaderDevice *b in [self findBootloaders]) {
                    [b flashWithMCU:mcu file:file];
                }

                dispatch_sync(dispatch_get_main_queue(), ^{
                    [self.logTextView logBootloader:@"Flash complete"];

                    if (!self.autoFlashEnabled) {
                        [self enableUI];
                    }
                });
            });
        } else {
            [self.logTextView logError:@"Please select a microcontroller"];
        }
    } else {
        [self.logTextView logError:@"Please select a file"];
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
        [self.logTextView logError:@"Please select a microcontroller"];
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

                [self.logTextView logBootloader:@"Attempting to clear EEPROM, please don't remove device"];
            });

            for (BootloaderDevice *b in [self findBootloaders]) {
                if ([b eepromFlashable]) {
                    [b flashEEPROMWithMCU:mcu file:@"reset.eep"];
                }
            }

            dispatch_sync(dispatch_get_main_queue(), ^{
                [self.logTextView logBootloader:@"EEPROM clear complete"];

                if (!self.autoFlashEnabled) {
                    [self enableUI];
                }
            });
        });
    } else {
        [self.logTextView logError:@"Please select a microcontroller"];
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

                [self.logTextView logBootloader:@"Attempting to set handedness, please don't remove device"];
            });

            for (BootloaderDevice *b in [self findBootloaders]) {
                if ([b eepromFlashable]) {
                    [b flashEEPROMWithMCU:mcu file:file];
                }
            }

            dispatch_sync(dispatch_get_main_queue(), ^{
                [self.logTextView logBootloader:@"EEPROM write complete"];

                if (!self.autoFlashEnabled) {
                    [self enableUI];
                }
            });
        });
    } else {
        [self.logTextView logError:@"Please select a microcontroller"];
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

        [self.logTextView logInfo:[NSString stringWithFormat:@"Downloading the file: %@", url.absoluteString]];
        NSData *data = [NSData dataWithContentsOfURL:url];
        if (!data) {
            // Try .bin extension if .hex 404'd
            url = [[url URLByDeletingPathExtension] URLByAppendingPathExtension:@"bin"];
            [self.logTextView logInfo:[NSString stringWithFormat:@"No .hex file found, trying %@", url.absoluteString]];
            data = [NSData dataWithContentsOfURL:url];
        }
        if (data) {
            NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDownloadsDirectory, NSUserDomainMask, YES);
            NSString *downloadsDirectory = [paths objectAtIndex:0];
            NSString *name = [url.lastPathComponent stringByReplacingOccurrencesOfString:@"." withString:[NSString stringWithFormat:@"_%@.", [[[NSProcessInfo processInfo] globallyUniqueString] substringToIndex:8]]];
            filename = [NSString stringWithFormat:@"%@/%@", downloadsDirectory, name];
            [data writeToFile:filename atomically:YES];
            [self.logTextView logInfo:[NSString stringWithFormat:@"File saved to: %@", filename]];
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
    [self.logTextView setString:@""];
}
@end
