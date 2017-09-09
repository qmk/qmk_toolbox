# QMK Toolbox

This is a collection of useful tools packaged into one app. This is a pretty recent developement, but is looking to replace the QMK Flasher.

# Flashing

Supporting following bootloaders:
 - DFU (Atmel, LUFA) via dfu-programmer (http://dfu-programmer.github.io/)
 - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)
 - Halfkay (Teensy, Ergodox EZ) via teensy_loader_cli (https://pjrc.com/teensy/loader_cli.html)
 - STM32 (ARM) via dfu-util (http://dfu-util.sourceforge.net/)
 
If there's an interest in any, more can be added if their commands are know.
 
# HID Listening
 
Also listens to HID ascii from usage page 0xFF31 (compatible with the hid_listen provided by PJRC) - connects automatically and to all sources available.

## How to download

A Windows and OSX version are available, and you can get [the latest release here](https://github.com/qmk/qmk_toolbox/releases).
