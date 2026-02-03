from PySide6.QtWidgets import (QMainWindow, QWidget, QVBoxLayout, QHBoxLayout,
                               QPushButton, QComboBox, QTextEdit, QLabel)
from PySide6.QtCore import Qt, Slot
from PySide6.QtGui import QTextCharFormat, QColor, QTextCursor
from ..hid.hid_listener import HidListener, HidConsoleDevice
from ..message_type import MessageType


class HidConsoleWindow(QMainWindow):
    """HID Console window for displaying console output from QMK keyboards."""
    
    MESSAGE_COLORS = {
        MessageType.INFO: QColor(0, 0, 0),
        MessageType.HID: QColor(128, 0, 128),
        MessageType.WARNING: QColor(255, 140, 0),
        MessageType.ERROR: QColor(255, 0, 0),
    }
    
    def __init__(self, parent=None):
        super().__init__(parent)
        self.hid_listener = HidListener()
        self.last_reported_device = None
        self.init_ui()
        self.setup_listener()
    
    def init_ui(self):
        self.setWindowTitle("HID Console")
        self.resize(800, 600)
        
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        main_layout = QVBoxLayout(central_widget)
        
        # Top bar with device selector and clear button
        top_bar = QHBoxLayout()
        
        device_label = QLabel("Device:")
        top_bar.addWidget(device_label)
        
        self.console_combo = QComboBox()
        self.console_combo.setMinimumWidth(300)
        top_bar.addWidget(self.console_combo, 1)
        
        self.clear_button = QPushButton("Clear")
        self.clear_button.clicked.connect(self.clear_console)
        top_bar.addWidget(self.clear_button)
        
        main_layout.addLayout(top_bar)
        
        # Console text area
        self.console_text = QTextEdit()
        self.console_text.setReadOnly(True)
        self.console_text.setLineWrapMode(QTextEdit.LineWrapMode.NoWrap)
        main_layout.addWidget(self.console_text, 1)
    
    def setup_listener(self):
        """Connect HID listener signals."""
        self.hid_listener.hid_device_connected = self.on_hid_device_connected
        self.hid_listener.hid_device_disconnected = self.on_hid_device_disconnected
        self.hid_listener.console_report_received = self.on_console_report_received
        self.hid_listener.start()
    
    def on_hid_device_connected(self, device: HidConsoleDevice):
        """Handle HID device connection."""
        self.last_reported_device = device
        self.update_console_list()
        self.log_hid(f"HID console connected: {device}")
    
    def on_hid_device_disconnected(self, device: HidConsoleDevice):
        """Handle HID device disconnection."""
        if self.last_reported_device == device:
            self.last_reported_device = None
        self.update_console_list()
        self.log_hid(f"HID console disconnected: {device}")
    
    def on_console_report_received(self, device: HidConsoleDevice, report: str):
        """Handle incoming console report from HID device."""
        selected_index = self.console_combo.currentIndex()
        
        # Filter by selected device (0 = all devices, 1+ = specific device)
        if selected_index == 0 or (selected_index > 0 and 
                                    selected_index - 1 < len(self.hid_listener.devices) and
                                    self.hid_listener.devices[selected_index - 1] == device):
            # Print device header if this is a new device
            if self.last_reported_device != device:
                device_name = f"{device.manufacturer} {device.product}".strip()
                if device_name:
                    self.log_hid(f"{device_name}:")
                self.last_reported_device = device
            
            # Print the actual console output
            self.log_hid_output(report)
    
    def update_console_list(self):
        """Update the device selection combo box."""
        selected = self.console_combo.currentIndex() if self.console_combo.currentIndex() >= 0 else 0
        self.console_combo.clear()
        
        # Add individual devices
        for device in self.hid_listener.devices:
            self.console_combo.addItem(str(device))
        
        # Add "All devices" option at the beginning if we have devices
        if self.console_combo.count() > 0:
            self.console_combo.insertItem(0, "(All connected devices)")
            # Restore selection or default to "All devices"
            if self.console_combo.count() > selected:
                self.console_combo.setCurrentIndex(selected)
            else:
                self.console_combo.setCurrentIndex(0)
    
    def log(self, message: str, msg_type: MessageType = MessageType.INFO):
        """Log a message with color formatting."""
        fmt = QTextCharFormat()
        fmt.setForeground(self.MESSAGE_COLORS.get(msg_type, QColor(0, 0, 0)))
        
        cursor = self.console_text.textCursor()
        cursor.movePosition(QTextCursor.MoveOperation.End)
        cursor.insertText(message + "\n", fmt)
        
        self.console_text.setTextCursor(cursor)
        self.console_text.ensureCursorVisible()
    
    def log_hid(self, message: str):
        """Log a HID-related message."""
        self.log(message, MessageType.HID)
    
    def log_hid_output(self, message: str):
        """Log console output from the HID device (plain text)."""
        self.log(message, MessageType.INFO)
    
    @Slot()
    def clear_console(self):
        """Clear the console text area."""
        self.console_text.clear()
    
    def closeEvent(self, event):
        """Handle window close event."""
        self.hid_listener.stop()
        event.accept()
