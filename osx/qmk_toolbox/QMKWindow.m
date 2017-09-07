//
//  QMKWindow.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/6/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "QMKWindow.h"

@implementation QMKWindow

- (void)setup {
    [self registerForDraggedTypes:[NSArray arrayWithObject:NSFilenamesPboardType]];
//    [self registerForDraggedTypes:[NSArray arrayWithObjects:
//        NSCreateFileContentsPboardType(@"qmk"),
//        NSCreateFileContentsPboardType(@"hex"),
//        NSCreateFileContentsPboardType(@"bin"),
//    nil]];
};

- (NSDragOperation)draggingEntered:(id <NSDraggingInfo>)sender {
    NSPasteboard *pboard;
    NSDragOperation sourceDragMask;
 
    sourceDragMask = [sender draggingSourceOperationMask];
    pboard = [sender draggingPasteboard];
 
    if ( [[pboard types] containsObject:NSFilenamesPboardType] ) {
        if (sourceDragMask & NSDragOperationLink) {
            return NSDragOperationLink;
        }
    }
    
    return NSDragOperationNone;
}

- (BOOL)performDragOperation:(id <NSDraggingInfo>)sender {
    NSPasteboard *pboard;
    NSDragOperation sourceDragMask;
 
    sourceDragMask = [sender draggingSourceOperationMask];
    pboard = [sender draggingPasteboard];
 
    NSArray *files = [pboard propertyListForType:NSFilenamesPboardType];
    for (NSString * file in files) {
        if ([[[file pathExtension] lowercaseString] isEqualToString:@"qmk"] ||
            [[[file pathExtension] lowercaseString] isEqualToString:@"hex"] ||
            [[[file pathExtension] lowercaseString] isEqualToString:@"bin"]) {
            [[[NSApplication sharedApplication] delegate] setFilePath:[NSURL URLWithString:file]];
        } else {
            NSAlert *alert = [[NSAlert alloc] init];

            [alert setMessageText:@"This file format isn't supported"];
            [alert addButtonWithTitle:@"Sorry"];
            [alert runModal];
        }
    }
    return YES;
}

@end
