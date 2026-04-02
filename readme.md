# QMK Toolbox

[![Latest Release](https://img.shields.io/github/v/release/qmk/qmk_toolbox?color=3D87CE&label=Latest&sort=semver&style=for-the-badge)](https://github.com/qmk/qmk_toolbox/releases/latest)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/qmk/qmk_toolbox/build.yml?logo=github&style=for-the-badge)](https://github.com/qmk/qmk_toolbox/actions?query=workflow%3ACI+branch%3Amaster)
[![Discord](https://img.shields.io/discord/440868230475677696.svg?logo=discord&logoColor=white&color=7289DA&style=for-the-badge)](https://discord.gg/qmk)

This is a collection of flashing tools packaged into one app. It supports auto-detection and auto-flashing of firmware to keyboards.

|Windows|macOS|Linux|
|-------|-----|-----|
|[![Windows](https://i.imgur.com/jHaX9bV.png)](https://i.imgur.com/jHaX9bV.png)|[![macOS](https://i.imgur.com/8hZEfDD.png)](https://i.imgur.com/8hZEfDD.png)|![Linux](https://i.imgur.com/8hZEfDD.png)|

## Flashing

QMK Toolbox supports the following bootloaders:

 - ARM DFU (APM32, AT32, Kiibohd, STM32, STM32duino) via [dfu-util](http://dfu-util.sourceforge.net/)
 - Atmel SAM-BA (Massdrop) via [Massdrop Loader](https://github.com/massdrop/mdloader)
 - Atmel/LUFA/QMK DFU via [dfu-programmer](http://dfu-programmer.github.io/)
 - BootloadHID (Atmel, PS2AVRGB) via [bootloadHID](https://www.obdev.at/products/vusb/bootloadhid.html)
 - Caterina (Arduino, Pro Micro) via [avrdude](http://nongnu.org/avrdude/)
 - HalfKay (Teensy, Ergodox EZ) via [Teensy Loader](https://pjrc.com/teensy/loader_cli.html)
 - LUFA Mass Storage
 - LUFA/QMK HID via [hid_bootloader_cli](https://github.com/abcminiuser/lufa)
 - Raspberry Pi RP2040/RP2350 (BOOTSEL) via [picotool](https://github.com/raspberrypi/picotool)
 - RISC-V DFU (GD32V) via [dfu-util](http://dfu-util.sourceforge.net/)
 - WB32 DFU via [wb32-dfu-updater_cli](https://github.com/WestberryTech/wb32-dfu-updater)

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

## Installation

### System Requirements

* Windows 10 May 2020 Update (20H1) or higher
* macOS 13 (Ventura) or higher, both Apple Silicon and Intel supported
* Linux (x86_64, aarch64/arm64)

### Dependencies

On Windows, QMK Toolbox will prompt at first run to install the necessary drivers.

If you run into any issues with "Device not found" when flashing, you may need to use [Zadig](https://docs.qmk.fm/#/driver_installation_zadig) to fix the issue.

On Linux, `libudev` is required for USB hotplug support (`libudev-dev` / `libudev1`). This is present by default on most desktop distributions.

### Download

* **Windows x64:** [qmk_toolbox_install.exe](https://github.com/qmk/qmk_toolbox/releases/latest/download/qmk_toolbox_install.exe)
* **macOS (Universal):** [QMK Toolbox.pkg](https://github.com/qmk/qmk_toolbox/releases/latest/download/QMK%20Toolbox.pkg)
* **Linux (x86_64):** [qmk_toolbox-linux-x64](https://github.com/qmk/qmk_toolbox/releases/latest/download/qmk_toolbox-linux-x64)
* **Linux (aarch64/arm64):** [qmk_toolbox-linux-arm64](https://github.com/qmk/qmk_toolbox/releases/latest/download/qmk_toolbox-linux-arm64)

### Building from source

All scripts require Docker. The full build sequence is:

```sh
# 1. Download flash tool binaries, hidapi, and udev resources for all platforms
scripts/fetch-tools.sh

# 2. Compile and publish self-contained executables for all targets
scripts/publish-all.sh

# 3. Assemble release artifacts (calls make-macos-app.sh, make-win-installer.sh, make-macos-pkg.sh internally)
#    Outputs: artifacts/qmk_toolbox-linux-x64, qmk_toolbox-linux-arm64,
#             qmk_toolbox.exe, qmk_toolbox_install.exe,
#             QMK Toolbox.app.zip, QMK Toolbox.dmg, QMK Toolbox.pkg
scripts/make-release-artifacts.sh
```

To build for a single target only, pass the RID to `publish-all.sh`:

```sh
scripts/publish-all.sh linux-x64
```

Alternatively, build manually without Docker. Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```sh
dotnet tool restore && dotnet husky install
dotnet build src/QmkToolbox.slnx
dotnet publish src/QmkToolbox.Desktop/QmkToolbox.Desktop.csproj -c Release -r <rid> --self-contained true -p:PublishSingleFile=true
```

Where `<rid>` is `win-x64`, `osx-arm64`, `osx-x64`, `linux-x64`, or `linux-arm64`.

### Updating NuGet dependencies

```sh
# Check for outdated packages
scripts/check-deps.sh

# Upgrade Directory.Packages.props in place
scripts/check-deps.sh --upgrade
```
