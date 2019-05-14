//
//  main.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import <Cocoa/Cocoa.h>
#import "Printing.h"
#import "Flashing.h"
#import "USB.h"
#import "HID.h"

Printing * _printer;
Flashing * _flasher;

int main(int argc, const char * argv[]) {
//    printf("Arguments: %d\n", argc - 1);
//    for (int i = 1; i < argc; i++) {
//        printf(" %d: %s\n", i, argv[i]);
//    }
    if (argc > 1 && strcmp(argv[1], "-NSDocumentRevisionsDebugMode")) {
        _printer = [[Printing alloc] init];
        
        if (!strcmp(argv[1], "list")) {
            [USB setupWithPrinter:_printer];
            return 0;
        }
        
        if (!strcmp(argv[1], "flash") && argc == 4) {
            _flasher = [[Flashing alloc] initWithPrinter:_printer];
            [HID setupWithPrinter:_printer];
            [USB setupWithPrinter:_printer];
            if ([USB areDevicesAvailable]) {
                [_printer print:@"Attempting to flash, please don't remove device" withType:MessageType_Bootloader];
                [_flasher flash:[NSString stringWithUTF8String:argv[2]] withFile:[NSString stringWithUTF8String:argv[3]]];
                return 0;
            } else {
                [_printer print:@"There are no devices available" withType:MessageType_Error];
                return 1;
            }
        }
        
        if (!strcmp(argv[1], "help")) {
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
            
            [_printer print:@"usage: QMK\\ Toolbox [command]" withType:MessageType_Info];
            [_printer printResponse:@"commands available:\n" withType:MessageType_Info];
            [_printer printResponse:@" - help                 print this message\n" withType:MessageType_Info];
            [_printer printResponse:@" - list                 list the connected devices\n" withType:MessageType_Info];
            [_printer printResponse:@" - flash [mcu] [file]   if available, flash the [mcu] with [file]\n" withType:MessageType_Info];
            return 0;
        }
        
        [_printer print:@"Command not found - use \"help\" for all commands" withType:MessageType_Error];
        return 1;
        
    } else {
        return NSApplicationMain(argc, argv);
    }
}
