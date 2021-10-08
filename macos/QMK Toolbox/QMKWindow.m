#import "QMKWindow.h"

#import "AppDelegate.h"

@implementation QMKWindow

- (void)setup {
    [self registerForDraggedTypes:[NSArray arrayWithObject:NSPasteboardTypeFileURL]];
};

- (NSDragOperation)draggingEntered:(id<NSDraggingInfo>)sender {
    NSPasteboard *pboard = [sender draggingPasteboard];

    if ([[pboard pasteboardItems] count] == 1) {
        NSString *fileExtension = [[NSURL URLFromPasteboard:pboard] pathExtension];

        if ([fileExtension isEqualToString:@"qmk"] || [fileExtension isEqualToString:@"hex"] || [fileExtension isEqualToString:@"bin"]) {
            return NSDragOperationCopy;
        }
    }

    return NSDragOperationNone;
}

- (BOOL)performDragOperation:(id<NSDraggingInfo>)sender {
    NSPasteboard *pboard = [sender draggingPasteboard];
    NSString *file = [[NSURL URLFromPasteboard:pboard] path];
    [(AppDelegate *)[[NSApplication sharedApplication] delegate] setFilePath:[NSURL fileURLWithPath:file]];
    return YES;
}

@end
