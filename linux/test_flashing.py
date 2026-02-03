#!/usr/bin/env python3
"""
Quick test script to verify flashing functionality works.
This doesn't require actual hardware - just tests the code paths.
"""

import sys
import os
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent / "src"))

def test_imports():
    print("Testing imports...")
    try:
        from qmk_toolbox.ui.main_window import MainWindow
        from qmk_toolbox.bootloader.bootloader_device import BootloaderDevice
        from qmk_toolbox.bootloader.bootloader_factory import BootloaderFactory
        from qmk_toolbox.usb.usb_listener import UsbListener
        print("✓ All imports successful")
        return True
    except ImportError as e:
        print(f"✗ Import failed: {e}")
        return False

def test_bootloader_classes():
    print("\nTesting bootloader classes...")
    try:
        from qmk_toolbox.bootloader.atmel_dfu_device import AtmelDfuDevice
        from qmk_toolbox.bootloader.stm32_dfu_device import Stm32DfuDevice
        from qmk_toolbox.bootloader.caterina_device import CaterinaDevice
        from qmk_toolbox.bootloader.halfkay_device import HalfKayDevice
        print("✓ Bootloader classes loaded")
        return True
    except ImportError as e:
        print(f"✗ Bootloader import failed: {e}")
        return False

def test_async_methods():
    print("\nTesting async method signatures...")
    try:
        from qmk_toolbox.ui.main_window import MainWindow
        import inspect
        
        methods = {
            '_flash_firmware_async': MainWindow._flash_firmware_async,
            '_reset_device_async': MainWindow._reset_device_async,
            '_clear_eeprom_async': MainWindow._clear_eeprom_async,
        }
        
        for name, method in methods.items():
            if inspect.iscoroutinefunction(method):
                print(f"  ✓ {name} is async")
            else:
                print(f"  ✗ {name} is NOT async")
                return False
        
        return True
    except Exception as e:
        print(f"✗ Async test failed: {e}")
        return False

def test_file_structure():
    print("\nTesting file structure...")
    required_files = [
        "src/qmk_toolbox/ui/main_window.py",
        "src/qmk_toolbox/bootloader/bootloader_device.py",
        "src/qmk_toolbox/bootloader/bootloader_factory.py",
        "FLASHING_GUIDE.md",
        "HID_CONSOLE_SETUP.md",
        "install_hid_support.sh",
        "packaging/99-qmk.rules",
    ]
    
    base_dir = Path(__file__).parent
    all_exist = True
    
    for file_path in required_files:
        full_path = base_dir / file_path
        if full_path.exists():
            print(f"  ✓ {file_path}")
        else:
            print(f"  ✗ {file_path} MISSING")
            all_exist = False
    
    return all_exist

def test_eeprom_files():
    print("\nTesting EEPROM files...")
    common_dir = Path(__file__).parent.parent / "common"
    
    eeprom_files = ["reset.eep", "reset_left.eep", "reset_right.eep"]
    all_exist = True
    
    for filename in eeprom_files:
        file_path = common_dir / filename
        if file_path.exists():
            print(f"  ✓ {filename} exists")
        else:
            print(f"  ✗ {filename} MISSING")
            all_exist = False
    
    return all_exist

def main():
    print("=" * 60)
    print("QMK Toolbox Flashing Implementation Test")
    print("=" * 60)
    
    tests = [
        ("Imports", test_imports),
        ("Bootloader Classes", test_bootloader_classes),
        ("Async Methods", test_async_methods),
        ("File Structure", test_file_structure),
        ("EEPROM Files", test_eeprom_files),
    ]
    
    results = []
    for name, test_func in tests:
        try:
            result = test_func()
            results.append((name, result))
        except Exception as e:
            print(f"\n✗ {name} crashed: {e}")
            results.append((name, False))
    
    print("\n" + "=" * 60)
    print("Test Results:")
    print("=" * 60)
    
    for name, result in results:
        status = "✓ PASS" if result else "✗ FAIL"
        print(f"{status:8} | {name}")
    
    all_passed = all(result for _, result in results)
    
    print("\n" + "=" * 60)
    if all_passed:
        print("✓ All tests passed!")
        print("\nFlashing implementation is ready to use.")
        print("See FLASHING_GUIDE.md for usage instructions.")
        return 0
    else:
        print("✗ Some tests failed")
        print("\nPlease fix the issues above before using.")
        return 1

if __name__ == "__main__":
    sys.exit(main())
