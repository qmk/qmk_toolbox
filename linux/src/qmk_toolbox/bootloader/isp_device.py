from .bootloader_device import BootloaderDevice
from .bootloader_type import BootloaderType


class StubDevice(BootloaderDevice):
    def __init__(self, usb_device):
        super().__init__(usb_device)
        self.name = "Stub"
    
    async def flash(self, mcu: str, file_path: str) -> int:
        return 0
