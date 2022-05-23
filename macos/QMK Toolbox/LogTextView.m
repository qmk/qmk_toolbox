#import "LogTextView.h"

@implementation LogTextView

- (void)awakeFromNib {
    [self setSelectedTextAttributes:@{
        NSBackgroundColorAttributeName: [NSColor colorNamed:@"LogBoxSelection"],
        NSForegroundColorAttributeName: [NSColor whiteColor]
    }];
}

- (void)logBootloader:(NSString *)message {
    [self log:message withType:MessageTypeBootloader];
}

- (void)logCommand:(NSString *)message {
    [self log:message withType:MessageTypeCommand];
}

- (void)logCommandError:(NSString *)message {
    [self log:message withType:MessageTypeCommandError];
}

- (void)logCommandOutput:(NSString *)message {
    [self log:message withType:MessageTypeCommandOutput];
}

- (void)logError:(NSString *)message {
    [self log:message withType:MessageTypeError];
}

- (void)logHID:(NSString *)message {
    [self log:message withType:MessageTypeHid];
}

- (void)logHIDOutput:(NSString *)message {
    [self log:message withType:MessageTypeHidOutput];
}

- (void)logInfo:(NSString *)message {
    [self log:message withType:MessageTypeInfo];
}

- (void)logUSB:(NSString *)message {
    [self log:message withType:MessageTypeUsb];
}

- (void)log:(NSString *)message withType:(MessageType)type {
    if ([message characterAtIndex:[message length] - 1] == '\n') {
        message = [message substringToIndex:[message length] - 2];
    }

    NSArray<NSString *> *lines = [message componentsSeparatedByString:@"\n"];

    for (NSString *line in lines) {
        switch (type) {
            case MessageTypeBootloader:
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageBootloader"]];
                break;
            case MessageTypeCommand:
                [self appendString:[NSString stringWithFormat:@"> %@\n", line] withColor:[NSColor colorNamed:@"LogMessageDefault"]];
                break;
            case MessageTypeCommandError:
                [self appendString:@"> " withColor:[NSColor colorNamed:@"LogMessageError"]];
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageInfo"]];
                break;
            case MessageTypeCommandOutput:
                [self appendString:@"> " withColor:[NSColor colorNamed:@"LogMessageDefault"]];
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageInfo"]];
                break;
            case MessageTypeError:
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageError"]];
                break;
            case MessageTypeHid:
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageHID"]];
                break;
            case MessageTypeHidOutput:
                [self appendString:@"> " withColor:[NSColor colorNamed:@"LogMessageHID"]];
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageHIDOutput"]];
                break;
            case MessageTypeInfo:
                [self appendString:@"* " withColor:[NSColor colorNamed:@"LogMessageDefault"]];
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageInfo"]];
                break;
            case MessageTypeUsb:
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageDefault"]];
                break;
        }
    }
}

- (void)appendString:(NSString *)string withColor:(NSColor *)color {
    NSDictionary *attrDict = @{
        NSForegroundColorAttributeName: color,
        NSFontAttributeName: [NSFont userFixedPitchFontOfSize:12]
    };
    NSAttributedString *attrString = [[NSAttributedString alloc] initWithString:string attributes:attrDict];
    [self.textStorage appendAttributedString:attrString];
    [self scrollToEndOfDocument:self];
}

@end
