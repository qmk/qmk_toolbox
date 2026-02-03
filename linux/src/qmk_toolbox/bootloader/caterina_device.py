from .bootloader_device import BootloaderDevice
from .bootloader_type import BootloaderType


class CaterinaDevice(BootloaderDevice):
    def __init__(self, usb_device):
        super().__init__(usb_device)
        self.bootloader_type = BootloaderType.CATERINA
        self.name = "Caterina"
        self.preferred_driver = "cdc_acm"
        self.is_eeprom_flashable = True
    
    async def flash(self, mcu: str, file_path: str) -> int:
        port = self._find_port()
        if not port:
            from ..message_type import MessageType
            self._print_message("Could not find serial port for device", MessageType.ERROR)
            return 1
        
        args = ["-p", mcu, "-c", "avr109", "-U", f"flash:w:{file_path}:i", "-P", port]
        return await self._run_process("avrdude", args)
    
    async def flash_eeprom(self, mcu: str, file_path: str) -> int:
        port = self._find_port()
        if not port:
            from ..message_type import MessageType
            self._print_message("Could not find serial port for device", MessageType.ERROR)
            return 1
        
        args = ["-p", mcu, "-c", "avr109", "-U", f"eeprom:w:{file_path}:i", "-P", port]
        return await self._run_process("avrdude", args)
    
    def _find_port(self):
        import glob
        ports = glob.glob('/dev/ttyACM*') + glob.glob('/dev/ttyUSB*')
        return ports[0] if ports else None
