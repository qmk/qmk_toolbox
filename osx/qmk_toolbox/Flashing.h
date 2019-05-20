//
//  Flashing.h
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/5/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import <Foundation/Foundation.h>
#include <AppKit/AppKit.h>
#include "Printing.h"

typedef enum {
    DFU,
    Halfkay,
    Caterina,
    CaterinaAlt,
    FeatherBLE32u4,
    STM32,
    Kiibohd,
    AVRISP,
    USBTiny,
    USBAsp,
    NumberOfChipsets
} Chipset;

@class Flashing;
@protocol FlashingDelegate <NSObject>
@optional
- (BOOL)canFlash:(Chipset)chipset;
@end

@interface Flashing : NSObject

- (id)initWithPrinter:(Printing *)p;
- (NSString *)runProcess:(NSString *)command withArgs:(NSArray<NSString *> *)args;

- (void)flash:(NSString *)mcu withFile:(NSString *)file;
- (void)reset:(NSString *)mcu;
- (void)eepromReset:(NSString *)mcu;
@property NSString * caterinaPort;

@property (nonatomic, assign) id <FlashingDelegate> delegate;

@end
