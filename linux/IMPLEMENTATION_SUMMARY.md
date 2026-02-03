# QMK Toolbox Linux Port - Implementation Summary

## Status: ✅ Complete

The Linux port of QMK Toolbox is now **fully functional** with all major features implemented.

## Completed Features

### ✅ Core Functionality
- [x] **USB Device Detection** - Detects bootloader devices via pyudev
- [x] **Firmware Flashing** - Flash .hex and .bin files to keyboards
- [x] **Device Reset** - Exit DFU mode and return to normal operation
- [x] **EEPROM Operations** - Clear EEPROM settings
- [x] **Auto-Flash Mode** - Automatically flash when bootloader detected

### ✅ User Interface (Qt/PySide6)
- [x] **Main Window** - File selection, MCU selector, flash/reset buttons
- [x] **Log Widget** - Color-coded output (commands, errors, bootloader messages)
- [x] **Key Tester** - Visual keyboard testing tool
- [x] **HID Console** - Debug output from keyboards with CONSOLE_ENABLE
- [x] **Settings Persistence** - Saves file history, auto-flash state, MCU selection

### ✅ Bootloader Support

Implemented bootloaders with full flashing support:
- **Atmel DFU** - via dfu-programmer (atmega32u4, etc.)
- **STM32 DFU** - via dfu-util (STM32, ARM keyboards)
- **APM32 DFU** - via dfu-util (APM32 chips)
- **Caterina** - via avrdude (Pro Micro, Arduino Leonardo)
- **HalfKay** - via teensy-loader-cli (Teensy boards)

Stub implementations (for future completion):
- Kiibohd DFU, STM32duino, GD32V DFU, WB32 DFU
- BootloadHID, LUFA HID, LUFA Mass Storage
- Atmel SAM-BA, ISP

### ✅ Linux-Specific Features
- **udev Rules** - USB device access without root
- **HID Device Probing** - Works around Linux hidapi limitations
- **Async Process Execution** - Non-blocking flash operations
- **Command-line Tool Integration** - Uses system-installed flashers

### ✅ Documentation
- [x] **README.md** - Installation and quick start
- [x] **FLASHING_GUIDE.md** - Comprehensive flashing instructions
- [x] **HID_CONSOLE_SETUP.md** - HID console troubleshooting
- [x] **INSTALL.md** - Detailed installation guide

### ✅ Development Tools
- [x] **Virtual Environment Setup** - Python venv with all dependencies
- [x] **Installation Scripts** - `install_hid_support.sh`, `activate.sh`
- [x] **Test Scripts** - `test_hid.sh`, `test_flashing.py`
- [x] **Packaging Scaffolding** - Debian, Arch, AppImage, RPM

## Architecture

```
linux/
├── src/qmk_toolbox/
│   ├── bootloader/          # Bootloader device implementations
│   │   ├── bootloader_device.py      # Base class with async flash()
│   │   ├── bootloader_factory.py     # Device detection and creation
│   │   ├── atmel_dfu_device.py       # dfu-programmer
│   │   ├── stm32_dfu_device.py       # dfu-util for STM32
│   │   ├── caterina_device.py        # avrdude
│   │   └── halfkay_device.py         # teensy-loader-cli
│   ├── usb/                 # USB device detection
│   │   ├── usb_listener.py           # pyudev-based USB monitoring
│   │   └── usb_device.py             # USB device representation
│   ├── hid/                 # HID console support
│   │   ├── hid_listener.py           # hidapi-based HID monitoring
│   │   └── (device probing for Linux)
│   ├── ui/                  # Qt GUI
│   │   ├── main_window.py            # Main application window
│   │   ├── log_widget.py             # Color-coded log display
│   │   ├── key_tester_window.py      # Keyboard tester
│   │   ├── key_widget.py             # Individual key display
│   │   └── hid_console_window.py     # HID console viewer
│   ├── window_state.py      # UI state management
│   ├── message_type.py      # Log message types
│   └── main.py              # Application entry point
├── packaging/               # Distribution packages
│   ├── 99-qmk.rules         # udev rules (bootloaders + HID)
│   ├── deb/                 # Debian package
│   ├── arch/                # Arch Linux AUR
│   ├── appimage/            # AppImage
│   └── rpm/                 # RPM package
├── resources/               # Icons, desktop files
├── venv/                    # Python virtual environment
├── pyproject.toml           # Python package metadata
├── setup.py                 # Legacy setuptools config
├── README.md
├── FLASHING_GUIDE.md
├── HID_CONSOLE_SETUP.md
└── INSTALL.md
```

## Key Implementation Details

### Flashing Process

1. **User Action** → `flash_firmware()` slot triggered
2. **Async Execution** → `asyncio.create_task(_flash_firmware_async())`
3. **Find Bootloaders** → Query USB listener for bootloader devices
4. **Disable UI** → Prevent user interaction during flash
5. **Execute Flash** → `bootloader.flash(mcu, file_path)` calls CLI tool
6. **Stream Output** → Real-time command output to log widget
7. **Re-enable UI** → Restore button states

### Auto-Flash Workflow

1. **Enable Auto-Flash** → Disables UI buttons
2. **Wait for Bootloader** → USB listener monitors for devices
3. **Auto-Trigger** → `on_bootloader_device_connected()` calls `flash_firmware()`
4. **Flash Automatically** → Same process as manual flash
5. **UI Stays Disabled** → Until auto-flash is turned off

### HID Console (Linux-specific)

**Problem**: Linux hidapi doesn't report `usage_page`/`usage` fields.

**Solution**: Implemented device probing fallback:
1. Try usage_page/usage filtering (works on some systems)
2. If no devices found, probe all HID interfaces > 0
3. Attempt to open each device to verify accessibility
4. Add accessible devices to console device list

## Testing

### Without Hardware
```bash
cd linux
source venv/bin/activate
python test_flashing.py        # Verify implementation
qmk-toolbox                     # Launch GUI
```

### With Hardware
1. Install udev rules: `./install_hid_support.sh`
2. Install flashing tools: `sudo apt install dfu-util dfu-programmer avrdude`
3. Launch QMK Toolbox
4. Select firmware file
5. Put keyboard in bootloader mode
6. Click "Flash"

## Known Limitations

1. **Stub Bootloaders** - Some bootloader types have placeholder implementations
2. **CLI Tool Dependencies** - Requires system-installed command-line tools
3. **HID Usage Detection** - May not work on all Linux distributions
4. **No Built-in Flashers** - Unlike Windows, doesn't bundle flashing tools

## Future Enhancements

### Priority 1 (High Value)
- [ ] Complete stub bootloader implementations
- [ ] Add unit tests (pytest + pytest-qt)
- [ ] CI/CD integration (GitHub Actions)
- [ ] Package publishing (PyPI, AUR, PPA)

### Priority 2 (Nice to Have)
- [ ] Drag-and-drop file support
- [ ] Firmware file validation
- [ ] Flash history tracking
- [ ] QMK compile integration
- [ ] Split keyboard handedness helper

### Priority 3 (Advanced)
- [ ] ISP flashing support
- [ ] Firmware backup/restore
- [ ] Bootloader installation
- [ ] Custom bootloader profiles

## Performance Notes

- **Async I/O** - Non-blocking flash operations using asyncio
- **Background Monitoring** - USB/HID listeners run in separate threads
- **Lazy Loading** - Windows created on-demand (Key Tester, HID Console)
- **Minimal Dependencies** - Only essential packages (PySide6, pyudev, hidapi)

## Comparison with Windows/macOS Versions

| Feature | Windows | macOS | Linux |
|---------|---------|-------|-------|
| GUI Framework | WinForms | Cocoa (Swift) | Qt (PySide6) |
| USB Detection | WMI | IOKit | pyudev |
| HID Console | HidLibrary | IOHIDManager | hidapi + probing |
| Flasher Tools | Bundled | Bundled | System-installed |
| Auto-Flash | ✅ | ✅ | ✅ |
| Key Tester | ✅ | ✅ | ✅ |
| EEPROM Clear | ✅ | ✅ | ✅ |

## File Statistics

```
Total Files: 50+
Total Lines of Code: ~3000+
Languages: Python (100%)
Dependencies: 4 (PySide6, pyudev, hidapi, pyusb)
```

## Contributors

Linux port created as part of QMK Toolbox cross-platform initiative.

Original Windows/macOS versions by QMK community.

## License

MIT License - see LICENSE.md

## References

- [QMK Firmware](https://qmk.fm/)
- [QMK Toolbox (original)](https://github.com/qmk/qmk_toolbox)
- [PySide6 Documentation](https://doc.qt.io/qtforpython/)
- [pyudev Documentation](https://pyudev.readthedocs.io/)
