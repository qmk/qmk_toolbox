//
//  QMKWindow.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/6/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).
//

#import "QMKWindow.h"

@implementation QMKWindow

- (void)setup {
    [self registerForDraggedTypes:[NSArray arrayWithObject:NSFilenamesPboardType]];
};

- (NSDragOperation)draggingEntered:(id <NSDraggingInfo>)sender {
    NSPasteboard *pboard = [sender draggingPasteboard];

    if ([[pboard types] containsObject:NSFilenamesPboardType]) {
        if ([[pboard pasteboardItems] count] == 1) {
            NSString *file = [pboard propertyListForType:NSFilenamesPboardType][0];
            NSString * fileExtension = [[file pathExtension] lowercaseString];

            if ([fileExtension isEqualToString:@"qmk"] || [fileExtension isEqualToString:@"hex"] || [fileExtension isEqualToString:@"bin"]) {
                return NSDragOperationCopy;
            }
        }
    }

    return NSDragOperationNone;
}

- (BOOL)performDragOperation:(id <NSDraggingInfo>)sender {
    NSPasteboard *pboard = [sender draggingPasteboard];
    NSString *file = [pboard propertyListForType:NSFilenamesPboardType][0];
    [(AppDelegate *)[[NSApplication sharedApplication] delegate] setFilePath:[NSURL fileURLWithPath:file]];
    return YES;
}

@end
