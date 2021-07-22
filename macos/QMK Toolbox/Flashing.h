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
    APM32DFU,
    AtmelDFU,
    AtmelSAMBA,
    AVRISP,
    BootloadHID,
    Caterina,
    Halfkay,
    Kiibohd,
    LUFAMS,
    QMKDFU,
    STM32DFU,
    STM32Duino,
    USBAsp,
    USBTiny,
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
- (void)clearEEPROM:(NSString *)mcu;

- (BOOL)canFlash;
- (BOOL)canReset;
- (BOOL)canClearEEPROM;

@property NSString * serialPort;
@property NSString * mountPoint;

@property (nonatomic, assign) id <FlashingDelegate> delegate;

@end
