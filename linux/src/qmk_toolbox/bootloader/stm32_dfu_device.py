from .bootloader_device import BootloaderDevice
from .bootloader_type import BootloaderType


class Stm32DfuDevice(BootloaderDevice):
    def __init__(self, usb_device):
        super().__init__(usb_device)
        self.bootloader_type = BootloaderType.STM32_DFU
        self.name = "STM32 DFU"
        self.preferred_driver = "usbhid"
    
    async def flash(self, mcu: str, file_path: str) -> int:
        args = ["-d", f"{self.vendor_id:04x}:{self.product_id:04x}", "-a", "0", "-s", "0x08000000:leave", "-D", file_path]
        return await self._run_process("dfu-util", args)
