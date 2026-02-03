from PySide6.QtWidgets import QTextEdit
from PySide6.QtGui import QTextCharFormat, QColor, QTextCursor
from PySide6.QtCore import Qt
from ..message_type import MessageType


class LogWidget(QTextEdit):
    MESSAGE_COLORS = {
        MessageType.INFO: QColor(0, 0, 0),
        MessageType.COMMAND: QColor(0, 0, 255),
        MessageType.BOOTLOADER: QColor(255, 165, 0),
        MessageType.HID: QColor(128, 0, 128),
        MessageType.ERROR: QColor(255, 0, 0),
        MessageType.WARNING: QColor(255, 140, 0),
    }
    
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setReadOnly(True)
        self.setLineWrapMode(QTextEdit.LineWrapMode.NoWrap)
    
    def log(self, message: str, msg_type: MessageType = MessageType.INFO):
        fmt = QTextCharFormat()
        fmt.setForeground(self.MESSAGE_COLORS.get(msg_type, QColor(0, 0, 0)))
        
        cursor = self.textCursor()
        cursor.movePosition(QTextCursor.MoveOperation.End)
        cursor.insertText(message + "\n", fmt)
        
        self.setTextCursor(cursor)
        self.ensureCursorVisible()
    
    def logInfo(self, message: str):
        self.log(message, MessageType.INFO)
    
    def logError(self, message: str):
        self.log(message, MessageType.ERROR)
    
    def logBootloader(self, message: str):
        self.log(message, MessageType.BOOTLOADER)
    
    def logHid(self, message: str):
        self.log(message, MessageType.HID)
    
    def logCommand(self, message: str):
        self.log(message, MessageType.COMMAND)
    
    def logWarning(self, message: str):
        self.log(message, MessageType.WARNING)
