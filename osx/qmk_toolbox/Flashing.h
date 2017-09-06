//
//  Flashing.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import <Foundation/Foundation.h>
#include <AppKit/AppKit.h>
#include "Printing.h"

@interface Flashing : NSObject

- (id)initWithPrinter:(Printing *)p;
- (void)runProcess:(NSString *)command withArgs:(NSArray<NSString *> *)args;

@end