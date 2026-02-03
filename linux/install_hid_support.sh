#!/bin/bash
# Quick installation script for QMK Toolbox HID Console support

set -e

echo "=== QMK Toolbox HID Console Setup ==="
echo ""

# Check if running as root
if [ "$EUID" -eq 0 ]; then 
    echo "Please run this script as a normal user (not root)"
    echo "The script will use sudo when needed"
    exit 1
fi

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Step 1: Installing udev rules..."
sudo cp "$SCRIPT_DIR/packaging/99-qmk.rules" /etc/udev/rules.d/
echo "✓ Rules installed"

echo ""
echo "Step 2: Reloading udev..."
sudo udevadm control --reload-rules
sudo udevadm trigger
echo "✓ Udev reloaded"

echo ""
echo "Step 3: Checking permissions..."
sleep 1

# Check if any hidraw devices are accessible
if ls /dev/hidraw* > /dev/null 2>&1; then
    FIRST_HIDRAW=$(ls /dev/hidraw* | head -1)
    PERMS=$(stat -c "%a" "$FIRST_HIDRAW")
    echo "Sample device: $FIRST_HIDRAW (permissions: $PERMS)"
    
    if [ "$PERMS" = "666" ] || [ "$PERMS" = "660" ]; then
        echo "✓ Permissions look correct"
    else
        echo "⚠ Permissions may need update - try unplugging/replugging devices"
    fi
else
    echo "ℹ No hidraw devices currently present"
fi

echo ""
echo "=== Installation Complete ==="
echo ""
echo "Next steps:"
echo "1. Unplug and replug your keyboard (or logout/login)"
echo "2. Run: source venv/bin/activate"
echo "3. Run: qmk-toolbox"
echo "4. Open: Tools → HID Console"
echo ""
echo "Note: Your keyboard needs QMK firmware with CONSOLE_ENABLE=yes"
echo "See HID_CONSOLE_SETUP.md for more details"
