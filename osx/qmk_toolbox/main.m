//
//  main.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import <Cocoa/Cocoa.h>
#import "Printing.h"

Printing * printer;
Flashing * flasher;

int main(int argc, const char * argv[]) {
    printf("Arguments: %d\n", argc);
    for (int i = 0; i < argc; i++) {
        printf(" %d: %s\n", i, argv[i]);
    }
    if (argc > 1) {
        printer = [[Printing alloc] init];
        [printer print:@"QMK Toolbox (http://qmk.fm/toolbox)" withType:MessageType_HID];
    } else {
        return NSApplicationMain(argc, argv);
    }
}
