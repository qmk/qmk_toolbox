//
//  Printing.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import <Foundation/Foundation.h>
#include <AppKit/AppKit.h>

@interface Printing : NSObject

typedef enum {
    MessageType_Info,
    MessageType_Error,
    MessageType_HID,
    MessageType_Bootloader,
    MessageType_Command
} MessageType;

- (id)initWithTextView:(NSTextView *)tV;

- (void)print:(NSString *)str withType:(MessageType)type;
- (void)printResponse:(NSString *)str withType:(MessageType)type;

@end
