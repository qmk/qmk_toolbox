# HID Console Setup Guide

## Problem
The HID Console window opens but doesn't detect any devices because:
1. Linux requires special permissions to access HID devices (`/dev/hidraw*`)
2. On Linux, `hidapi` doesn't reliably report usage_page/usage fields

## Solution
We've implemented two fixes:

### Fix 1: Updated udev Rules
Added a rule to grant user access to all hidraw devices in `packaging/99-qmk.rules`:
```
KERNEL=="hidraw*", SUBSYSTEM=="hidraw", MODE="0666", TAG+="uaccess"
```

### Fix 2: Enhanced HID Listener
Updated `src/qmk_toolbox/hid/hid_listener.py` to:
- Try usage_page/usage filtering first (works on some Linux systems)
- Fall back to probing devices by attempting to open them
- Skip interface 0 (typically keyboard/mouse)
- Test accessibility of higher interfaces (where console devices live)

## Installation Steps

### 1. Install udev Rules (Requires sudo)
```bash
cd /home/amin/qmk/qmk_toolbox/linux
sudo cp packaging/99-qmk.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

### 2. Apply Permissions
The udev rules will apply to new devices immediately, but for existing devices you need to:
- **Option A**: Unplug and replug your keyboard
- **Option B**: Log out and log back in
- **Option C**: Reboot your system

### 3. Verify Installation
```bash
cd /home/amin/qmk/qmk_toolbox/linux
./test_hid.sh
```

Expected output when working:
```
✓ udev rules are installed
✓ HID device access is working!
```

### 4. Run QMK Toolbox
```bash
source venv/bin/activate
qmk-toolbox
```

Then open: **Tools → HID Console**

## Important Notes

### Does Your Keyboard Support HID Console?

The HID Console feature requires:
1. **QMK firmware** (not proprietary firmware)
2. **CONSOLE_ENABLE = yes** in the keyboard's `rules.mk`
3. Firmware compiled with console support

**Your NuPhy Air75 V2** likely runs proprietary firmware by default and won't show up in the HID Console unless you've flashed it with custom QMK firmware.

### Testing with a QMK Keyboard

To test HID Console functionality:

1. **Use a keyboard running QMK** (or flash your keyboard with QMK)

2. **Enable console in your keymap's rules.mk:**
   ```make
   CONSOLE_ENABLE = yes
   ```

3. **Add debug output in your keymap.c:**
   ```c
   #include "print.h"
   
   bool process_record_user(uint16_t keycode, keyrecord_t *record) {
       if (record->event.pressed) {
           uprintf("Key pressed: 0x%04X\n", keycode);
       }
       return true;
   }
   ```

4. **Flash the firmware** and connect the keyboard

5. **Open QMK Toolbox → Tools → HID Console**

6. **Press keys** - you should see debug output appear

## Troubleshooting

### No devices appear in HID Console
- Check if udev rules are installed: `ls -la /etc/udev/rules.d/99-qmk.rules`
- Check hidraw permissions: `ls -la /dev/hidraw*` (should show `rw-rw----` or `rw-rw-rw-`)
- Verify your keyboard has QMK with CONSOLE_ENABLE
- Try unplugging and replugging the keyboard

### "open failed" errors when testing
- udev rules not applied yet - try logout/login or reboot
- User not in correct group - udev rules should handle this with TAG+="uaccess"

### Devices show but no console output
- Keyboard doesn't have CONSOLE_ENABLE
- Firmware not compiled with console support
- Need to add `xprintf()` or `uprintf()` calls in keymap code

## Alternative: Run as Root (NOT RECOMMENDED)
For testing only, you can run QMK Toolbox as root:
```bash
sudo ./venv/bin/qmk-toolbox
```

This bypasses permission issues but is not a secure long-term solution.

## References
- [QMK Debugging](https://docs.qmk.fm/#/debugging)
- [QMK Console](https://docs.qmk.fm/#/newbs_flashing?id=debugging)
- [PJRC hid_listen](https://www.pjrc.com/teensy/hid_listen.html) - Compatible HID console protocol
