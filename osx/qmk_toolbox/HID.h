//
//  HID.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>
#import "Printing.h"

@interface HID : NSObject

+ (void)setupWithPrinter:(Printing *)printer;

@end