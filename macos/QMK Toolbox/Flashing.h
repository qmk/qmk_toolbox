#import <Foundation/Foundation.h>

#import "Printing.h"

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
- (void)setHandedness:(NSString *)mcu rightHand:(BOOL)rightHand;

- (BOOL)canFlash;
- (BOOL)canReset;
- (BOOL)canClearEEPROM;

@property NSString *serialPort;
@property NSString *mountPoint;

@property (nonatomic, assign) id<FlashingDelegate> delegate;

@end
