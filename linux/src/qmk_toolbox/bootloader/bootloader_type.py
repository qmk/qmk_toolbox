from enum import Enum, auto


class BootloaderType(Enum):
    APM32_DFU = auto()
    ATMEL_DFU = auto()
    ATMEL_SAM_BA = auto()
    BOOTLOAD_HID = auto()
    CATERINA = auto()
    GD32V_DFU = auto()
    HALFKAY = auto()
    KIIBOHD_DFU = auto()
    LUFA_HID = auto()
    LUFA_MS = auto()
    STM32_DFU = auto()
    STM32DUINO = auto()
    USBASP = auto()
    USBTINY_ISP = auto()
    AVR_ISP = auto()
    WB32_DFU = auto()
