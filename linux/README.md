# QMK Toolbox - Linux

QMK Toolbox for Linux - A keyboard firmware flashing utility with Qt GUI.

## System Requirements

* Linux (kernel 4.4+)
* Python 3.8 or higher
* Qt 6.6 or higher (via PySide6)

## Dependencies

### Runtime Dependencies

The following command-line tools need to be installed on your system:

* `dfu-util` - For ARM/RISC-V DFU bootloaders
* `dfu-programmer` - For Atmel/LUFA/QMK DFU bootloaders
* `avrdude` - For Caterina (Pro Micro) bootloaders
* `teensy-loader-cli` - For HalfKay (Teensy) bootloaders

### Install Dependencies

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

### Python Dependencies

Install from PyPI:
```bash
pip install qmk-toolbox
```

Or install from source:
```bash
cd linux
pip install -e .
```

## udev Rules

To access USB devices without root permissions, install the udev rules:

**Quick Install:**
```bash
./install_hid_support.sh
```

**Manual Install:**
```bash
sudo cp packaging/99-qmk.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

You may need to unplug/replug devices or log out and back in for the changes to take effect.

**Note:** The udev rules provide access to both bootloader devices (for flashing) and HID devices (for the HID Console feature).

## Running

After installation, run:

```bash
qmk-toolbox
```

Or from source:
```bash
cd linux/src
python -m qmk_toolbox.main
```

## Usage

### Flashing Firmware

See [FLASHING_GUIDE.md](FLASHING_GUIDE.md) for detailed instructions on:
- Flashing firmware files (.hex/.bin)
- Auto-flash mode
- Resetting devices (Exit DFU)
- Clearing EEPROM
- Troubleshooting flashing issues

**Quick Start:**
1. Install udev rules (see above)
2. Install flashing tools for your bootloader type
3. Open QMK Toolbox
4. Click "Open" and select firmware file
5. Put keyboard in bootloader mode
6. Click "Flash"

## Building Packages

### AppImage

```bash
cd packaging/appimage
./build.sh
```

### Debian Package

```bash
cd packaging/deb
./build.sh
```

### Arch Linux Package

```bash
cd packaging/arch
makepkg -si
```

### RPM Package

```bash
cd packaging/rpm
rpmbuild -ba qmk-toolbox.spec
```

## Supported Bootloaders

- ARM DFU (APM32, Kiibohd, STM32, STM32duino) via dfu-util
- RISC-V DFU (GD32V) via dfu-util
- Atmel/LUFA/QMK DFU via dfu-programmer
- Atmel SAM-BA (Massdrop) via mdloader
- BootloadHID (Atmel, PS2AVRGB) via bootloadHID
- Caterina (Arduino, Pro Micro) via avrdude
- HalfKay (Teensy, Ergodox EZ) via teensy-loader-cli
- LUFA/QMK HID via hid_bootloader_cli
- WB32 DFU via wb32-dfu-updater_cli
- LUFA Mass Storage

## HID Console

The HID Console listens for messages on USB HID usage page `0xFF31` and usage `0x0074`, compatible with PJRC's `hid_listen`.

If your keyboard has `CONSOLE_ENABLE = yes` in `rules.mk`, you can print debug messages with `xprintf()` or `uprintf()`.

**Requirements:**
- QMK firmware (not proprietary firmware)
- `CONSOLE_ENABLE = yes` in keyboard's `rules.mk`
- udev rules installed (see above)

**Troubleshooting:**
If the HID Console doesn't detect devices, see [HID_CONSOLE_SETUP.md](HID_CONSOLE_SETUP.md) for detailed setup and troubleshooting instructions.

## License

MIT License - see LICENSE.md for details.
