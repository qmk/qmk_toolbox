//
//  AppDelegate.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "AppDelegate.h"

@interface AppDelegate () <NSTextViewDelegate>

@property (weak) IBOutlet NSWindow *window;
@property IBOutlet NSTextView * textView;

@end

@implementation AppDelegate

typedef enum {
    MessageType_Info,
    MessageType_Error,
    MessageType_HID,
    MessageType_Bootloader,
    MessageType_Command
} MessageType;


- (NSString *)prepend:(NSString *)str withIndent:(NSString *)indent newline:(bool)newline {
    NSString * out;
    if (newline)
        out = [NSString stringWithFormat: @"%@%@%@", indent, str, @"\n"];
    else
        out = [NSString stringWithFormat: @"%@%@", indent, str];
    return out;
}

- (NSDictionary *)formatCommon:(NSColor *)color {
    NSFont *font = [NSFont userFixedPitchFontOfSize:10];
    NSDictionary *attrs = @{
        NSForegroundColorAttributeName : color,
        NSFontAttributeName: font
    };
    return attrs;
    
}

- (NSMutableAttributedString *)format:(NSString *) str forType:(MessageType)type {
    NSColor * color = [NSColor whiteColor];
    switch(type) {
        case MessageType_Info:
            color = [NSColor whiteColor];
            str = [self prepend:str withIndent:@"*** " newline:true];
            break;
        case MessageType_Command:
            color = [NSColor whiteColor];
            str = [self prepend:str withIndent:@">>> " newline:true];
            break;
        case MessageType_Bootloader:
            color = [NSColor yellowColor];
            str = [self prepend:str withIndent:@"*** " newline:true];
            break;
        case MessageType_Error:
            color = [NSColor redColor];
            str = [self prepend:str withIndent:@"  ! " newline:true];
            break;
        case MessageType_HID:
            color = [NSColor blueColor];
            str = [self prepend:str withIndent:@"*** " newline:true];
            break;
    }

    NSMutableAttributedString *attrStr = [[NSMutableAttributedString alloc] initWithString:str attributes:[self formatCommon:color]];
    return attrStr;
}

- (NSMutableAttributedString *)formatResponse:(NSString *) str forType:(MessageType)type {


    str = [str stringByReplacingOccurrencesOfString:@"\n" withString:@"\n    "];
    
    NSColor * color = [NSColor whiteColor];
    switch(type) {
        case MessageType_Info:
            color = [NSColor lightGrayColor];
            str = [self prepend:str withIndent:@"    " newline:true];
            break;
        case MessageType_Command:
            color = [NSColor lightGrayColor];
            str = [self prepend:str withIndent:@"    " newline:true];
            break;
        case MessageType_Bootloader:
            color = [NSColor yellowColor];
            str = [self prepend:str withIndent:@"    " newline:true];
            break;
        case MessageType_Error:
            color = [NSColor redColor];
            str = [self prepend:str withIndent:@"    " newline:true];
            break;
        case MessageType_HID:
            color = [NSColor blueColor];
            str = [self prepend:str withIndent:@"  > " newline:true];
            break;
    }
    
    if ([[_textView.textStorage string] characterAtIndex:[_textView.textStorage length]-1] != '\n')
        str = [NSString stringWithFormat:@"\n%@", str ];
    //if ([str characterAtIndex:[str length] - 1] != '\n')
      //  str = [str substringIndex:4];
    
    NSMutableAttributedString *attrStr = [[NSMutableAttributedString alloc] initWithString:str attributes:[self formatCommon:color]];
    return attrStr;
}

- (void)print:(NSString *)str withType:(MessageType)type {
    [_textView.textStorage appendAttributedString:[self format:str forType:type]];
    [_textView scrollRangeToVisible: NSMakeRange(_textView.string.length, 0)];

}
- (void)printResponse:(NSString *)str withType:(MessageType)type {
    [_textView.textStorage appendAttributedString:[self formatResponse:str forType:type]];
    [_textView scrollRangeToVisible: NSMakeRange(_textView.string.length, 0)];
}

- (void)runProcess:(NSString *)command withArgs:(NSArray<NSString *> *)args {

    [self print:[NSString stringWithFormat:@"%@ %@", command, [args componentsJoinedByString:@" "]] withType:MessageType_Command];
    int pid = [[NSProcessInfo processInfo] processIdentifier];
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
    NSLog (@"grep returned:\n%@", grepOutput);
    [self printResponse:grepOutput withType:MessageType_Command];
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
    // Insert code here to initialize your application
    
    NSSize layoutSize = [_textView maxSize];
    layoutSize.width = layoutSize.height;
    [_textView setMaxSize:layoutSize];
    [[_textView textContainer] setWidthTracksTextView:NO];
    [[_textView textContainer] setContainerSize:layoutSize];

    [self print:@"QMK Toolbox" withType:MessageType_Info];
    [self printResponse:@"Supports the following bootloaders:" withType:MessageType_Info];
    [self printResponse:@" - DFU" withType:MessageType_Info];
    [self printResponse:@" - Halfkay" withType:MessageType_Info];
    [self printResponse:@" - Caterina" withType:MessageType_Info];
    [self printResponse:@" - STM32" withType:MessageType_Info];
    
    [self runProcess:@"dfu-programmer" withArgs:@[@"--help"]];
    [self runProcess:@"avrdude" withArgs:@[@"-C", [[NSBundle mainBundle] pathForResource:@"avrdude.conf" ofType:@""]]];
    [self runProcess:@"teensy_loader_cli" withArgs:@[@"-v"]];
    [self runProcess:@"dfu-util" withArgs:@[@""]];
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
    // Insert code here to tear down your application
}

@end
