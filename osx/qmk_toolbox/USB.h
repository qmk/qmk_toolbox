//
//  USB.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
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
- (void)setCaterinaPort:(NSString *)port;
@end

@interface USB : NSObject

+ (void)setupWithPrinter:(Printing *)printer;
+ (void)setupWithPrinter:(Printing *)printer andDelegate:(id<USBDelegate>)d;
+ (BOOL)areDevicesAvailable;
+ (BOOL)canFlash:(Chipset)chipset;

@end

static id<USBDelegate> delegate;
