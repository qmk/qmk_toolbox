from .bootloader_device import BootloaderDevice
from .bootloader_type import BootloaderType


class AtmelDfuDevice(BootloaderDevice):
    def __init__(self, usb_device):
        super().__init__(usb_device)
        self.bootloader_type = BootloaderType.ATMEL_DFU
        self.name = "Atmel DFU"
        self.preferred_driver = "usbhid"
        self.is_eeprom_flashable = True
        self.is_resettable = True
    
    async def flash(self, mcu: str, file_path: str) -> int:
        result = await self._run_process("dfu-programmer", [mcu, "erase", "--force"])
        if result != 0:
            return result
        
        result = await self._run_process("dfu-programmer", [mcu, "flash", file_path])
        if result != 0:
            return result
        
        return await self._run_process("dfu-programmer", [mcu, "reset"])
    
    async def flash_eeprom(self, mcu: str, file_path: str) -> int:
        result = await self._run_process("dfu-programmer", [mcu, "eeprom", file_path])
        if result != 0:
            return result
        return await self._run_process("dfu-programmer", [mcu, "reset"])
    
    async def reset(self, mcu: str) -> int:
        return await self._run_process("dfu-programmer", [mcu, "reset"])
