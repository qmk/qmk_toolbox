from .bootloader_device import BootloaderDevice
from .bootloader_type import BootloaderType


class KiibohdDfuDevice(BootloaderDevice):
    def __init__(self, usb_device):
        super().__init__(usb_device)
        self.bootloader_type = BootloaderType.KIIBOHD_DFU
        self.name = "Kiibohd DFU"
    
    async def flash(self, mcu: str, file_path: str) -> int:
        args = ["-d", f"{self.vendor_id:04x}:{self.product_id:04x}", "-D", file_path]
        return await self._run_process("dfu-util", args)
