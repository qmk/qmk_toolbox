#import "KeyView.h"

@interface KeyView ()
@property (weak) IBOutlet NSView *contentView;

@property (weak) IBOutlet NSBox *boxView;

@property (weak) IBOutlet NSTextField *legendView;
@end

@implementation KeyView
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

- (void)setPressed:(BOOL)pressed {
    _pressed = pressed;
    [self setNeedsDisplay:YES];
}

- (void)setTested:(BOOL)tested {
    _tested = tested;
    [self setNeedsDisplay:YES];
}

- (NSString *)legend {
    return self.legendView.stringValue;
}

- (void)setLegend:(NSString *)legend {
    self.legendView.stringValue = legend;
    [self setNeedsDisplay:YES];
}

- (void)drawRect:(NSRect)dirtyRect {
    [super drawRect:dirtyRect];

    self.boxView.fillColor = self.pressed ? [NSColor systemYellowColor] : self.tested ? [NSColor systemGreenColor] : nil;
    self.legendView.textColor = (self.pressed || self.tested) ? [NSColor blackColor] : [NSColor secondaryLabelColor];
}

- (void)customInit {
    [[NSBundle bundleForClass:self.class] loadNibNamed:@"KeyView" owner:self topLevelObjects:nil];

    NSMutableArray<NSLayoutConstraint *> *newConstraints = [[NSMutableArray alloc] init];

    for (NSLayoutConstraint *oldConstraint in self.contentView.constraints) {
        NSView *firstItem = [oldConstraint.firstItem isEqualTo:self.contentView] ? self : oldConstraint.firstItem;
        NSView *secondItem = [oldConstraint.secondItem isEqualTo:self.contentView] ? self : oldConstraint.secondItem;

        [newConstraints addObject:[NSLayoutConstraint constraintWithItem:firstItem attribute:oldConstraint.firstAttribute relatedBy:oldConstraint.relation toItem:secondItem attribute:oldConstraint.secondAttribute multiplier:oldConstraint.multiplier constant:oldConstraint.constant]];

        for (NSView *newView in self.contentView.subviews) {
            [self addSubview:newView];
        }

        [self addConstraints:newConstraints];
    }
}
@end
