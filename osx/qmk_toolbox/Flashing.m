//
//  Flashing.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "Flashing.h"

@interface Flashing ()

@property Printing * printer;

@end

@implementation Flashing

- (id)initWithPrinter:(Printing *)p {
    if (self = [super init]) {
        _printer = p;
    }
    return self;
}

- (void)runProcess:(NSString *)command withArgs:(NSArray<NSString *> *)args {

    [_printer print:[NSString stringWithFormat:@"%@ %@", command, [args componentsJoinedByString:@" "]] withType:MessageType_Command];
    //int pid = [[NSProcessInfo processInfo] processIdentifier];
    NSPipe *pipe = [NSPipe pipe];
    NSFileHandle *file = pipe.fileHandleForReading;

    NSTask *task = [[NSTask alloc] init];
    task.launchPath = [[NSBundle mainBundle] pathForResource:command ofType:@""];
    task.arguments = args;
    task.standardOutput = pipe;
    task.standardError = pipe;

    [task launch];

    NSData *data = [file readDataToEndOfFile];
    [file closeFile];

    NSString *grepOutput = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];
    // NSLog (@"grep returned:\n%@", grepOutput);
    [_printer printResponse:grepOutput withType:MessageType_Command];
}

@end