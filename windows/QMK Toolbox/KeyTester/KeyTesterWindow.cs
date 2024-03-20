using System;
using System.Windows.Forms;

namespace QMK_Toolbox.KeyTester
{
    public partial class KeyTesterWindow : Form
    {
        private const int WM_KEYDOWN    = 0x100;
        private const int WM_KEYUP      = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP   = 0x105;

        // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        private const int VK_VOLUME_MUTE      = 0xAD;
        private const int VK_VOLUME_DOWN      = 0xAE;
        private const int VK_VOLUME_UP        = 0xAF;
        private const int VK_MEDIA_NEXT_TRACK = 0xB0;
        private const int VK_MEDIA_PREV_TRACK = 0xB1;
        private const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const int VK_LAUNCH_MAIL      = 0xB4;
        private const int VK_LAUNCH_APP2      = 0xB7;
        private const int VK_OEM_5            = 0xDC;

        // https://download.microsoft.com/download/1/6/1/161ba512-40e2-4cc9-843a-923143f3456c/translate.pdf
        // These are mostly PS/2 equivalent, regardless of transport (eg. USB HID)
        // presumably for backwards compatibility from the time where there was only PS/2
        private const int SC_ESCAPE        = 0x001;
        private const int SC_1             = 0x002;
        private const int SC_2             = 0x003;
        private const int SC_3             = 0x004;
        private const int SC_4             = 0x005;
        private const int SC_5             = 0x006;
        private const int SC_6             = 0x007;
        private const int SC_7             = 0x008;
        private const int SC_8             = 0x009;
        private const int SC_9             = 0x00A;
        private const int SC_0             = 0x00B;
        private const int SC_MINUS         = 0x00C;
        private const int SC_EQUAL         = 0x00D;
        private const int SC_BACKSPACE     = 0x00E;
        private const int SC_TAB           = 0x00F;
        private const int SC_Q             = 0x010;
        private const int SC_W             = 0x011;
        private const int SC_E             = 0x012;
        private const int SC_R             = 0x013;
        private const int SC_T             = 0x014;
        private const int SC_Y             = 0x015;
        private const int SC_U             = 0x016;
        private const int SC_I             = 0x017;
        private const int SC_O             = 0x018;
        private const int SC_P             = 0x019;
        private const int SC_LEFT_BRACE    = 0x01A;
        private const int SC_RIGHT_BRACE   = 0x01B;
        private const int SC_ENTER         = 0x01C;
        private const int SC_LEFT_CONTROL  = 0x01D;
        private const int SC_A             = 0x01E;
        private const int SC_S             = 0x01F;
        private const int SC_D             = 0x020;
        private const int SC_F             = 0x021;
        private const int SC_G             = 0x022;
        private const int SC_H             = 0x023;
        private const int SC_J             = 0x024;
        private const int SC_K             = 0x025;
        private const int SC_L             = 0x026;
        private const int SC_SEMICOLON     = 0x027;
        private const int SC_QUOTE         = 0x028;
        private const int SC_GRAVE         = 0x029;
        private const int SC_LEFT_SHIFT    = 0x02A;
        private const int SC_BACKSLASH     = 0x02B;
        private const int SC_Z             = 0x02C;
        private const int SC_X             = 0x02D;
        private const int SC_C             = 0x02E;
        private const int SC_V             = 0x02F;
        private const int SC_B             = 0x030;
        private const int SC_N             = 0x031;
        private const int SC_M             = 0x032;
        private const int SC_COMMA         = 0x033;
        private const int SC_DOT           = 0x034;
        private const int SC_SLASH         = 0x035;
        private const int SC_RIGHT_SHIFT   = 0x036;
        private const int SC_PAD_ASTERISK  = 0x037;
        private const int SC_LEFT_ALT      = 0x038;
        private const int SC_SPACE         = 0x039;
        private const int SC_CAPS_LOCK     = 0x03A;
        private const int SC_F1            = 0x03B;
        private const int SC_F2            = 0x03C;
        private const int SC_F3            = 0x03D;
        private const int SC_F4            = 0x03E;
        private const int SC_F5            = 0x03F;
        private const int SC_F6            = 0x040;
        private const int SC_F7            = 0x041;
        private const int SC_F8            = 0x042;
        private const int SC_F9            = 0x043;
        private const int SC_F10           = 0x044;
        private const int SC_PAUSE         = 0x045;
        private const int SC_SCROLL_LOCK   = 0x046;
        private const int SC_PAD_7         = 0x047;
        private const int SC_PAD_8         = 0x048;
        private const int SC_PAD_9         = 0x049;
        private const int SC_PAD_MINUS     = 0x04A;
        private const int SC_PAD_4         = 0x04B;
        private const int SC_PAD_5         = 0x04C;
        private const int SC_PAD_6         = 0x04D;
        private const int SC_PAD_PLUS      = 0x04E;
        private const int SC_PAD_1         = 0x04F;
        private const int SC_PAD_2         = 0x050;
        private const int SC_PAD_3         = 0x051;
        private const int SC_PAD_0         = 0x052;
        private const int SC_PAD_DOT       = 0x053;
        private const int SC_NUBS          = 0x056;
        private const int SC_F11           = 0x057;
        private const int SC_F12           = 0x058;
        private const int SC_F13           = 0x064;
        private const int SC_F14           = 0x065;
        private const int SC_F15           = 0x066;
        private const int SC_F16           = 0x067;
        private const int SC_F17           = 0x068;
        private const int SC_F18           = 0x069;
        private const int SC_F19           = 0x06A;
        private const int SC_F20           = 0x06B;
        private const int SC_F21           = 0x06C;
        private const int SC_F22           = 0x06D;
        private const int SC_F23           = 0x06E;
        private const int SC_F24           = 0x076;
        private const int SC_PAD_ENTER     = 0x11C;
        private const int SC_RIGHT_CONTROL = 0x11D;
        private const int SC_PAD_SLASH     = 0x135;
        private const int SC_PRINT_SCREEN  = 0x137;
        private const int SC_RIGHT_ALT     = 0x138;
        private const int SC_NUM_LOCK      = 0x145;
        private const int SC_HOME          = 0x147;
        private const int SC_UP            = 0x148;
        private const int SC_PAGE_UP       = 0x149;
        private const int SC_LEFT          = 0x14B;
        private const int SC_RIGHT         = 0x14D;
        private const int SC_END           = 0x14F;
        private const int SC_DOWN          = 0x150;
        private const int SC_PAGE_DOWN     = 0x151;
        private const int SC_INSERT        = 0x152;
        private const int SC_DELETE        = 0x153;
        private const int SC_LEFT_GUI      = 0x15B;
        private const int SC_RIGHT_GUI     = 0x15C;
        private const int SC_MENU          = 0x15D;

        private static KeyTesterWindow instance = null;

        public KeyTesterWindow()
        {
            InitializeComponent();
        }

        public static KeyTesterWindow GetInstance()
        {
            if (instance == null)
            {
                instance = new KeyTesterWindow();
                instance.FormClosed += delegate { instance = null; };
            }

            return instance;
        }

        private void KeyTesterWindow_Load(object sender, EventArgs e)
        {
            CenterToParent();
        }

        protected override bool ProcessKeyMessage(ref Message m)
        {
            // The KeyDown and KeyUp events do not provide access to the virtual key and scancode
            // Scancode seems more precise than virtual key, it allows for distinguishing between left and right mods, for example
            if (m.Msg == WM_KEYDOWN || m.Msg == WM_KEYUP || m.Msg == WM_SYSKEYDOWN || m.Msg == WM_SYSKEYUP)
            {
                int vKey = m.WParam.ToInt32();
                int scanCode = (int)((m.LParam.ToInt64() >> 16) & 0x1FF);
                KeyControl pressedKeyControl = GetKeyControlForKey(vKey, scanCode);

                if (pressedKeyControl != null)
                {
                    pressedKeyControl.Pressed = m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN;
                    pressedKeyControl.Tested = true;
                }

                lblLastVirtualKey.Text = $"Last VK: {vKey:X2}";
                lblLastScanCode.Text = $"Last SC: {scanCode:X2}";

                return true;
            }

            return base.ProcessKeyMessage(ref m);
        }

        private KeyControl GetKeyControlForKey(int vKey, int scanCode)
        {
            // If scancode is 0 but marked as extended, switch on VK instead as it's most likely a media key that won't change with keyboard layout anyway
            if (scanCode == 0x100)
            {
                switch (vKey)
                {
                    case VK_VOLUME_MUTE:
                        return keyMute;
                    case VK_VOLUME_DOWN:
                        return keyVolumeDown;
                    case VK_VOLUME_UP:
                        return keyVolumeUp;
                    case VK_MEDIA_PLAY_PAUSE:
                        return keyPlayPause;
                    case VK_MEDIA_PREV_TRACK:
                        return keyMediaPrevious;
                    case VK_MEDIA_NEXT_TRACK:
                        return keyMediaNext;
                    case VK_LAUNCH_MAIL:
                        return keyMail;
                    case VK_LAUNCH_APP2:
                        return keyCalculator;
                }
            }
            else
            {
                switch (scanCode)
                {
                    case SC_F1:
                        return keyF1;
                    case SC_F2:
                        return keyF2;
                    case SC_F3:
                        return keyF3;
                    case SC_F4:
                        return keyF4;
                    case SC_F5:
                        return keyF5;
                    case SC_F6:
                        return keyF6;
                    case SC_F7:
                        return keyF7;
                    case SC_F8:
                        return keyF8;
                    case SC_F9:
                        return keyF9;
                    case SC_F10:
                        return keyF10;
                    case SC_F11:
                        return keyF11;
                    case SC_F12:
                        return keyF12;
                    case SC_F13:
                        return keyF13;
                    case SC_F14:
                        return keyF14;
                    case SC_F15:
                        return keyF15;
                    case SC_F16:
                        return keyF16;
                    case SC_F17:
                        return keyF17;
                    case SC_F18:
                        return keyF18;
                    case SC_F19:
                        return keyF19;
                    case SC_F20:
                        return keyF20;
                    case SC_F21:
                        return keyF21;
                    case SC_F22:
                        return keyF22;
                    case SC_F23:
                        return keyF23;
                    case SC_F24:
                        return keyF24;
                    case SC_1:
                        return key1;
                    case SC_2:
                        return key2;
                    case SC_3:
                        return key3;
                    case SC_4:
                        return key4;
                    case SC_5:
                        return key5;
                    case SC_6:
                        return key6;
                    case SC_7:
                        return key7;
                    case SC_8:
                        return key8;
                    case SC_9:
                        return key9;
                    case SC_0:
                        return key0;
                    case SC_Q:
                        return keyQ;
                    case SC_W:
                        return keyW;
                    case SC_E:
                        return keyE;
                    case SC_R:
                        return keyR;
                    case SC_T:
                        return keyT;
                    case SC_Y:
                        return keyY;
                    case SC_U:
                        return keyU;
                    case SC_I:
                        return keyI;
                    case SC_O:
                        return keyO;
                    case SC_P:
                        return keyP;
                    case SC_A:
                        return keyA;
                    case SC_S:
                        return keyS;
                    case SC_D:
                        return keyD;
                    case SC_F:
                        return keyF;
                    case SC_G:
                        return keyG;
                    case SC_H:
                        return keyH;
                    case SC_J:
                        return keyJ;
                    case SC_K:
                        return keyK;
                    case SC_L:
                        return keyL;
                    case SC_Z:
                        return keyZ;
                    case SC_X:
                        return keyX;
                    case SC_C:
                        return keyC;
                    case SC_V:
                        return keyV;
                    case SC_B:
                        return keyB;
                    case SC_N:
                        return keyN;
                    case SC_M:
                        return keyM;
                    case SC_ESCAPE:
                        return keyEscape;
                    case SC_GRAVE:
                        return keyGrave;
                    case SC_MINUS:
                        return keyMinus;
                    case SC_EQUAL:
                        return keyEqual;
                    case SC_BACKSPACE:
                        return keyBackspace;
                    case SC_TAB:
                        return keyTab;
                    case SC_LEFT_BRACE:
                        return keyLeftBrace;
                    case SC_RIGHT_BRACE:
                        return keyRightBrace;
                    case SC_BACKSLASH:
                        // ANSI backslash and ISO NUHS are mapped to the same scancode, but the VK for the latter will be something other than OEM 5
                        return vKey == VK_OEM_5 ? keyBackslash : keyNUHS;
                    case SC_CAPS_LOCK:
                        return keyCapsLock;
                    case SC_SEMICOLON:
                        return keySemicolon;
                    case SC_QUOTE:
                        return keyQuote;
                    case SC_ENTER:
                        return keyEnter;
                    case SC_NUBS:
                        return keyNUBS;
                    case SC_COMMA:
                        return keyComma;
                    case SC_DOT:
                        return keyDot;
                    case SC_SLASH:
                        return keySlash;
                    case SC_SPACE:
                        return keySpace;
                    case SC_MENU:
                        return keyMenu;
                    case SC_LEFT_CONTROL:
                        return keyLeftControl;
                    case SC_LEFT_SHIFT:
                        return keyLeftShift;
                    case SC_LEFT_ALT:
                        return keyLeftAlt;
                    case SC_LEFT_GUI:
                        return keyLeftGUI;
                    case SC_RIGHT_CONTROL:
                        return keyRightControl;
                    case SC_RIGHT_SHIFT:
                        return keyRightShift;
                    case SC_RIGHT_ALT:
                        return keyRightAlt;
                    case SC_RIGHT_GUI:
                        return keyRightGUI;
                    case SC_PRINT_SCREEN:
                        return keyPrintScreen;
                    case SC_SCROLL_LOCK:
                        return keyScrollLock;
                    case SC_PAUSE:
                        return keyPauseBreak;
                    case SC_INSERT:
                        return keyInsert;
                    case SC_HOME:
                        return keyHome;
                    case SC_PAGE_UP:
                        return keyPageUp;
                    case SC_DELETE:
                        return keyDelete;
                    case SC_END:
                        return keyEnd;
                    case SC_PAGE_DOWN:
                        return keyPageDown;
                    case SC_UP:
                        return keyUp;
                    case SC_LEFT:
                        return keyLeft;
                    case SC_DOWN:
                        return keyDown;
                    case SC_RIGHT:
                        return keyRight;
                    case SC_NUM_LOCK:
                        return keyNumLock;
                    case SC_PAD_SLASH:
                        return keyPadSlash;
                    case SC_PAD_ASTERISK:
                        return keyPadAsterisk;
                    case SC_PAD_MINUS:
                        return keyPadMinus;
                    case SC_PAD_PLUS:
                        return keyPadPlus;
                    case SC_PAD_ENTER:
                        return keyPadEnter;
                    case SC_PAD_DOT:
                        return keyPadDot;
                    case SC_PAD_0:
                        return keyPad0;
                    case SC_PAD_1:
                        return keyPad1;
                    case SC_PAD_2:
                        return keyPad2;
                    case SC_PAD_3:
                        return keyPad3;
                    case SC_PAD_4:
                        return keyPad4;
                    case SC_PAD_5:
                        return keyPad5;
                    case SC_PAD_6:
                        return keyPad6;
                    case SC_PAD_7:
                        return keyPad7;
                    case SC_PAD_8:
                        return keyPad8;
                    case SC_PAD_9:
                        return keyPad9;
                }
            }

            return null;
        }
    }
}
