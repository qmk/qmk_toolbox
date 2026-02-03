from enum import Enum


class MessageType(Enum):
    INFO = "info"
    COMMAND = "command"
    BOOTLOADER = "bootloader"
    HID = "hid"
    ERROR = "error"
    WARNING = "warning"
