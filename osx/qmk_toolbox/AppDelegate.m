//
//  AppDelegate.m
//  qmk_toolbox
//
//  Created by Jack Humbert on 9/3/17.
//  Copyright © 2017 QMK. All rights reserved.
//

#import "AppDelegate.h"

@interface AppDelegate () <NSTextViewDelegate>

@property (weak) IBOutlet NSWindow *window;
@property IBOutlet NSTextView * textView;

@end

@implementation AppDelegate

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
    // Insert code here to initialize your application
    [_textView.textStorage appendAttributedString:[[NSAttributedString alloc] initWithString:@"Welcome!"]];
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
    // Insert code here to tear down your application
}

@end
