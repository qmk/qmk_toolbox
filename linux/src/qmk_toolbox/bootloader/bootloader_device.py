import asyncio
import subprocess
from typing import Optional, Callable
from abc import ABC, abstractmethod
from ..message_type import MessageType
from ..usb.usb_device import UsbDevice
from .bootloader_type import BootloaderType


class BootloaderDevice(ABC):
    def __init__(self, usb_device: UsbDevice):
        self.usb_device = usb_device
        self.vendor_id = usb_device.vendor_id
        self.product_id = usb_device.product_id
        self.revision_bcd = usb_device.revision_bcd
        self.manufacturer_string = usb_device.manufacturer_string
        self.product_string = usb_device.product_string
        self.driver = usb_device.driver
        
        self.bootloader_type: Optional[BootloaderType] = None
        self.name: str = "Unknown Bootloader"
        self.preferred_driver: Optional[str] = None
        self.is_eeprom_flashable: bool = False
        self.is_resettable: bool = False
        
        self.output_received: Optional[Callable] = None
    
    def matches(self, pyudev_device) -> bool:
        return self.usb_device.matches(pyudev_device)
    
    def __str__(self):
        return str(self.usb_device)
    
    @abstractmethod
    async def flash(self, mcu: str, file_path: str) -> int:
        raise NotImplementedError
    
    async def flash_eeprom(self, mcu: str, file_path: str) -> int:
        raise NotImplementedError("EEPROM flashing not supported")
    
    async def reset(self, mcu: str) -> int:
        raise NotImplementedError("Reset not supported")
    
    def _print_message(self, message: str, msg_type: MessageType):
        if self.output_received:
            self.output_received(self, message, msg_type)
    
    async def _run_process(self, command: str, args: list) -> int:
        full_command = [command] + args
        cmd_str = " ".join(full_command)
        self._print_message(cmd_str, MessageType.COMMAND)
        
        try:
            process = await asyncio.create_subprocess_exec(
                *full_command,
                stdout=asyncio.subprocess.PIPE,
                stderr=asyncio.subprocess.PIPE
            )
            
            async def read_stream(stream, msg_type):
                while True:
                    line = await stream.readline()
                    if not line:
                        break
                    text = line.decode('utf-8', errors='replace').rstrip()
                    if text:
                        self._print_message(text, msg_type)
            
            await asyncio.gather(
                read_stream(process.stdout, MessageType.INFO),
                read_stream(process.stderr, MessageType.ERROR)
            )
            
            return_code = await process.wait()
            return return_code
            
        except FileNotFoundError:
            self._print_message(f"Command not found: {command}", MessageType.ERROR)
            self._print_message(f"Please install {command} using your package manager", MessageType.ERROR)
            return 127
        except Exception as e:
            self._print_message(f"Error running command: {e}", MessageType.ERROR)
            return 1
