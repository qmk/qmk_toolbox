#!/bin/bash
# Script to test HID console functionality after udev rules installation

echo "=== QMK Toolbox HID Console Test ==="
echo ""

# Check if udev rules are installed
if [ -f /etc/udev/rules.d/99-qmk.rules ]; then
    echo "✓ udev rules are installed"
else
    echo "✗ udev rules NOT found at /etc/udev/rules.d/99-qmk.rules"
    echo "  Please run: sudo cp packaging/99-qmk.rules /etc/udev/rules.d/"
    exit 1
fi

# Check hidraw permissions
echo ""
echo "HID device permissions:"
ls -la /dev/hidraw* | head -5

# Check if we can read hidraw devices
echo ""
echo "Testing HID device access..."
python3 << 'EOF'
import hid
import sys

try:
    all_devices = hid.enumerate()
    print(f"Found {len(all_devices)} HID devices total")
    
    # Try to open some devices
    accessible_count = 0
    for dev in all_devices[:5]:  # Test first 5 devices
        try:
            device = hid.device()
            device.open_path(dev['path'])
            device.close()
            accessible_count += 1
        except Exception as e:
            pass
    
    print(f"Can access {accessible_count} out of {min(5, len(all_devices))} tested devices")
    
    if accessible_count > 0:
        print("\n✓ HID device access is working!")
        print("\nYou can now run QMK Toolbox and open the HID Console (Tools → HID Console)")
        print("If your keyboard has CONSOLE_ENABLE=yes, it should appear in the device list.")
    else:
        print("\n✗ Cannot access HID devices")
        print("  Try logging out and back in, or reboot your system")
        sys.exit(1)
        
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)
EOF
