#import "LogTextView.h"

@implementation LogTextView

- (void)awakeFromNib {
    [self setSelectedTextAttributes:@{
        NSBackgroundColorAttributeName: [NSColor colorNamed:@"LogBoxSelection"],
        NSForegroundColorAttributeName: [NSColor whiteColor]
    }];
}

- (void)logBootloader:(NSString *)message {
    [self log:message withType:MessageType_Bootloader];
}

- (void)logCommand:(NSString *)message {
    [self log:message withType:MessageType_Command];
}

- (void)logCommandError:(NSString *)message {
    [self log:message withType:MessageType_CommandError];
}

- (void)logCommandOutput:(NSString *)message {
    [self log:message withType:MessageType_CommandOutput];
}

- (void)logError:(NSString *)message {
    [self log:message withType:MessageType_Error];
}

- (void)logHID:(NSString *)message {
    [self log:message withType:MessageType_HID];
}

- (void)logHIDOutput:(NSString *)message {
    [self log:message withType:MessageType_HIDOutput];
}

- (void)logInfo:(NSString *)message {
    [self log:message withType:MessageType_Info];
}

- (void)logUSB:(NSString *)message {
    [self log:message withType:MessageType_USB];
}

- (void)log:(NSString *)message withType:(MessageType)type {
    if ([message characterAtIndex:[message length] - 1] == '\n') {
        message = [message substringToIndex:[message length] - 2];
    }

    NSArray<NSString *> *lines = [message componentsSeparatedByString:@"\n"];

    for (NSString *line in lines) {
        switch (type) {
            case MessageType_Bootloader:
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageBootloader"]];
                break;
            case MessageType_Command:
                [self appendString:[NSString stringWithFormat:@"> %@\n", line] withColor:[NSColor colorNamed:@"LogMessageDefault"]];
                break;
            case MessageType_CommandError:
                [self appendString:@"> " withColor:[NSColor colorNamed:@"LogMessageError"]];
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageInfo"]];
                break;
            case MessageType_CommandOutput:
                [self appendString:@"> " withColor:[NSColor colorNamed:@"LogMessageDefault"]];
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageInfo"]];
                break;
            case MessageType_Error:
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageError"]];
                break;
            case MessageType_HID:
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageHID"]];
                break;
            case MessageType_HIDOutput:
                [self appendString:@"> " withColor:[NSColor colorNamed:@"LogMessageHID"]];
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageHIDOutput"]];
                break;
            case MessageType_Info:
                [self appendString:@"* " withColor:[NSColor colorNamed:@"LogMessageDefault"]];
                [self appendString:[NSString stringWithFormat:@"%@\n", line] withColor:[NSColor colorNamed:@"LogMessageInfo"]];
                break;
            case MessageType_USB:
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
