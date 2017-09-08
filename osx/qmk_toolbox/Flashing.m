//
//  Flashing.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "Flashing.h"
#import "USB.h"

@interface Flashing ()

@property Printing * printer;

@end

@implementation Flashing
@synthesize delegate;
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
    if ([delegate canFlash:DFU])
        [self flashDFU:mcu withFile:file];
    if ([delegate canFlash:Caterina])
        [self flashCaterina:mcu withFile:file];
    if ([delegate canFlash:Halfkay])
        [self flashHalfkay:mcu withFile:file];
    if ([delegate canFlash:STM32])
        [self flashSTM32WithFile:file];
}

- (void)reset:(NSString *)mcu {
    if ([delegate canFlash:DFU])
        [self resetDFU:mcu];
    if ([delegate canFlash:Halfkay])
        [self resetHalfkay:mcu];
}

- (void)eepromReset:(NSString *)mcu {
    if ([delegate canFlash:DFU])
        [self eepromResetDFU:mcu];
    if ([delegate canFlash:Caterina])
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
    NSString * file = [[NSBundle mainBundle] pathForResource:[mcu stringByAppendingString:@"_eeprom_reset"] ofType:@"hex"];
    result = [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"erase", @"--force"]];
    result = [self runProcess:@"dfu-programmer" withArgs:@[mcu, @"flash", @"--eeprom", file]];
    [_printer print:@"Device has been erased - please reflash" withType:MessageType_Bootloader];
}

- (void)flashCaterina:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", [NSString stringWithFormat:@"flash:w:%@:i", file], @"-P", caterinaPort, @"-C", @"avrdude.conf"]];
}

- (void)eepromResetCaterina:(NSString *)mcu {
    NSString * result;
    NSString * file = [mcu stringByAppendingString:@"_eeprom_reset.hex"];
    result = [self runProcess:@"avrdude" withArgs:@[@"-p", mcu, @"-c", @"avr109", @"-U", [NSString stringWithFormat:@"eeprom:w:%@:i", file], @"-P", caterinaPort, @"-C", @"avrdude.conf"]];
}

- (void)flashHalfkay:(NSString *)mcu withFile:(NSString *)file {
    [self runProcess:@"teensy_loader_cli" withArgs:@[[@"-mmcu=" stringByAppendingString:mcu], file, @"-v"]];
}

- (void)resetHalfkay:(NSString *)mcu {
    [self runProcess:@"teensy_loader_cli" withArgs:@[[@"-mmcu=" stringByAppendingString:mcu], @"-bv"]];
}

- (void)flashSTM32WithFile:(NSString *)file {
    [self runProcess:@"dfu-util" withArgs:@[@"-a", @"0", @"-d", @"0482:df11", @"-s", @"0x8000000", @"-D", file]];
}

@end