import hid
import threading
from typing import Optional, Callable, List


CONSOLE_USAGE_PAGE = 0xFF31
CONSOLE_USAGE = 0x0074

RAW_USAGE_PAGE = 0xFF60
RAW_USAGE = 0x0061


class HidConsoleDevice:
    def __init__(self, hid_device_info):
        self.device_info = hid_device_info
        self.vendor_id = hid_device_info['vendor_id']
        self.product_id = hid_device_info['product_id']
        self.manufacturer = hid_device_info.get('manufacturer_string', '')
        self.product = hid_device_info.get('product_string', '')
        self.path = hid_device_info['path']
        
        self.device = None
        self.thread = None
        self.running = False
        self.console_report_received: Optional[Callable] = None
    
    def start_listening(self):
        try:
            self.device = hid.device()
            self.device.open_path(self.path)
            self.device.set_nonblocking(True)
            self.running = True
            self.thread = threading.Thread(target=self._read_loop, daemon=True)
            self.thread.start()
        except Exception as e:
            print(f"Failed to open HID device: {e}")
    
    def stop_listening(self):
        self.running = False
        if self.thread:
            self.thread.join(timeout=1.0)
        if self.device:
            try:
                self.device.close()
            except:
                pass
            self.device = None
    
    def _read_loop(self):
        while self.running and self.device:
            try:
                data = self.device.read(64, timeout_ms=100)
                if data and len(data) > 0:
                    text = self._parse_console_data(data)
                    if text and self.console_report_received:
                        self.console_report_received(self, text)
            except Exception as e:
                print(f"Error reading HID data: {e}")
                break
    
    def _parse_console_data(self, data: List[int]) -> str:
        try:
            text = bytes(data).decode('utf-8', errors='ignore').rstrip('\x00')
            return text
        except:
            return ""
    
    def __str__(self):
        parts = []
        if self.manufacturer:
            parts.append(self.manufacturer)
        if self.product:
            parts.append(self.product)
        parts.append(f"({self.vendor_id:04X}:{self.product_id:04X})")
        return " ".join(parts)


class HidListener:
    def __init__(self):
        self.devices: List[HidConsoleDevice] = []
        self.running = False
        self.thread = None
        
        self.hid_device_connected: Optional[Callable] = None
        self.hid_device_disconnected: Optional[Callable] = None
        self.console_report_received: Optional[Callable] = None
    
    def start(self):
        self.running = True
        self._enumerate_hid_devices()
        self.thread = threading.Thread(target=self._monitor_loop, daemon=True)
        self.thread.start()
    
    def stop(self):
        self.running = False
        if self.thread:
            self.thread.join(timeout=1.0)
        
        for device in self.devices:
            device.stop_listening()
        self.devices.clear()
    
    def _enumerate_hid_devices(self):
        try:
            all_devices = hid.enumerate()
            
            # On Linux, hidapi often reports usage_page and usage as 0
            # Try filtering by usage_page/usage first (works on some systems)
            console_devices = [
                d for d in all_devices
                if d.get('usage_page') == CONSOLE_USAGE_PAGE and d.get('usage') == CONSOLE_USAGE
            ]
            
            # If no devices found with usage filtering, try probing devices
            # that could potentially be console devices (interface number > 0 typically)
            if not console_devices:
                console_devices = self._probe_for_console_devices(all_devices)
            
            current_paths = {d.path for d in self.devices}
            enumerated_paths = {d['path'] for d in console_devices}
            
            for dev_info in console_devices:
                if dev_info['path'] not in current_paths:
                    device = HidConsoleDevice(dev_info)
                    device.console_report_received = self._console_report_received
                    self.devices.append(device)
                    device.start_listening()
                    
                    if self.hid_device_connected:
                        self.hid_device_connected(device)
            
            for device in list(self.devices):
                if device.path not in enumerated_paths:
                    device.stop_listening()
                    self.devices.remove(device)
                    
                    if self.hid_device_disconnected:
                        self.hid_device_disconnected(device)
        
        except Exception as e:
            print(f"Error enumerating HID devices: {e}")
    
    def _probe_for_console_devices(self, all_devices: List) -> List:
        """
        Probe HID devices to find potential console devices.
        On Linux, usage_page/usage are often not reported, so we need to
        try opening devices and checking if they might be console devices.
        """
        console_candidates = []
        
        for dev_info in all_devices:
            # Skip interface 0 (usually keyboard/mouse)
            # Console devices are typically on interface 1 or higher
            interface_num = dev_info.get('interface_number', -1)
            if interface_num <= 0:
                continue
            
            # Try to open the device
            try:
                device = hid.device()
                device.open_path(dev_info['path'])
                device.close()
                
                # If we can open it, it's a candidate
                console_candidates.append(dev_info)
            except Exception:
                # Can't open, skip it
                pass
        
        return console_candidates
    
    def _console_report_received(self, device: HidConsoleDevice, data: str):
        if self.console_report_received:
            self.console_report_received(device, data)
    
    def _monitor_loop(self):
        import time
        while self.running:
            self._enumerate_hid_devices()
            time.sleep(1.0)
