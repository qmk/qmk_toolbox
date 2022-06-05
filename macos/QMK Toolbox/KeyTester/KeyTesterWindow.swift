import AppKit
import Carbon.HIToolbox.Events

class KeyTesterWindow: NSPanel {
    @IBOutlet var keyViewEscape: KeyView!
    @IBOutlet var keyViewF1: KeyView!
    @IBOutlet var keyViewF2: KeyView!
    @IBOutlet var keyViewF3: KeyView!
    @IBOutlet var keyViewF4: KeyView!
    @IBOutlet var keyViewF5: KeyView!
    @IBOutlet var keyViewF6: KeyView!
    @IBOutlet var keyViewF7: KeyView!
    @IBOutlet var keyViewF8: KeyView!
    @IBOutlet var keyViewF9: KeyView!
    @IBOutlet var keyViewF10: KeyView!
    @IBOutlet var keyViewF11: KeyView!
    @IBOutlet var keyViewF12: KeyView!
    @IBOutlet var keyViewF13: KeyView!
    @IBOutlet var keyViewF14: KeyView!
    @IBOutlet var keyViewF15: KeyView!
    @IBOutlet var keyViewF16: KeyView!
    @IBOutlet var keyViewF17: KeyView!
    @IBOutlet var keyViewF18: KeyView!
    @IBOutlet var keyViewF19: KeyView!

    @IBOutlet var keyViewGrave: KeyView!
    @IBOutlet var keyView1: KeyView!
    @IBOutlet var keyView2: KeyView!
    @IBOutlet var keyView3: KeyView!
    @IBOutlet var keyView4: KeyView!
    @IBOutlet var keyView5: KeyView!
    @IBOutlet var keyView6: KeyView!
    @IBOutlet var keyView7: KeyView!
    @IBOutlet var keyView8: KeyView!
    @IBOutlet var keyView9: KeyView!
    @IBOutlet var keyView0: KeyView!
    @IBOutlet var keyViewMinus: KeyView!
    @IBOutlet var keyViewEqual: KeyView!
    @IBOutlet var keyViewBackspace: KeyView!

    @IBOutlet var keyViewTab: KeyView!
    @IBOutlet var keyViewQ: KeyView!
    @IBOutlet var keyViewW: KeyView!
    @IBOutlet var keyViewE: KeyView!
    @IBOutlet var keyViewR: KeyView!
    @IBOutlet var keyViewT: KeyView!
    @IBOutlet var keyViewY: KeyView!
    @IBOutlet var keyViewU: KeyView!
    @IBOutlet var keyViewI: KeyView!
    @IBOutlet var keyViewO: KeyView!
    @IBOutlet var keyViewP: KeyView!
    @IBOutlet var keyViewLeftBracket: KeyView!
    @IBOutlet var keyViewRightBracket: KeyView!
    @IBOutlet var keyViewBackslash: KeyView!

    @IBOutlet var keyViewCapsLock: KeyView!
    @IBOutlet var keyViewA: KeyView!
    @IBOutlet var keyViewS: KeyView!
    @IBOutlet var keyViewD: KeyView!
    @IBOutlet var keyViewF: KeyView!
    @IBOutlet var keyViewG: KeyView!
    @IBOutlet var keyViewH: KeyView!
    @IBOutlet var keyViewJ: KeyView!
    @IBOutlet var keyViewK: KeyView!
    @IBOutlet var keyViewL: KeyView!
    @IBOutlet var keyViewSemicolon: KeyView!
    @IBOutlet var keyViewQuote: KeyView!
    @IBOutlet var keyViewEnter: KeyView!

    @IBOutlet var keyViewLeftShift: KeyView!
    @IBOutlet var keyViewNUBS: KeyView!
    @IBOutlet var keyViewZ: KeyView!
    @IBOutlet var keyViewX: KeyView!
    @IBOutlet var keyViewC: KeyView!
    @IBOutlet var keyViewV: KeyView!
    @IBOutlet var keyViewB: KeyView!
    @IBOutlet var keyViewN: KeyView!
    @IBOutlet var keyViewM: KeyView!
    @IBOutlet var keyViewComma: KeyView!
    @IBOutlet var keyViewDot: KeyView!
    @IBOutlet var keyViewSlash: KeyView!
    @IBOutlet var keyViewRightShift: KeyView!

    @IBOutlet var keyViewFunction: KeyView!
    @IBOutlet var keyViewLeftControl: KeyView!
    @IBOutlet var keyViewLeftOption: KeyView!
    @IBOutlet var keyViewLeftCommand: KeyView!
    @IBOutlet var keyViewSpace: KeyView!
    @IBOutlet var keyViewRightCommand: KeyView!
    @IBOutlet var keyViewRightOption: KeyView!
    @IBOutlet var keyViewMenu: KeyView!
    @IBOutlet var keyViewRightControl: KeyView!

    @IBOutlet var keyViewHelp: KeyView!
    @IBOutlet var keyViewHome: KeyView!
    @IBOutlet var keyViewPageUp: KeyView!
    @IBOutlet var keyViewDelete: KeyView!
    @IBOutlet var keyViewEnd: KeyView!
    @IBOutlet var keyViewPageDown: KeyView!
    @IBOutlet var keyViewUpArrow: KeyView!
    @IBOutlet var keyViewLeftArrow: KeyView!
    @IBOutlet var keyViewDownArrow: KeyView!
    @IBOutlet var keyViewRightArrow: KeyView!

    @IBOutlet var keyViewPadClear: KeyView!
    @IBOutlet var keyViewPadEqual: KeyView!
    @IBOutlet var keyViewPadSlash: KeyView!
    @IBOutlet var keyViewPadAsterisk: KeyView!
    @IBOutlet var keyViewPad7: KeyView!
    @IBOutlet var keyViewPad8: KeyView!
    @IBOutlet var keyViewPad9: KeyView!
    @IBOutlet var keyViewPadMinus: KeyView!
    @IBOutlet var keyViewPad4: KeyView!
    @IBOutlet var keyViewPad5: KeyView!
    @IBOutlet var keyViewPad6: KeyView!
    @IBOutlet var keyViewPadPlus: KeyView!
    @IBOutlet var keyViewPad1: KeyView!
    @IBOutlet var keyViewPad2: KeyView!
    @IBOutlet var keyViewPad3: KeyView!
    @IBOutlet var keyViewPad0: KeyView!
    @IBOutlet var keyViewPadDot: KeyView!
    @IBOutlet var keyViewPadEnter: KeyView!

    @IBOutlet var lastVKLabel: NSTextField!
    @IBOutlet var flagsLabel: NSTextField!

    override func keyDown(with event: NSEvent) {
        keyEvent(true, keyCode: event.keyCode)
    }

    override func keyUp(with event: NSEvent) {
        keyEvent(false, keyCode: event.keyCode)
    }

    override func flagsChanged(with event: NSEvent) {
        flagsLabel.stringValue = String(format: "Flags: %08lX", event.modifierFlags.rawValue)

        var keyDown = false
        switch Int(event.keyCode) {
        case kVK_Control:
            fallthrough
        case kVK_RightControl:
            if event.modifierFlags.contains(.control) {
                keyDown = true
            }
        case kVK_Shift:
            fallthrough
        case kVK_RightShift:
            if event.modifierFlags.contains(.shift) {
                keyDown = true
            }
        case kVK_Option:
            fallthrough
        case kVK_RightOption:
            if event.modifierFlags.contains(.option) {
                keyDown = true
            }
        case kVK_Command:
            fallthrough
        case kVK_RightCommand:
            if event.modifierFlags.contains(.command) {
                keyDown = true
            }
        case kVK_Function:
            if event.modifierFlags.contains(.function) {
                keyDown = true
            }
        case kVK_CapsLock:
            if event.modifierFlags.contains(.capsLock) {
                keyDown = true
            }
        default:
            return
        }

        keyEvent(keyDown, keyCode: event.keyCode)
    }

    func keyEvent(_ keyDown: Bool, keyCode: UInt16) {
        lastVKLabel.stringValue = String(format: "Last VK: %02X", keyCode)

        if let keyView = keyViewFor(keyCode) {
            keyView.pressed = keyDown
            keyView.tested = true
        }
    }

    func keyViewFor(_ keyCode: UInt16) -> KeyView? {
        switch Int(keyCode) {
        case kVK_Escape:
            return keyViewEscape
        case kVK_F1:
            return keyViewF1
        case kVK_F2:
            return keyViewF2
        case kVK_F3:
            return keyViewF3
        case kVK_F4:
            return keyViewF4
        case kVK_F5:
            return keyViewF5
        case kVK_F6:
            return keyViewF6
        case kVK_F7:
            return keyViewF7
        case kVK_F8:
            return keyViewF8
        case kVK_F9:
            return keyViewF9
        case kVK_F10:
            return keyViewF10
        case kVK_F11:
            return keyViewF11
        case kVK_F12:
            return keyViewF12
        case kVK_F13:
            return keyViewF13
        case kVK_F14:
            return keyViewF14
        case kVK_F15:
            return keyViewF15
        case kVK_F16:
            return keyViewF16
        case kVK_F17:
            return keyViewF17
        case kVK_F18:
            return keyViewF18
        case kVK_F19:
            return keyViewF19

        case kVK_ANSI_Grave:
            return keyViewGrave
        case kVK_ANSI_1:
            return keyView1
        case kVK_ANSI_2:
            return keyView2
        case kVK_ANSI_3:
            return keyView3
        case kVK_ANSI_4:
            return keyView4
        case kVK_ANSI_5:
            return keyView5
        case kVK_ANSI_6:
            return keyView6
        case kVK_ANSI_7:
            return keyView7
        case kVK_ANSI_8:
            return keyView8
        case kVK_ANSI_9:
            return keyView9
        case kVK_ANSI_0:
            return keyView0
        case kVK_ANSI_Minus:
            return keyViewMinus
        case kVK_ANSI_Equal:
            return keyViewEqual
        case kVK_Delete:
            return keyViewBackspace

        case kVK_Tab:
            return keyViewTab
        case kVK_ANSI_Q:
            return keyViewQ
        case kVK_ANSI_W:
            return keyViewW
        case kVK_ANSI_E:
            return keyViewE
        case kVK_ANSI_R:
            return keyViewR
        case kVK_ANSI_T:
            return keyViewT
        case kVK_ANSI_Y:
            return keyViewY
        case kVK_ANSI_U:
            return keyViewU
        case kVK_ANSI_I:
            return keyViewI
        case kVK_ANSI_O:
            return keyViewO
        case kVK_ANSI_P:
            return keyViewP
        case kVK_ANSI_LeftBracket:
            return keyViewLeftBracket
        case kVK_ANSI_RightBracket:
            return keyViewRightBracket
        case kVK_ANSI_Backslash:
            return keyViewBackslash

        case kVK_CapsLock:
            return keyViewCapsLock
        case kVK_ANSI_A:
            return keyViewA
        case kVK_ANSI_S:
            return keyViewS
        case kVK_ANSI_D:
            return keyViewD
        case kVK_ANSI_F:
            return keyViewF
        case kVK_ANSI_G:
            return keyViewG
        case kVK_ANSI_H:
            return keyViewH
        case kVK_ANSI_J:
            return keyViewJ
        case kVK_ANSI_K:
            return keyViewK
        case kVK_ANSI_L:
            return keyViewL
        case kVK_ANSI_Semicolon:
            return keyViewSemicolon
        case kVK_ANSI_Quote:
            return keyViewQuote
        case kVK_Return:
            return keyViewEnter

        case kVK_Shift:
            return keyViewLeftShift
        case kVK_ISO_Section:
            return keyViewNUBS
        case kVK_ANSI_Z:
            return keyViewZ
        case kVK_ANSI_X:
            return keyViewX
        case kVK_ANSI_C:
            return keyViewC
        case kVK_ANSI_V:
            return keyViewV
        case kVK_ANSI_B:
            return keyViewB
        case kVK_ANSI_N:
            return keyViewN
        case kVK_ANSI_M:
            return keyViewM
        case kVK_ANSI_Comma:
            return keyViewComma
        case kVK_ANSI_Period:
            return keyViewDot
        case kVK_ANSI_Slash:
            return keyViewSlash
        case kVK_RightShift:
            return keyViewRightShift

        case kVK_Function:
            return keyViewFunction
        case kVK_Control:
            return keyViewLeftControl
        case kVK_Option:
            return keyViewLeftOption
        case kVK_Command:
            return keyViewLeftCommand
        case kVK_Space:
            return keyViewSpace
        case kVK_RightCommand:
            return keyViewRightCommand
        case kVK_RightOption:
            return keyViewRightOption
        case 0x6E: // No enum entry in Events.h for this one
            return keyViewMenu
        case kVK_RightControl:
            return keyViewRightControl

        case kVK_Help:
            return keyViewHelp
        case kVK_Home:
            return keyViewHome
        case kVK_PageUp:
            return keyViewPageUp
        case kVK_ForwardDelete:
            return keyViewDelete
        case kVK_End:
            return keyViewEnd
        case kVK_PageDown:
            return keyViewPageDown
        case kVK_UpArrow:
            return keyViewUpArrow
        case kVK_LeftArrow:
            return keyViewLeftArrow
        case kVK_DownArrow:
            return keyViewDownArrow
        case kVK_RightArrow:
            return keyViewRightArrow

        case kVK_ANSI_KeypadClear:
            return keyViewPadClear
        case kVK_ANSI_KeypadEquals:
            return keyViewPadEqual
        case kVK_ANSI_KeypadDivide:
            return keyViewPadSlash
        case kVK_ANSI_KeypadMultiply:
            return keyViewPadAsterisk
        case kVK_ANSI_Keypad7:
            return keyViewPad7
        case kVK_ANSI_Keypad8:
            return keyViewPad8
        case kVK_ANSI_Keypad9:
            return keyViewPad9
        case kVK_ANSI_KeypadMinus:
            return keyViewPadMinus
        case kVK_ANSI_Keypad4:
            return keyViewPad4
        case kVK_ANSI_Keypad5:
            return keyViewPad5
        case kVK_ANSI_Keypad6:
            return keyViewPad6
        case kVK_ANSI_KeypadPlus:
            return keyViewPadPlus
        case kVK_ANSI_Keypad1:
            return keyViewPad1
        case kVK_ANSI_Keypad2:
            return keyViewPad2
        case kVK_ANSI_Keypad3:
            return keyViewPad3
        case kVK_ANSI_Keypad0:
            return keyViewPad0
        case kVK_ANSI_KeypadDecimal:
            return keyViewPadDot
        case kVK_ANSI_KeypadEnter:
            return keyViewPadEnter
        default:
            return nil
        }
    }
}
