from .bootloader_device import BootloaderDevice
from .bootloader_type import BootloaderType


class HalfKayDevice(BootloaderDevice):
    def __init__(self, usb_device):
        super().__init__(usb_device)
        self.bootloader_type = BootloaderType.HALFKAY
        self.name = "HalfKay"
        self.preferred_driver = "usbhid"
    
    async def flash(self, mcu: str, file_path: str) -> int:
        args = ["-mmcu=" + mcu, "-w", file_path, "-v"]
        return await self._run_process("teensy-loader-cli", args)
