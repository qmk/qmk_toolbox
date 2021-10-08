#import "Flashing.h"

#import "USB.h"

@interface Flashing ()
@property Printing *printer;
@end

@implementation Flashing
@synthesize serialPort;
@synthesize mountPoint;

- (id)initWithPrinter:(Printing *)p {
    if (self = [super init]) {
        _printer = p;
    }
    return self;
}

- (NSString *)runProcess:(NSString *)command withArgs:(NSArray<NSString *> *)args {
    [self.printer print:[NSString stringWithFormat:@"%@ %@", command, [args componentsJoinedByString:@" "]] withType:MessageType_Command];
    //int pid = [[NSProcessInfo processInfo] processIdentifier];
    NSPipe *pipe = [NSPipe pipe];
    NSFileHandle *file = pipe.fileHandleForReading;

    NSTask *task = [[NSTask alloc] init];
    task.launchPath = [[NSBundle mainBundle] pathForResource:command ofType:@""];
    task.currentDirectoryPath = [[NSBundle mainBundle] resourcePath];
    task.arguments = args;
    task.standardOutput = pipe;
    task.standardError = pipe;

    [task launch];

    NSData *data = [file readDataToEndOfFile];
    [file closeFile];

    NSString *grepOutput = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    // NSLog (@"grep returned:\n%@", grepOutput);
    [self.printer printResponse:grepOutput withType:MessageType_Command];
    return grepOutput;
}

- (void)flash:(NSString *)mcu withFile:(NSString *)file {
    if ([USB canFlash:AtmelDFU] || [USB canFlash:QMKDFU])
        [self flashAtmelDFU:mcu withFile:file];
    if ([USB canFlash:Caterina])
        [self flashCaterina:mcu withFile:file];
    if ([USB canFlash:Halfkay])
        [self flashHalfkay:mcu withFile:file];
    if ([USB canFlash:STM32DFU])
        [self flashSTM32DFUWithFile:file];
    if ([USB canFlash:APM32DFU])
        [self flashAPM32DFUWithFile:file];
    if ([USB canFlash:Kiibohd])
        [self flashKiibohdWithFile:file];
    if ([USB canFlash:STM32Duino])
        [self flashSTM32DuinoWithFile:file];
    if ([USB canFlash:AVRISP])
        [self flashAVRISP:mcu withFile:file];
    if ([USB canFlash:USBAsp])
        [self flashUSBAsp:mcu withFile:file];
    if ([USB canFlash:USBTiny])
        [self flashUSBTiny:mcu withFile:file];
    if ([USB canFlash:AtmelSAMBA])
        [self flashAtmelSAMBAwithFile:file];
    if ([USB canFlash:BootloadHID])
        [self flashBootloadHIDwithFile:file];
    if ([USB canFlash:LUFAMS])
        [self flashLUFAMSwithFile:file];
}

- (void)reset:(NSString *)mcu {
    if ([USB canFlash:AtmelDFU] || [USB canFlash:QMKDFU])
        [self resetAtmelDFU:mcu];
    if ([USB canFlash:Halfkay])
        [self resetHalfkay:mcu];
    if ([USB canFlash:AtmelSAMBA])
        [self resetAtmelSAMBA];
    if ([USB canFlash:BootloadHID])
        [self resetBootloadHID];
}

- (void)clearEEPROM:(NSString *)mcu {
    if ([USB canFlash:AtmelDFU] || [USB canFlash:QMKDFU])
        [self clearEEPROMAtmelDFU:mcu eraseFirst:![USB canFlash:QMKDFU]];
    if ([USB canFlash:Caterina])
        [self clearEEPROMCaterina:mcu];
    if ([USB canFlash:USBAsp])
        [self clearEEPROMUSBAsp:mcu];
}

- (void)setHandedness:(NSString *)mcu rightHand:(BOOL)rightHand {
    if ([USB canFlash:AtmelDFU] || [USB canFlash:QMKDFU])
        [self setHandednessAtmelDFU:mcu rightHand:rightHand eraseFirst:![USB canFlash:QMKDFU]];
    if ([USB canFlash:Caterina])
        [self setHandednessCaterina:mcu rightHand:rightHand];
    if ([USB canFlash:USBAsp])
        [self setHandednessUSBAsp:mcu rightHand:rightHand];
}

- (BOOL)canFlash {
    return [USB areDevicesAvailable];
}

- (BOOL)canReset {
    NSArray<NSNumber *> *resettable = @[
        @(AtmelDFU),
        @(AtmelSAMBA),
        @(BootloadHID),
        @(Halfkay),
        @(QMKDFU)
    ];
    for (NSNumber *chipset in resettable) {
        if ([USB canFlash:(Chipset)chipset.intValue])
            return YES;
    }
    return NO;
}

- (BOOL)canClearEEPROM {
    NSArray<NSNumber *> *clearable = @[
        @(AtmelDFU),
        @(Caterina),
        @(QMKDFU),
        @(USBAsp)
    ];
    for (NSNumber *chipset in clearable) {
        if ([USB canFlash:(Chipset)chipset.intValue])
            return YES;
    }
    return NO;
}

- (void)flashAPM32DFUWithFile:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"314B:0106", @"-s", @"0x8000000:leave", @"-D", file]];
    } else {
        [self.printer print:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageType_Error];
    }
}

- (void)flashAtmelDFU:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"erase", @"--force"]];
    NSString *result = [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"flash", @"--force", file]];
    if ([result containsString:@"Bootloader and code overlap."]) {
        [self.printer print:@"File is too large for device" withType:MessageType_Error];
    } else {
        [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"reset"]];
    }
}

- (void)resetAtmelDFU:(NSString *)mcu {
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"reset"]];
}

- (void)clearEEPROMAtmelDFU:(NSString *)mcu eraseFirst:(BOOL)erase {
    if (erase) {
        [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"erase", @"--force"]];
    }
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"flash", @"--force", @"--suppress-validation", @"--eeprom", @"reset.eep"]];
    if (erase) {
        [self.printer print:@"Please reflash device with firmware now" withType:MessageType_Bootloader];
    }
}

-(void)setHandednessAtmelDFU:(NSString *)mcu rightHand:(BOOL)rightHand eraseFirst:(BOOL)erase {
    NSString *file = [NSString stringWithFormat: @"reset_%@.eep", (rightHand ? @"right" : @"left")];
    if (erase) {
        [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"erase", @"--force"]];
    }
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"flash", @"--force", @"--suppress-validation", @"--eeprom", file]];
    if (erase) {
        [self.printer print:@"Please reflash device with firmware now" withType:MessageType_Bootloader];
    }
}

- (void)flashAtmelSAMBAwithFile:(NSString *)file {
    [self runProcess:@"mdloader" withArgs:@[@"-p", serialPort, @"-D", file, @"--restart"]];
}

- (void)resetAtmelSAMBA {
    [self runProcess:@"mdloader" withArgs:@[@"-p", serialPort, @"--restart"]];
}

- (void)flashAVRISP:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avrisp", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-P", serialPort, @"-C", @"avrdude.conf"]];
}

- (void)flashBootloadHIDwithFile:(NSString *)file {
    [self runProcess:@"bootloadHID" withArgs:@[@"-r", file]];
}

- (void)resetBootloadHID {
    [self runProcess:@"bootloadHID" withArgs:@[@"-r"]];
}

- (void)flashCaterina:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-P", serialPort, @"-C", @"avrdude.conf"]];
}

- (void)clearEEPROMCaterina:(NSString *)mcu {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", @"eeprom:w:reset.eep:i", @"-P", serialPort, @"-C", @"avrdude.conf"]];
}

- (void)setHandednessCaterina:(NSString *)mcu rightHand:(BOOL)rightHand {
    NSString *file = [NSString stringWithFormat: @"reset_%@.eep", (rightHand ? @"right" : @"left")];
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", [NSString stringWithFormat:@"eeprom:w:%@:i", file], @"-P", serialPort, @"-C", @"avrdude.conf"]];
}

- (void)flashHalfkay:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"teensy_loader_cli" withArgs:@[[@"-mmcu=" stringByAppendingString:mcu], file, @"-v"]];
}

- (void)resetHalfkay:(NSString *)mcu {
    [self runProcess:@"teensy_loader_cli" withArgs:@[[@"-mmcu=" stringByAppendingString:mcu], @"-bv"]];
}

- (void)flashKiibohdWithFile:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-D", file]];
    } else {
        [self.printer print:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageType_Error];
    }
}

- (void)flashLUFAMSwithFile:(NSString *)file {
    if (mountPoint != nil) {
        if ([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
            NSString *destFile = [NSString stringWithFormat:@"%@/FLASH.BIN", mountPoint];
            NSError *error;

            [self.printer print:[NSString stringWithFormat:@"Deleting %@...", destFile] withType:MessageType_Command];
            if (![[NSFileManager defaultManager] removeItemAtPath:destFile error:&error]) {
                [self.printer print:[NSString stringWithFormat:@"IO ERROR: %@", [error localizedDescription]] withType:MessageType_Error];
            }

            [self.printer print:[NSString stringWithFormat:@"Copying %@ to %@...", file, destFile] withType:MessageType_Command];
            if (![[NSFileManager defaultManager] copyItemAtPath:file toPath:destFile error:&error]) {
                [self.printer print:[NSString stringWithFormat:@"IO ERROR: %@", [error localizedDescription]] withType:MessageType_Error];
            }

            [self.printer print:@"Done, please eject drive now." withType:MessageType_Info];
        } else {
            [self.printer print:@"Only firmware files in .bin format can be flashed with this bootloader!" withType:MessageType_Error];
        }
    } else {
        [self.printer print:@"Could not find mount path for device!" withType:MessageType_Error];
    }
}

- (void)flashSTM32DFUWithFile:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"0483:DF11", @"-s", @"0x8000000:leave", @"-D", file]];
    } else {
        [self.printer print:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageType_Error];
    }
}

- (void)flashSTM32DuinoWithFile:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-a", @"2", @"-d", @"1EAF:0003", @"-R", @"-D", file]];
    } else {
        [self.printer print:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageType_Error];
    }
}

- (void)flashUSBAsp:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"usbasp", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-C", @"avrdude.conf"]];
}

- (void)clearEEPROMUSBAsp:(NSString *)mcu {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"usbasp", @"-U", @"eeprom:w:reset.eep:i", @"-C", @"avrdude.conf"]];
}

- (void)setHandednessUSBAsp:(NSString *)mcu rightHand:(BOOL)rightHand {
    NSString *file = [NSString stringWithFormat: @"reset_%@.eep", (rightHand ? @"right" : @"left")];
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"usbasp", @"-U", [NSString stringWithFormat:@"eeprom:w:%@:i", file], @"-C", @"avrdude.conf"]];
}

- (void)flashUSBTiny:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"usbtiny", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-C", @"avrdude.conf"]];
}
@end
