//
//  Flashing.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import "Flashing.h"
#import "USB.h"

@interface Flashing ()

@property Printing * printer;

@end

@implementation Flashing
@synthesize caterinaPort;

- (id)initWithPrinter:(Printing *)p {
    if (self = [super init]) {
        _printer = p;
    }
    return self;
}

- (NSString *)runProcess:(NSString *)command withArgs:(NSArray<NSString *> *)args {

    [_printer print:[NSString stringWithFormat:@"%@ %@", command, [args componentsJoinedByString:@" "]] withType:MessageType_Command];
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

    NSString *grepOutput = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];
    // NSLog (@"grep returned:\n%@", grepOutput);
    [_printer printResponse:grepOutput withType:MessageType_Command];
    return grepOutput;
}

- (void)flash:(NSString *)mcu withFile:(NSString *)file {
    if ([USB canFlash:DFU])
        [self flashDFU:mcu withFile:file];
    if ([USB canFlash:Caterina])
        [self flashCaterina:mcu withFile:file];
    if ([USB canFlash:Halfkay])
        [self flashHalfkay:mcu withFile:file];
    if ([USB canFlash:STM32])
        [self flashSTM32WithFile:file];
    if ([USB canFlash:Kiibohd])
        [self flashKiibohdWithFile:file];
    if ([USB canFlash:AVRISP])
        [self flashAVRISP:mcu withFile:file];
    if ([USB canFlash:USBAsp])
        [self flashUSBAsp:mcu withFile:file];
    if ([USB canFlash:USBTiny])
        [self flashUSBTiny:mcu withFile:file];
}

- (void)reset:(NSString *)mcu {
    if ([USB canFlash:DFU])
        [self resetDFU:mcu];
    if ([USB canFlash:Halfkay])
        [self resetHalfkay:mcu];
}

- (void)eepromReset:(NSString *)mcu {
    if ([USB canFlash:DFU])
        [self eepromResetDFU:mcu];
    if ([USB canFlash:Caterina])
        [self eepromResetCaterina:mcu];
}

- (void)flashDFU:(NSString *)mcu withFile:(NSString *)file {
    NSString * result;
    result = [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"erase", @"--force"]];
    result = [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"flash", file]];
    if ([result containsString:@"Bootloader and code overlap."]) {
        [_printer print:@"File is too large for device" withType:MessageType_Error];
    } else {
        result = [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"reset"]];
    }
}

- (void)resetDFU:(NSString *)mcu {
    [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"reset"]];
}

- (void)eepromResetDFU:(NSString *)mcu {
    NSString * result;
    NSString * file = [[NSBundle mainBundle] pathForResource:@"reset" ofType:@"eep"];
    result = [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"flash", @"--force", @"--eeprom", file]];
}

- (void)flashCaterina:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-P", caterinaPort, @"-C", @"avrdude.conf"]];
}

- (void)eepromResetCaterina:(NSString *)mcu {
    NSString * result;
    NSString * file = [[NSBundle mainBundle] pathForResource:@"reset" ofType:@"eep"];
    result = [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", [NSString stringWithFormat:@"eeprom:w:%@:i", file], @"-P", caterinaPort, @"-C", @"avrdude.conf"]];
}

- (void)flashHalfkay:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"teensy_loader_cli" withArgs:@[[@"-mmcu=" stringByAppendingString:mcu], file, @"-v"]];
}

- (void)resetHalfkay:(NSString *)mcu {
    [self runProcess:@"teensy_loader_cli" withArgs:@[[@"-mmcu=" stringByAppendingString:mcu], @"-bv"]];
}

- (void)flashSTM32WithFile:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"0482:df11", @"-s", @"0x8000000:leave", @"-D", file]];
    } else {
        [_printer print:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageType_Error];
    }
}

- (void)flashKiibohdWithFile:(NSString *)file {
    if([[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
        [self runProcess:@"dfu-util" withArgs:@[@"-D", file]];
    } else {
        [_printer print:@"Only firmware files in .bin format can be flashed with dfu-util!" withType:MessageType_Error];
    }
}

- (void)flashAVRISP:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avrisp", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-P", caterinaPort, @"-C", @"avrdude.conf"]];
}

- (void)flashUSBTiny:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"usbtiny", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-P", caterinaPort, @"-C", @"avrdude.conf"]];
}

- (void)flashUSBAsp:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"usbasp", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-C", @"avrdude.conf"]];
}

@end
