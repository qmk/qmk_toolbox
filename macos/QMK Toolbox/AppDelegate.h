//
//  AppDelegate.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import <Cocoa/Cocoa.h>

#import "Printing.h"

@interface AppDelegate : NSObject <NSApplicationDelegate> {

@private
    BOOL _autoFlashEnabled;
}

@property BOOL canFlash;
@property BOOL canReset;
@property BOOL canClearEEPROM;

@property(nonatomic)  Printing * printer;

- (void)setFilePath:(NSURL *)url;

@end
