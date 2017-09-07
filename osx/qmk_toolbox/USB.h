//
//  USB.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>
#import "Printing.h"
#import "Flashing.h"

@class USB;
@protocol USBDelegate <NSObject>
@optional
- (void)deviceConnected:(Chipset)chipset;
- (void)deviceDisconnected:(Chipset)chipset;
@end

@interface USB : NSObject

+ (void)setupWithPrinter:(Printing *)printer andDelegate:(id<USBDelegate>)d;

@end

static id<USBDelegate> delegate;