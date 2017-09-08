//
//  Printing.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "Printing.h"

@implementation Printing

NSTextView * textView;
MessageType lastMessage;

- (id)initWithTextView:(NSTextView *)tV {
    if (self = [super init]) {
        textView = tV;
        
        NSSize layoutSize = [textView maxSize];
        layoutSize.width = layoutSize.height;
        [textView setMaxSize:layoutSize];
        [[textView textContainer] setWidthTracksTextView:NO];
        [[textView textContainer] setContainerSize:layoutSize];
        
        [textView setSelectedTextAttributes:@{
            NSBackgroundColorAttributeName : [NSColor colorWithHue:0 saturation:0 brightness:.3 alpha:1]
        }];
    }
    return self;
}

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
            color = [NSColor colorWithHue:200.0/360 saturation:.9 brightness:1. alpha:1.];
            str = [self prepend:str withIndent:@"*** " newline:true];
            break;
    }

    lastMessage = type;
    NSMutableAttributedString *attrStr = [[NSMutableAttributedString alloc] initWithString:str attributes:[self formatCommon:color]];
    return attrStr;
}

- (NSMutableAttributedString *)formatResponse:(NSString *) str forType:(MessageType)type {
    bool addBackNewLine = false;
    if ([str characterAtIndex:[str length] - 1] == '\n') {
        str = [str substringToIndex:(str.length - 1	)];
        addBackNewLine = true;
    }
    str = [str stringByReplacingOccurrencesOfString:@"\n" withString:@"\n    "];
    if (addBackNewLine)
        str = [NSString stringWithFormat:@"%@\n", str];
    
    NSColor * color = [NSColor whiteColor];
    switch(type) {
        case MessageType_Info:
            color = [NSColor lightGrayColor];
            str = [self prepend:str withIndent:@"    " newline:false];
            break;
        case MessageType_Command:
            color = [NSColor lightGrayColor];
            str = [self prepend:str withIndent:@"    " newline:false];
            break;
        case MessageType_Bootloader:
            color = [NSColor yellowColor];
            str = [self prepend:str withIndent:@"    " newline:false];
            break;
        case MessageType_Error:
            color = [NSColor redColor];
            str = [self prepend:str withIndent:@"    " newline:false];
            break;
        case MessageType_HID:
            color = [NSColor colorWithHue:200.0/360 saturation:.5 brightness:.9 alpha:1.];
            if ([[textView.textStorage string] characterAtIndex:[textView.textStorage length]-1] == '\n')
                str = [self prepend:str withIndent:@"  > " newline:false];
            break;
    }
    
    if (lastMessage != type && [[textView.textStorage string] characterAtIndex:[textView.textStorage length]-1] != '\n')
        str = [NSString stringWithFormat:@"\n%@", str ];
    
    lastMessage = type;
    NSMutableAttributedString *attrStr = [[NSMutableAttributedString alloc] initWithString:str attributes:[self formatCommon:color]];
    return attrStr;
}

- (void)print:(NSString *)str withType:(MessageType)type {
    [textView.textStorage appendAttributedString:[self format:str forType:type]];
    [textView scrollRangeToVisible: NSMakeRange(textView.string.length, 0)];

}
- (void)printResponse:(NSString *)str withType:(MessageType)type {
    [textView.textStorage appendAttributedString:[self formatResponse:str forType:type]];
    [textView scrollRangeToVisible: NSMakeRange(textView.string.length, 0)];
}


@end
