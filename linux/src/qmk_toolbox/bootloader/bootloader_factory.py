from .bootloader_device import BootloaderDevice
from .bootloader_type import BootloaderType
from ..usb.usb_device import UsbDevice


BOOTLOADER_VID_PID = {
    (0x03EB, 0x2FEF): BootloaderType.ATMEL_DFU,
    (0x03EB, 0x2FF0): BootloaderType.ATMEL_DFU,
    (0x03EB, 0x2FF3): BootloaderType.ATMEL_DFU,
    (0x03EB, 0x2FF4): BootloaderType.ATMEL_DFU,
    (0x03EB, 0x2FF9): BootloaderType.ATMEL_DFU,
    (0x03EB, 0x2FFA): BootloaderType.ATMEL_DFU,
    (0x03EB, 0x2FFB): BootloaderType.ATMEL_DFU,
    (0x03EB, 0x6124): BootloaderType.ATMEL_SAM_BA,
    (0x16C0, 0x0478): BootloaderType.HALFKAY,
    (0x16C0, 0x05DF): BootloaderType.BOOTLOAD_HID,
    (0x16C0, 0x05DC): BootloaderType.LUFA_HID,
    (0x1C11, 0xB007): BootloaderType.KIIBOHD_DFU,
    (0x1EAF, 0x0003): BootloaderType.STM32DUINO,
    (0x1209, 0x2301): BootloaderType.LUFA_MS,
    (0x0483, 0xDF11): BootloaderType.STM32_DFU,
    (0x314B, 0x0106): BootloaderType.APM32_DFU,
    (0x28E9, 0x0189): BootloaderType.GD32V_DFU,
    (0x342D, 0xDFA0): BootloaderType.WB32_DFU,
    (0x16C0, 0x0483): BootloaderType.AVR_ISP,
    (0x16C0, 0x05DC): BootloaderType.USBASP,
    (0x1781, 0x0C9F): BootloaderType.USBTINY_ISP,
    (0x2341, 0x0036): BootloaderType.CATERINA,
    (0x2341, 0x0037): BootloaderType.CATERINA,
    (0x2341, 0x8036): BootloaderType.CATERINA,
    (0x2341, 0x8037): BootloaderType.CATERINA,
    (0x1B4F, 0x9203): BootloaderType.CATERINA,
    (0x1B4F, 0x9205): BootloaderType.CATERINA,
    (0x1B4F, 0x9207): BootloaderType.CATERINA,
    (0x2A03, 0x0036): BootloaderType.CATERINA,
    (0x2A03, 0x0037): BootloaderType.CATERINA,
}


class BootloaderFactory:
    @staticmethod
    def create(usb_device: UsbDevice):
        key = (usb_device.vendor_id, usb_device.product_id)
        bootloader_type = BOOTLOADER_VID_PID.get(key)
        
        if not bootloader_type:
            return None
        
        if bootloader_type == BootloaderType.ATMEL_DFU:
            from .atmel_dfu_device import AtmelDfuDevice
            return AtmelDfuDevice(usb_device)
        elif bootloader_type == BootloaderType.STM32_DFU:
            from .stm32_dfu_device import Stm32DfuDevice
            return Stm32DfuDevice(usb_device)
        elif bootloader_type == BootloaderType.APM32_DFU:
            from .apm32_dfu_device import Apm32DfuDevice
            return Apm32DfuDevice(usb_device)
        elif bootloader_type == BootloaderType.KIIBOHD_DFU:
            from .kiibohd_dfu_device import KiibohdDfuDevice
            return KiibohdDfuDevice(usb_device)
        elif bootloader_type == BootloaderType.STM32DUINO:
            from .stm32duino_device import Stm32DuinoDevice
            return Stm32DuinoDevice(usb_device)
        elif bootloader_type == BootloaderType.GD32V_DFU:
            from .gd32v_dfu_device import Gd32vDfuDevice
            return Gd32vDfuDevice(usb_device)
        elif bootloader_type == BootloaderType.WB32_DFU:
            from .wb32_dfu_device import Wb32DfuDevice
            return Wb32DfuDevice(usb_device)
        elif bootloader_type == BootloaderType.CATERINA:
            from .caterina_device import CaterinaDevice
            return CaterinaDevice(usb_device)
        elif bootloader_type == BootloaderType.HALFKAY:
            from .halfkay_device import HalfKayDevice
            return HalfKayDevice(usb_device)
        elif bootloader_type == BootloaderType.BOOTLOAD_HID:
            from .bootload_hid_device import BootloadHidDevice
            return BootloadHidDevice(usb_device)
        elif bootloader_type == BootloaderType.LUFA_HID:
            from .lufa_hid_device import LufaHidDevice
            return LufaHidDevice(usb_device)
        elif bootloader_type == BootloaderType.LUFA_MS:
            from .lufa_ms_device import LufaMsDevice
            return LufaMsDevice(usb_device)
        elif bootloader_type == BootloaderType.ATMEL_SAM_BA:
            from .atmel_samba_device import AtmelSamBaDevice
            return AtmelSamBaDevice(usb_device)
        elif bootloader_type in (BootloaderType.AVR_ISP, BootloaderType.USBASP, BootloaderType.USBTINY_ISP):
            from .isp_device import IspDevice
            return IspDevice(usb_device, bootloader_type)
        
        return None
