# Flashing Guide for QMK Toolbox (Linux)

## Overview

QMK Toolbox supports flashing firmware to keyboards in bootloader mode. The Linux version uses command-line tools to communicate with bootloader devices.

## Prerequisites

### 1. Install Required Tools

The flashing commands depend on external tools. Install based on your bootloader type:

**Debian/Ubuntu:**
```bash
sudo apt install dfu-util dfu-programmer avrdude teensy-loader-cli
```

**Arch Linux:**
```bash
sudo pacman -S dfu-util dfu-programmer avrdude teensy-loader-cli
```

**Fedora:**
```bash
sudo dnf install dfu-util dfu-programmer avrdude teensy-loader-cli
```

### 2. Install udev Rules

```bash
cd /home/amin/qmk/qmk_toolbox/linux
sudo cp packaging/99-qmk.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

### 3. Set Up Python Environment

```bash
cd /home/amin/qmk/qmk_toolbox/linux
python3 -m venv venv
source venv/bin/activate
pip install -e .
```

## Flashing Methods

### Method 1: Manual Flash

1. **Select firmware file**
   - Click "Open" button or drag firmware file to window
   - Supported formats: `.hex`, `.bin`

2. **Select MCU** (AVR only)
   - Choose your keyboard's MCU from the dropdown
   - Default: `atmega32u4` (most common)
   - Only used for Atmel/LUFA/QMK DFU bootloaders

3. **Put keyboard in bootloader mode**
   - Press keyboard's reset button, or
   - Use QMK's bootloader keycode (usually holding Esc while plugging in)
   - Watch log for "device connected" message

4. **Click "Flash" button**
   - Wait for "Flash complete" message
   - Do NOT unplug keyboard during flashing

### Method 2: Auto-Flash

Auto-flash automatically flashes firmware when a bootloader device is detected.

1. **Select firmware file** (same as manual)

2. **Enable Auto-Flash**
   - Check "Auto-Flash" checkbox, or
   - Menu: Tools → Auto-Flash

3. **Put keyboard in bootloader mode**
   - Flashing starts automatically
   - UI buttons are disabled during auto-flash mode

4. **Disable Auto-Flash when done**
   - Uncheck "Auto-Flash" checkbox

## Supported Bootloaders

### DFU Bootloaders (dfu-util)
- **ARM DFU**: STM32, APM32, Kiibohd, STM32duino
- **RISC-V DFU**: GD32V, WB32

**Requirements:**
- `dfu-util` installed
- No MCU selection needed

**Example devices:**
- Drop keyboards (Massdrop)
- Most ARM-based keyboards
- STM32-based keyboards

### Atmel DFU (dfu-programmer)
- **Atmel/LUFA/QMK DFU**

**Requirements:**
- `dfu-programmer` installed
- **MCU must be selected** (e.g., `atmega32u4`)

**Example devices:**
- Many AVR-based QMK keyboards
- Keyboards with ATmega32U4 chip

### Caterina (avrdude)
- **Arduino bootloader**
- **Pro Micro based keyboards**

**Requirements:**
- `avrdude` installed
- MCU must be selected

**Example devices:**
- Pro Micro based builds
- Arduino Leonardo based keyboards

### HalfKay (teensy-loader-cli)
- **Teensy bootloader**

**Requirements:**
- `teensy-loader-cli` installed

**Example devices:**
- Teensy-based keyboards
- Ergodox EZ

### Other Bootloaders

**BootloadHID**: Requires `bootloadHID` (not commonly packaged)
**LUFA HID**: Requires `hid_bootloader_cli`
**LUFA Mass Storage**: Device appears as USB drive
**Atmel SAM-BA**: Requires `mdloader`

## Additional Features

### Reset Device (Exit DFU)

Exits bootloader mode and returns keyboard to normal operation.

1. Connect keyboard in bootloader mode
2. Click "Exit DFU" button
3. Keyboard reboots to normal mode

**Note**: Not all bootloaders support reset command.

### Clear EEPROM

Clears keyboard's EEPROM memory (settings, layers, etc.).

1. Connect keyboard in bootloader mode
2. Click "Clear EEPROM" button
3. Wait for "EEPROM clear complete" message

**Requirements:**
- EEPROM reset file exists: `/common/reset.eep`
- Bootloader supports EEPROM flashing (AVR bootloaders only)

## Troubleshooting

### "No bootloader devices connected"

**Problem**: Toolbox doesn't detect keyboard in bootloader mode.

**Solutions:**
1. Verify udev rules are installed (see Prerequisites)
2. Check if device appears: `lsusb` (look for DFU or bootloader device)
3. Try unplugging and replugging keyboard
4. Verify keyboard is in bootloader mode (look for different USB VID/PID)

### "Command not found: dfu-util" (or other tool)

**Problem**: Required flashing tool not installed.

**Solution**: Install the tool for your bootloader type (see Prerequisites).

### "Flash failed with exit code 1"

**Problem**: Flash operation failed.

**Common causes:**
1. **Wrong MCU selected** (AVR only) - Select correct MCU
2. **Wrong firmware file** - Verify firmware is for your keyboard
3. **Wrong file format** - Some bootloaders need `.hex`, others need `.bin`
4. **Permission issues** - Ensure udev rules are installed
5. **Corrupted firmware** - Recompile or redownload firmware

**Debug steps:**
1. Check log output for specific error message
2. Try flashing from command line manually
3. Verify firmware file is not corrupted

### "EEPROM clear failed"

**Problem**: EEPROM clearing failed.

**Solutions:**
1. Verify `/common/reset.eep` file exists
2. Check if bootloader supports EEPROM (AVR only)
3. Some bootloaders don't support EEPROM operations

### Flash hangs or freezes

**Problem**: Flash operation doesn't complete.

**Solutions:**
1. **DO NOT** unplug keyboard
2. Wait 30-60 seconds
3. If still frozen, close QMK Toolbox
4. Unplug keyboard, replug in bootloader mode
5. Try again

### Permission denied errors

**Problem**: Can't access USB device.

**Solutions:**
1. Install udev rules (see Prerequisites)
2. Logout and login (or reboot)
3. Verify user is in correct groups: `groups`
4. Check device permissions: `ls -la /dev/ttyACM*` or similar

## Testing Flashing

### Without Hardware

You can test the UI without a real bootloader device:

```bash
source venv/bin/activate
qmk-toolbox
```

- Select a firmware file
- UI will enable/disable buttons correctly
- No actual flashing will occur without bootloader device

### With Hardware

**Safe test procedure:**

1. **Backup current firmware** (if possible)
2. **Use test firmware** - Flash known-good QMK default firmware
3. **Start QMK Toolbox**
4. **Select firmware file**
5. **Put keyboard in bootloader mode**
6. **Watch log output** for errors
7. **Verify "Flash complete" message**
8. **Test keyboard** - Verify keys work after flashing

**Emergency recovery:**

If flash fails and keyboard doesn't work:
1. Don't panic - bootloader is usually intact
2. Put keyboard back in bootloader mode
3. Flash known-good firmware
4. If keyboard is bricked, may need ISP flashing (advanced)

## File Locations

- **Common files**: `/home/amin/qmk/qmk_toolbox/common/`
  - `reset.eep` - EEPROM clear file
  - `reset_left.eep` - Left-hand EEPROM for split keyboards
  - `reset_right.eep` - Right-hand EEPROM for split keyboards
  - `avrdude.conf` - avrdude configuration

## Command-Line Equivalents

QMK Toolbox runs these commands internally:

**DFU (ARM/RISC-V):**
```bash
dfu-util -D firmware.bin
```

**Atmel DFU:**
```bash
dfu-programmer atmega32u4 erase
dfu-programmer atmega32u4 flash firmware.hex
dfu-programmer atmega32u4 reset
```

**Caterina:**
```bash
avrdude -p atmega32u4 -c avr109 -P /dev/ttyACM0 -U flash:w:firmware.hex:i
```

**HalfKay:**
```bash
teensy-loader-cli -mmcu=atmega32u4 -w firmware.hex
```

## References

- [QMK Flashing Guide](https://docs.qmk.fm/#/newbs_flashing)
- [QMK Bootloaders](https://docs.qmk.fm/#/flashing)
- [dfu-util documentation](http://dfu-util.sourceforge.net/)
- [dfu-programmer](http://dfu-programmer.github.io/)
- [avrdude documentation](http://nongnu.org/avrdude/)
