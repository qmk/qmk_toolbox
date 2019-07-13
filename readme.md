# QMK Toolbox

This is a collection of useful tools packaged into one app. This is a pretty recent development, but is looking to replace the QMK Flasher.

# Flashing

Supporting following bootloaders:
 - DFU (Atmel, LUFA) via dfu-programmer (http://dfu-programmer.github.io/)
 - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)
 - Halfkay (Teensy, Ergodox EZ) via teensy_loader_cli (https://pjrc.com/teensy/loader_cli.html)
 - STM32 (ARM) via dfu-util (http://dfu-util.sourceforge.net/)
 - BootloadHID (Atmel, ps2avrGB, CA66) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)

If there's an interest in any others, they can be added if their commands are known.

# HID Listening

Also listens to HID ascii from usage page 0xFF31 (compatible with the hid_listen provided by PJRC) - connects automatically and to all sources available.

# Installation

## Dependencies

When using the QMK Toolbox on Windows, please install the mandatory drivers first. You can get [the latest release here](https://github.com/qmk/qmk_driver_installer/releases).

## Download

A Windows and macOS version are available, and you can get [the latest release here](https://github.com/qmk/qmk_toolbox/releases).

For Homebrew users, it is also available as a Cask:

```
$ brew tap homebrew/cask-drivers
$ brew cask install qmk-toolbox
```
