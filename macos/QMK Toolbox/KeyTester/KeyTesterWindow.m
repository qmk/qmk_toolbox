#import <Carbon/Carbon.h>

#import "KeyTesterWindow.h"
#import "KeyView.h"

@interface KeyTesterWindow ()
@property (weak) IBOutlet KeyView * keyViewEscape;
@property (weak) IBOutlet KeyView * keyViewF1;
@property (weak) IBOutlet KeyView * keyViewF2;
@property (weak) IBOutlet KeyView * keyViewF3;
@property (weak) IBOutlet KeyView * keyViewF4;
@property (weak) IBOutlet KeyView * keyViewF5;
@property (weak) IBOutlet KeyView * keyViewF6;
@property (weak) IBOutlet KeyView * keyViewF7;
@property (weak) IBOutlet KeyView * keyViewF8;
@property (weak) IBOutlet KeyView * keyViewF9;
@property (weak) IBOutlet KeyView * keyViewF10;
@property (weak) IBOutlet KeyView * keyViewF11;
@property (weak) IBOutlet KeyView * keyViewF12;
@property (weak) IBOutlet KeyView * keyViewF13;
@property (weak) IBOutlet KeyView * keyViewF14;
@property (weak) IBOutlet KeyView * keyViewF15;
@property (weak) IBOutlet KeyView * keyViewF16;
@property (weak) IBOutlet KeyView * keyViewF17;
@property (weak) IBOutlet KeyView * keyViewF18;
@property (weak) IBOutlet KeyView * keyViewF19;

@property (weak) IBOutlet KeyView * keyViewGrave;
@property (weak) IBOutlet KeyView * keyView1;
@property (weak) IBOutlet KeyView * keyView2;
@property (weak) IBOutlet KeyView * keyView3;
@property (weak) IBOutlet KeyView * keyView4;
@property (weak) IBOutlet KeyView * keyView5;
@property (weak) IBOutlet KeyView * keyView6;
@property (weak) IBOutlet KeyView * keyView7;
@property (weak) IBOutlet KeyView * keyView8;
@property (weak) IBOutlet KeyView * keyView9;
@property (weak) IBOutlet KeyView * keyView0;
@property (weak) IBOutlet KeyView * keyViewMinus;
@property (weak) IBOutlet KeyView * keyViewEqual;
@property (weak) IBOutlet KeyView * keyViewBackspace;

@property (weak) IBOutlet KeyView * keyViewTab;
@property (weak) IBOutlet KeyView * keyViewQ;
@property (weak) IBOutlet KeyView * keyViewW;
@property (weak) IBOutlet KeyView * keyViewE;
@property (weak) IBOutlet KeyView * keyViewR;
@property (weak) IBOutlet KeyView * keyViewT;
@property (weak) IBOutlet KeyView * keyViewY;
@property (weak) IBOutlet KeyView * keyViewU;
@property (weak) IBOutlet KeyView * keyViewI;
@property (weak) IBOutlet KeyView * keyViewO;
@property (weak) IBOutlet KeyView * keyViewP;
@property (weak) IBOutlet KeyView * keyViewLeftBracket;
@property (weak) IBOutlet KeyView * keyViewRightBracket;
@property (weak) IBOutlet KeyView * keyViewBackslash;

@property (weak) IBOutlet KeyView * keyViewCapsLock;
@property (weak) IBOutlet KeyView * keyViewA;
@property (weak) IBOutlet KeyView * keyViewS;
@property (weak) IBOutlet KeyView * keyViewD;
@property (weak) IBOutlet KeyView * keyViewF;
@property (weak) IBOutlet KeyView * keyViewG;
@property (weak) IBOutlet KeyView * keyViewH;
@property (weak) IBOutlet KeyView * keyViewJ;
@property (weak) IBOutlet KeyView * keyViewK;
@property (weak) IBOutlet KeyView * keyViewL;
@property (weak) IBOutlet KeyView * keyViewSemicolon;
@property (weak) IBOutlet KeyView * keyViewQuote;
@property (weak) IBOutlet KeyView * keyViewEnter;

@property (weak) IBOutlet KeyView * keyViewLeftShift;
@property (weak) IBOutlet KeyView * keyViewNUBS;
@property (weak) IBOutlet KeyView * keyViewZ;
@property (weak) IBOutlet KeyView * keyViewX;
@property (weak) IBOutlet KeyView * keyViewC;
@property (weak) IBOutlet KeyView * keyViewV;
@property (weak) IBOutlet KeyView * keyViewB;
@property (weak) IBOutlet KeyView * keyViewN;
@property (weak) IBOutlet KeyView * keyViewM;
@property (weak) IBOutlet KeyView * keyViewComma;
@property (weak) IBOutlet KeyView * keyViewDot;
@property (weak) IBOutlet KeyView * keyViewSlash;
@property (weak) IBOutlet KeyView * keyViewRightShift;

@property (weak) IBOutlet KeyView * keyViewFunction;
@property (weak) IBOutlet KeyView * keyViewLeftControl;
@property (weak) IBOutlet KeyView * keyViewLeftOption;
@property (weak) IBOutlet KeyView * keyViewLeftCommand;
@property (weak) IBOutlet KeyView * keyViewSpace;
@property (weak) IBOutlet KeyView * keyViewRightCommand;
@property (weak) IBOutlet KeyView * keyViewRightOption;
@property (weak) IBOutlet KeyView * keyViewMenu;
@property (weak) IBOutlet KeyView * keyViewRightControl;

@property (weak) IBOutlet KeyView * keyViewHelp;
@property (weak) IBOutlet KeyView * keyViewHome;
@property (weak) IBOutlet KeyView * keyViewPageUp;
@property (weak) IBOutlet KeyView * keyViewDelete;
@property (weak) IBOutlet KeyView * keyViewEnd;
@property (weak) IBOutlet KeyView * keyViewPageDown;
@property (weak) IBOutlet KeyView * keyViewUpArrow;
@property (weak) IBOutlet KeyView * keyViewLeftArrow;
@property (weak) IBOutlet KeyView * keyViewDownArrow;
@property (weak) IBOutlet KeyView * keyViewRightArrow;

@property (weak) IBOutlet KeyView * keyViewPadClear;
@property (weak) IBOutlet KeyView * keyViewPadEqual;
@property (weak) IBOutlet KeyView * keyViewPadSlash;
@property (weak) IBOutlet KeyView * keyViewPadAsterisk;
@property (weak) IBOutlet KeyView * keyViewPad7;
@property (weak) IBOutlet KeyView * keyViewPad8;
@property (weak) IBOutlet KeyView * keyViewPad9;
@property (weak) IBOutlet KeyView * keyViewPadMinus;
@property (weak) IBOutlet KeyView * keyViewPad4;
@property (weak) IBOutlet KeyView * keyViewPad5;
@property (weak) IBOutlet KeyView * keyViewPad6;
@property (weak) IBOutlet KeyView * keyViewPadPlus;
@property (weak) IBOutlet KeyView * keyViewPad1;
@property (weak) IBOutlet KeyView * keyViewPad2;
@property (weak) IBOutlet KeyView * keyViewPad3;
@property (weak) IBOutlet KeyView * keyViewPad0;
@property (weak) IBOutlet KeyView * keyViewPadDot;
@property (weak) IBOutlet KeyView * keyViewPadEnter;

@property (weak) IBOutlet NSTextField * lastVKLabel;
@property (weak) IBOutlet NSTextField * flagsLabel;
@end

@implementation KeyTesterWindow
- (void)keyDown:(NSEvent *)event {
    [self keyEvent:YES withKeyCode:event.keyCode];
}

- (void)keyUp:(NSEvent *)event {
    [self keyEvent:NO withKeyCode:event.keyCode];
}

- (void)flagsChanged:(NSEvent *)event {
    self.flagsLabel.stringValue = [NSString stringWithFormat:@"Flags: %08lX", event.modifierFlags];

    BOOL keyDown = NO;

    switch (event.keyCode) {
        case kVK_Control:
        case kVK_RightControl:
            if (event.modifierFlags & NSEventModifierFlagControl) {
                keyDown = YES;
            }
            break;
        case kVK_Shift:
        case kVK_RightShift:
            if (event.modifierFlags & NSEventModifierFlagShift) {
                keyDown = YES;
            }
            break;
        case kVK_Option:
        case kVK_RightOption:
            if (event.modifierFlags & NSEventModifierFlagOption) {
                keyDown = YES;
            }
            break;
        case kVK_Command:
        case kVK_RightCommand:
            if (event.modifierFlags & NSEventModifierFlagCommand) {
                keyDown = YES;
            }
            break;
        case kVK_Function:
            if (event.modifierFlags & NSEventModifierFlagFunction) {
                keyDown = YES;
            }
            break;
        case kVK_CapsLock:
            if (event.modifierFlags & NSEventModifierFlagCapsLock) {
                keyDown = YES;
            }
            break;
        default:
            return;
    }

    [self keyEvent:keyDown withKeyCode:event.keyCode];
}

- (void)keyEvent:(BOOL)keyDown withKeyCode:(ushort)keyCode {
    self.lastVKLabel.stringValue = [NSString stringWithFormat:@"Last VK: %02X", keyCode];

    KeyView *keyView = [self keyViewFor:keyCode];

    if (keyView != nil) {
        keyView.pressed = keyDown;
        keyView.tested = YES;
    }
}

- (KeyView *)keyViewFor:(ushort)keyCode {
    switch (keyCode) {
        case kVK_Escape:
            return self.keyViewEscape;
        case kVK_F1:
            return self.keyViewF1;
        case kVK_F2:
            return self.keyViewF2;
        case kVK_F3:
            return self.keyViewF3;
        case kVK_F4:
            return self.keyViewF4;
        case kVK_F5:
            return self.keyViewF5;
        case kVK_F6:
            return self.keyViewF6;
        case kVK_F7:
            return self.keyViewF7;
        case kVK_F8:
            return self.keyViewF8;
        case kVK_F9:
            return self.keyViewF9;
        case kVK_F10:
            return self.keyViewF10;
        case kVK_F11:
            return self.keyViewF11;
        case kVK_F12:
            return self.keyViewF12;
        case kVK_F13:
            return self.keyViewF13;
        case kVK_F14:
            return self.keyViewF14;
        case kVK_F15:
            return self.keyViewF15;
        case kVK_F16:
            return self.keyViewF16;
        case kVK_F17:
            return self.keyViewF17;
        case kVK_F18:
            return self.keyViewF18;
        case kVK_F19:
            return self.keyViewF19;

        case kVK_ANSI_Grave:
            return self.keyViewGrave;
        case kVK_ANSI_1:
            return self.keyView1;
        case kVK_ANSI_2:
            return self.keyView2;
        case kVK_ANSI_3:
            return self.keyView3;
        case kVK_ANSI_4:
            return self.keyView4;
        case kVK_ANSI_5:
            return self.keyView5;
        case kVK_ANSI_6:
            return self.keyView6;
        case kVK_ANSI_7:
            return self.keyView7;
        case kVK_ANSI_8:
            return self.keyView8;
        case kVK_ANSI_9:
            return self.keyView9;
        case kVK_ANSI_0:
            return self.keyView0;
        case kVK_ANSI_Minus:
            return self.keyViewMinus;
        case kVK_ANSI_Equal:
            return self.keyViewEqual;
        case kVK_Delete:
            return self.keyViewBackspace;

        case kVK_Tab:
            return self.keyViewTab;
        case kVK_ANSI_Q:
            return self.keyViewQ;
        case kVK_ANSI_W:
            return self.keyViewW;
        case kVK_ANSI_E:
            return self.keyViewE;
        case kVK_ANSI_R:
            return self.keyViewR;
        case kVK_ANSI_T:
            return self.keyViewT;
        case kVK_ANSI_Y:
            return self.keyViewY;
        case kVK_ANSI_U:
            return self.keyViewU;
        case kVK_ANSI_I:
            return self.keyViewI;
        case kVK_ANSI_O:
            return self.keyViewO;
        case kVK_ANSI_P:
            return self.keyViewP;
        case kVK_ANSI_LeftBracket:
            return self.keyViewLeftBracket;
        case kVK_ANSI_RightBracket:
            return self.keyViewRightBracket;
        case kVK_ANSI_Backslash:
            return self.keyViewBackslash;

        case kVK_CapsLock:
            return self.keyViewCapsLock;
        case kVK_ANSI_A:
            return self.keyViewA;
        case kVK_ANSI_S:
            return self.keyViewS;
        case kVK_ANSI_D:
            return self.keyViewD;
        case kVK_ANSI_F:
            return self.keyViewF;
        case kVK_ANSI_G:
            return self.keyViewG;
        case kVK_ANSI_H:
            return self.keyViewH;
        case kVK_ANSI_J:
            return self.keyViewJ;
        case kVK_ANSI_K:
            return self.keyViewK;
        case kVK_ANSI_L:
            return self.keyViewL;
        case kVK_ANSI_Semicolon:
            return self.keyViewSemicolon;
        case kVK_ANSI_Quote:
            return self.keyViewQuote;
        case kVK_Return:
            return self.keyViewEnter;

        case kVK_Shift:
            return self.keyViewLeftShift;
        case kVK_ISO_Section:
            return self.keyViewNUBS;
        case kVK_ANSI_Z:
            return self.keyViewZ;
        case kVK_ANSI_X:
            return self.keyViewX;
        case kVK_ANSI_C:
            return self.keyViewC;
        case kVK_ANSI_V:
            return self.keyViewV;
        case kVK_ANSI_B:
            return self.keyViewB;
        case kVK_ANSI_N:
            return self.keyViewN;
        case kVK_ANSI_M:
            return self.keyViewM;
        case kVK_ANSI_Comma:
            return self.keyViewComma;
        case kVK_ANSI_Period:
            return self.keyViewDot;
        case kVK_ANSI_Slash:
            return self.keyViewSlash;
        case kVK_RightShift:
            return self.keyViewRightShift;

        case kVK_Function:
            return self.keyViewFunction;
        case kVK_Control:
            return self.keyViewLeftControl;
        case kVK_Option:
            return self.keyViewLeftOption;
        case kVK_Command:
            return self.keyViewLeftCommand;
        case kVK_Space:
            return self.keyViewSpace;
        case kVK_RightCommand:
            return self.keyViewRightCommand;
        case kVK_RightOption:
            return self.keyViewRightOption;
        case 0x6E: // No enum entry in Events.h for this one
            return self.keyViewMenu;
        case kVK_RightControl:
            return self.keyViewRightControl;

        case kVK_Help:
            return self.keyViewHelp;
        case kVK_Home:
            return self.keyViewHome;
        case kVK_PageUp:
            return self.keyViewPageUp;
        case kVK_ForwardDelete:
            return self.keyViewDelete;
        case kVK_End:
            return self.keyViewEnd;
        case kVK_PageDown:
            return self.keyViewPageDown;
        case kVK_UpArrow:
            return self.keyViewUpArrow;
        case kVK_LeftArrow:
            return self.keyViewLeftArrow;
        case kVK_DownArrow:
            return self.keyViewDownArrow;
        case kVK_RightArrow:
            return self.keyViewRightArrow;

        case kVK_ANSI_KeypadClear:
            return self.keyViewPadClear;
        case kVK_ANSI_KeypadEquals:
            return self.keyViewPadEqual;
        case kVK_ANSI_KeypadDivide:
            return self.keyViewPadSlash;
        case kVK_ANSI_KeypadMultiply:
            return self.keyViewPadAsterisk;
        case kVK_ANSI_Keypad7:
            return self.keyViewPad7;
        case kVK_ANSI_Keypad8:
            return self.keyViewPad8;
        case kVK_ANSI_Keypad9:
            return self.keyViewPad9;
        case kVK_ANSI_KeypadMinus:
            return self.keyViewPadMinus;
        case kVK_ANSI_Keypad4:
            return self.keyViewPad4;
        case kVK_ANSI_Keypad5:
            return self.keyViewPad5;
        case kVK_ANSI_Keypad6:
            return self.keyViewPad6;
        case kVK_ANSI_KeypadPlus:
            return self.keyViewPadPlus;
        case kVK_ANSI_Keypad1:
            return self.keyViewPad1;
        case kVK_ANSI_Keypad2:
            return self.keyViewPad2;
        case kVK_ANSI_Keypad3:
            return self.keyViewPad3;
        case kVK_ANSI_Keypad0:
            return self.keyViewPad0;
        case kVK_ANSI_KeypadDecimal:
            return self.keyViewPadDot;
        case kVK_ANSI_KeypadEnter:
            return self.keyViewPadEnter;
    }

    return nil;
}
@end
