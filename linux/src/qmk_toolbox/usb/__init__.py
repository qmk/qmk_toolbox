from typing import Optional


class UsbDevice:
    def __init__(self, pyudev_device, vendor_id: int, product_id: int, revision_bcd: int):
        self.pyudev_device = pyudev_device
        self.vendor_id = vendor_id
        self.product_id = product_id
        self.revision_bcd = revision_bcd
        
        self.manufacturer_string = pyudev_device.get('ID_VENDOR_FROM_DATABASE', '')
        self.product_string = pyudev_device.get('ID_MODEL_FROM_DATABASE', '')
        self.driver = pyudev_device.get('ID_USB_DRIVER', '')
    
    def matches(self, pyudev_device) -> bool:
        return self.pyudev_device.sys_path == pyudev_device.sys_path
    
    def __str__(self):
        parts = []
        if self.manufacturer_string:
            parts.append(self.manufacturer_string)
        if self.product_string:
            parts.append(self.product_string)
        parts.append(f"({self.vendor_id:04X}:{self.product_id:04X}:{self.revision_bcd:04X})")
        return " ".join(parts)
