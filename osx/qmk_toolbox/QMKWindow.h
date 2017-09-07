//
//  QMKWindow.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/6/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import "AppDelegate.h"

@interface QMKWindow : NSWindow <NSDraggingDestination>

- (void)setup;

@end
