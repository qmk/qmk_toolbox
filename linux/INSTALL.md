# Installation Instructions - QMK Toolbox Linux

## Quick Start

### From Source

```bash
cd linux
pip install -e .
qmk-toolbox
```

### System Requirements

- Python 3.8 or higher
- PySide6 (Qt 6.6+)
- pyudev
- hidapi
- pyusb

## Installing Dependencies

### Ubuntu/Debian

```bash
# Install system dependencies
sudo apt update
sudo apt install python3 python3-pip python3-venv
sudo apt install dfu-util dfu-programmer avrdude teensy-loader-cli

# Install Python package
pip install qmk-toolbox

# Install udev rules
sudo cp packaging/99-qmk.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger

# Add your user to necessary groups
sudo usermod -a -G plugdev $USER

# Log out and back in for group changes to take effect
```

### Arch Linux

```bash
# Install from AUR (when available)
yay -S qmk-toolbox

# Or from source
cd linux/packaging/arch
makepkg -si
```

### Fedora

```bash
# Install system dependencies
sudo dnf install python3 python3-pip
sudo dnf install dfu-util dfu-programmer avrdude teensy-loader-cli

# Install Python package
pip install qmk-toolbox

# Install udev rules
sudo cp packaging/99-qmk.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

## Building Packages

### Debian Package

```bash
cd linux/packaging/deb
./build.sh
sudo dpkg -i qmk-toolbox_*.deb
```

### AppImage

```bash
cd linux/packaging/appimage
./build.sh
chmod +x qmk-toolbox-*.AppImage
./qmk-toolbox-*.AppImage
```

## Development

### Setting up Development Environment

```bash
cd linux

# Create virtual environment
python3 -m venv venv
source venv/bin/activate

# Install in editable mode with dev dependencies
pip install -e ".[dev]"

# Run tests
pytest

# Format code
black src/

# Type checking
mypy src/
```

### Project Structure

```
linux/
├── src/qmk_toolbox/          # Main package
│   ├── ui/                   # Qt UI components
│   ├── usb/                  # USB device detection
│   ├── hid/                  # HID console listener
│   ├── bootloader/           # Bootloader implementations
│   └── helpers/              # Utility functions
├── packaging/                # Packaging scripts
│   ├── deb/                  # Debian package
│   ├── arch/                 # Arch Linux PKGBUILD
│   ├── appimage/             # AppImage build
│   └── 99-qmk.rules          # udev rules
└── resources/                # Icons, desktop entry
```

## Troubleshooting

### USB Devices Not Detected

1. Make sure udev rules are installed:
   ```bash
   ls -l /etc/udev/rules.d/99-qmk.rules
   ```

2. Reload udev:
   ```bash
   sudo udevadm control --reload-rules
   sudo udevadm trigger
   ```

3. Check if your user is in the plugdev group:
   ```bash
   groups $USER
   ```

4. Try running with sudo (not recommended for regular use):
   ```bash
   sudo qmk-toolbox
   ```

### HID Console Not Working

Make sure hidapi is installed and accessible:
```bash
python3 -c "import hid; print(hid.enumerate())"
```

### Flashing Tools Not Found

Install the required tools for your bootloader:
```bash
# For Atmel DFU
sudo apt install dfu-programmer

# For ARM DFU
sudo apt install dfu-util

# For Caterina
sudo apt install avrdude

# For Teensy
sudo apt install teensy-loader-cli
```

### Qt/PySide6 Issues

If PySide6 installation fails, try:
```bash
pip install --upgrade pip
pip install PySide6 --prefer-binary
```

## Uninstall

### From pip

```bash
pip uninstall qmk-toolbox
```

### Debian package

```bash
sudo apt remove qmk-toolbox
```

### Clean up udev rules

```bash
sudo rm /etc/udev/rules.d/99-qmk.rules
sudo udevadm control --reload-rules
```
