import pyudev
import re
from typing import Optional, Callable, List
from .usb_device import UsbDevice
from ..bootloader.bootloader_factory import BootloaderFactory
from ..bootloader.bootloader_device import BootloaderDevice
from ..message_type import MessageType


class UsbListener:
    USB_ID_REGEX = re.compile(r"ID_VENDOR_ID=([0-9A-Fa-f]{4})\nID_MODEL_ID=([0-9A-Fa-f]{4})")
    
    def __init__(self):
        self.devices: List = []
        self.context = pyudev.Context()
        self.monitor = pyudev.Monitor.from_netlink(self.context)
        self.monitor.filter_by(subsystem='usb')
        self.observer = None
        
        self.usb_device_connected: Optional[Callable] = None
        self.usb_device_disconnected: Optional[Callable] = None
        self.bootloader_device_connected: Optional[Callable] = None
        self.bootloader_device_disconnected: Optional[Callable] = None
        self.output_received: Optional[Callable] = None
    
    def start(self):
        self._enumerate_usb_devices(connected=True)
        self.observer = pyudev.MonitorObserver(self.monitor, self._usb_device_event)
        self.observer.start()
    
    def stop(self):
        if self.observer:
            self.observer.stop()
            self.observer = None
    
    def _enumerate_usb_devices(self, connected: bool):
        enumerated = []
        for device in self.context.list_devices(subsystem='usb'):
            if device.device_type != 'usb_device':
                continue
            
            vendor_id = device.get('ID_VENDOR_ID')
            product_id = device.get('ID_MODEL_ID')
            
            if vendor_id and product_id:
                try:
                    vid = int(vendor_id, 16)
                    pid = int(product_id, 16)
                    enumerated.append((device, vid, pid))
                except ValueError:
                    continue
        
        if connected:
            for device, vid, pid in enumerated:
                if not any(d.matches(device) for d in self.devices):
                    self._add_device(device, vid, pid)
        else:
            for existing in list(self.devices):
                if not any(existing.matches(dev[0]) for dev in enumerated):
                    self._remove_device(existing)
    
    def _add_device(self, pyudev_device, vid: int, pid: int):
        revision_str = pyudev_device.get('ID_REVISION', '0000')
        try:
            revision = int(revision_str, 16)
        except ValueError:
            revision = 0
        
        usb_device = UsbDevice(pyudev_device, vid, pid, revision)
        bootloader = BootloaderFactory.create(usb_device)
        
        if bootloader:
            self.devices.append(bootloader)
            if self.bootloader_device_connected:
                self.bootloader_device_connected(bootloader)
            bootloader.output_received = self._flash_output_received
        else:
            self.devices.append(usb_device)
            if self.usb_device_connected:
                self.usb_device_connected(usb_device)
    
    def _remove_device(self, device):
        self.devices.remove(device)
        
        if isinstance(device, BootloaderDevice):
            if self.bootloader_device_disconnected:
                self.bootloader_device_disconnected(device)
            device.output_received = None
        else:
            if self.usb_device_disconnected:
                self.usb_device_disconnected(device)
    
    def _flash_output_received(self, device: BootloaderDevice, data: str, msg_type: MessageType):
        if self.output_received:
            self.output_received(device, data, msg_type)
    
    def _usb_device_event(self, action, device):
        if device.device_type != 'usb_device':
            return
        
        if action == 'add':
            vendor_id = device.get('ID_VENDOR_ID')
            product_id = device.get('ID_MODEL_ID')
            
            if vendor_id and product_id:
                try:
                    vid = int(vendor_id, 16)
                    pid = int(product_id, 16)
                    self._add_device(device, vid, pid)
                except ValueError:
                    pass
        elif action == 'remove':
            for existing in list(self.devices):
                if existing.matches(device):
                    self._remove_device(existing)
                    break
