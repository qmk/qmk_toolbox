//
//  AppDelegate.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import <Cocoa/Cocoa.h>
#import "Printing.h"
#import "Flashing.h"
#import "HID.h"
#import "USB.h"

@interface AppDelegate : NSObject <NSApplicationDelegate>

@property Printing * printer;

- (void)setFilePath:(NSURL *)url;

@end

