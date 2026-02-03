# QMK Toolbox

[![Latest Release](https://img.shields.io/github/v/release/Aghabeiki/qmk_toolbox?color=3D87CE&label=Latest&sort=semver&style=for-the-badge)](https://github.com/Aghabeiki/qmk_toolbox/releases/latest)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aghabeiki/qmk_toolbox/build.yml?logo=github&style=for-the-badge)](https://github.com/Aghabeiki/qmk_toolbox/actions?query=workflow%3ACI+branch%3Amaster)
[![Discord](https://img.shields.io/discord/440868230475677696.svg?logo=discord&logoColor=white&color=7289DA&style=for-the-badge)](https://discord.gg/qmk)

This is a collection of flashing tools packaged into one app. It supports auto-detection and auto-flashing of firmware to keyboards.

**Now with full Linux support!**

|Windows|macOS|Linux|
|-------|-----|-----|
|[![Windows](https://i.imgur.com/jHaX9bV.png)](https://i.imgur.com/jHaX9bV.png)|[![macOS](https://i.imgur.com/8hZEfDD.png)](https://i.imgur.com/8hZEfDD.png)|Python/Qt GUI|

## Flashing

QMK Toolbox supports the following bootloaders:

 - ARM DFU (APM32, Kiibohd, STM32, STM32duino) via [dfu-util](http://dfu-util.sourceforge.net/)
 - Atmel/LUFA/QMK DFU via [dfu-programmer](http://dfu-programmer.github.io/)
 - Atmel SAM-BA (Massdrop) via [Massdrop Loader](https://github.com/massdrop/mdloader)
 - BootloadHID (Atmel, PS2AVRGB) via [bootloadHID](https://www.obdev.at/products/vusb/bootloadhid.html)
 - Caterina (Arduino, Pro Micro) via [avrdude](http://nongnu.org/avrdude/)
 - HalfKay (Teensy, Ergodox EZ) via [Teensy Loader](https://pjrc.com/teensy/loader_cli.html)
 - LUFA/QMK HID via [hid_bootloader_cli](https://github.com/abcminiuser/lufa)
 - WB32 DFU (WB32) via [wb32-dfu-updater_cli](https://github.com/WestberryTech/wb32-dfu-updater)
 - LUFA Mass Storage

And the following ISP flashers:

 - AVRISP (Arduino ISP)
 - USBasp (AVR ISP)
 - USBTiny (AVR Pocket)

If there's an interest in any others, they can be added if their commands are known.

## HID Console

The Toolbox also listens to HID messages on usage page `0xFF31` and usage `0x0074`, compatible with PJRC's [`hid_listen`](https://www.pjrc.com/teensy/hid_listen.html).

If you have `CONSOLE_ENABLE = yes` in your keyboard's `rules.mk`, you can print messages with `xprintf()`, useful for debugging:

![Hello world from Console](https://i.imgur.com/bY8l233.png)

See the [QMK Docs](https://docs.qmk.fm/#/newbs_testing_debugging?id=debugging) for more information.

## Linux Port

This fork includes a **complete Linux port** of QMK Toolbox built with Python 3 and PySide6 (Qt).

**Features:**
- ✅ Full bootloader support (DFU, Caterina, HalfKay, STM32, etc.)
- ✅ USB device detection with pyudev
- ✅ Async firmware flashing (non-blocking UI)
- ✅ Auto-flash mode
- ✅ HID Console with debug output
- ✅ Key Tester window
- ✅ EEPROM clear functionality
- ✅ Device reset support

**Architecture:**
- Uses system-installed CLI tools (dfu-util, avrdude, etc.)
- Real-time USB hotplug detection via udev
- Fully async I/O with asyncio
- Compatible with all Linux distributions

**Documentation:**
- [Installation Guide](linux/INSTALL.md)
- [Flashing Guide](linux/FLASHING_GUIDE.md)
- [HID Console Setup](linux/HID_CONSOLE_SETUP.md)
- [Implementation Summary](linux/IMPLEMENTATION_SUMMARY.md)

The Linux implementation is **architecturally identical** to the Windows and macOS versions, ensuring compatibility and feature parity across all platforms.

## Installation

### System Requirements

* **Windows:** Windows 10 May 2020 Update (20H1) or higher
* **macOS:** macOS 12 (Monterey) or higher
* **Linux:** Kernel 4.4+ with Python 3.8 or higher

### Download

The [current version](https://github.com/Aghabeiki/qmk_toolbox/releases) of QMK Toolbox is **0.3.3**.

#### Windows
* [Standalone](https://github.com/Aghabeiki/qmk_toolbox/releases/latest/download/qmk_toolbox.exe)
* [Installer](https://github.com/Aghabeiki/qmk_toolbox/releases/latest/download/qmk_toolbox_install.exe)

When using QMK Toolbox on Windows, it will prompt at first run to install the necessary drivers. If you run into "Device not found" issues, use [Zadig](https://docs.qmk.fm/#/driver_installation_zadig) to fix the issue.

#### macOS
* [Standalone](https://github.com/Aghabeiki/qmk_toolbox/releases/latest/download/QMK.Toolbox.app.zip)
* [Installer](https://github.com/Aghabeiki/qmk_toolbox/releases/latest/download/QMK.Toolbox.pkg)

For Homebrew users:
```sh
brew install qmk-toolbox
```

#### Linux

**Install from PyPI:**
```sh
pip install qmk-toolbox
```

**Install from source:**
```sh
cd linux
pip install -e .
```

**System dependencies** (flashing tools):
```sh
# Debian/Ubuntu
sudo apt install dfu-util dfu-programmer avrdude teensy-loader-cli

# Arch Linux
sudo pacman -S dfu-util dfu-programmer avrdude teensy-loader-cli

# Fedora
sudo dnf install dfu-util dfu-programmer avrdude teensy-loader-cli
```

**udev rules** (required for USB device access):
```sh
cd linux
./install_hid_support.sh
```

See [linux/README.md](linux/README.md) for detailed Linux installation instructions.
