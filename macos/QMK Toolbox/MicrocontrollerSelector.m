#import "MicrocontrollerSelector.h"

@interface MicrocontrollerSelector () <NSComboBoxDelegate, NSComboBoxDataSource>
@property NSMutableArray<NSString *> *keys;
@property NSMutableArray<NSString *> *values;
@end

@implementation MicrocontrollerSelector
- (instancetype)initWithFrame:(NSRect)frame {
    self = [super initWithFrame:frame];
    if (self) {
        [self customInit];
    }
    return self;
}

- (instancetype)initWithCoder:(NSCoder *)coder {
    self = [super initWithCoder:coder];
    if (self) {
        [self customInit];
    }
    return self;
}

- (void)customInit {
    self.keys = [[NSMutableArray alloc] init];
    self.values = [[NSMutableArray alloc] init];

    NSString *path = [[NSBundle mainBundle] pathForResource:@"mcu-list" ofType:@"txt"];
    NSString *fileContents = [NSString stringWithContentsOfFile:path encoding:NSUTF8StringEncoding error:nil];
    NSArray<NSString *> *microcontrollers = [fileContents componentsSeparatedByCharactersInSet:[NSCharacterSet newlineCharacterSet]];

    for (NSString *microcontroller in microcontrollers) {
        if (microcontroller.length > 0) {
            NSArray *parts = [microcontroller componentsSeparatedByString:@":"];
            [self.keys addObject:parts[0]];
            [self.values addObject:parts[1]];
        }
    }

    self.dataSource = self;
    self.delegate = self;

    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    NSString *selectedMicrocontroller = [defaults stringForKey:kMicrocontroller];
    if (selectedMicrocontroller) {
        [self selectItemWithKey:selectedMicrocontroller];
    } else {
        [self selectItemWithKey:@"atmega32u4"];
    }
}

- (void)selectItemWithKey:(NSString *)key {
    for (NSString *item in self.keys) {
        if ([item isEqualToString:key]) {
            [self selectItemAtIndex:[self.keys indexOfObject:item]];
            return;
        }
    }
}

- (NSString *)keyForSelectedItem {
    return self.keys[self.indexOfSelectedItem];
}

- (id)comboBox:(NSComboBox *)comboBox objectValueForItemAtIndex:(NSInteger)index {
    return self.values[index];
}

- (NSInteger)numberOfItemsInComboBox:(NSComboBox *)comboBox {
    return self.values.count;
}

- (NSUInteger)comboBox:(NSComboBox *)comboBox indexOfItemWithStringValue:(NSString *)string {
    for (NSString *value in self.values) {
        if ([value isEqualToString:string]) {
            return [self.values indexOfObject:value];
        }
    }
    return -1;
}

- (void)comboBoxSelectionDidChange:(NSNotification *)notification {
    [[NSUserDefaults standardUserDefaults] setValue:[self keyForSelectedItem] forKey:kMicrocontroller];
}
@end
