//
//  QMKWindow.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/6/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import <Cocoa/Cocoa.h>
#import "AppDelegate.h"

@interface QMKWindow : NSWindow <NSDraggingDestination>

- (void)setup;

@end
