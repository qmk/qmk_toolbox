from PySide6.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QGridLayout,
                               QLabel, QFrame)
from PySide6.QtCore import Qt, Slot
from PySide6.QtGui import QKeyEvent
from .key_widget import KeyWidget


class KeyTesterWindow(QWidget):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setWindowTitle("Key Tester")
        self.resize(900, 400)
        self.setWindowFlags(Qt.WindowType.Window)
        
        self.key_map = {}
        self.last_vk_label = None
        self.last_sc_label = None
        
        self.init_ui()
        self.setFocusPolicy(Qt.FocusPolicy.StrongFocus)
    
    def init_ui(self):
        main_layout = QVBoxLayout(self)
        
        info_layout = QHBoxLayout()
        self.last_vk_label = QLabel("Last VK: --")
        self.last_sc_label = QLabel("Last SC: --")
        info_layout.addWidget(self.last_vk_label)
        info_layout.addWidget(self.last_sc_label)
        info_layout.addStretch()
        main_layout.addLayout(info_layout)
        
        keyboard_widget = QWidget()
        keyboard_layout = QVBoxLayout(keyboard_widget)
        keyboard_layout.setSpacing(2)
        
        function_row = self._create_function_row()
        keyboard_layout.addLayout(function_row)
        
        keyboard_layout.addSpacing(10)
        
        number_row = self._create_number_row()
        keyboard_layout.addLayout(number_row)
        
        qwerty_row = self._create_qwerty_row()
        keyboard_layout.addLayout(qwerty_row)
        
        asdf_row = self._create_asdf_row()
        keyboard_layout.addLayout(asdf_row)
        
        zxcv_row = self._create_zxcv_row()
        keyboard_layout.addLayout(zxcv_row)
        
        bottom_row = self._create_bottom_row()
        keyboard_layout.addLayout(bottom_row)
        
        keyboard_layout.addStretch()
        
        main_layout.addWidget(keyboard_widget)
    
    def _create_function_row(self):
        row = QHBoxLayout()
        row.setSpacing(2)
        
        self._add_key(row, Qt.Key.Key_Escape, "Esc")
        row.addSpacing(30)
        
        for i in range(1, 13):
            key = getattr(Qt.Key, f"Key_F{i}")
            self._add_key(row, key, f"F{i}")
            if i in [4, 8]:
                row.addSpacing(15)
        
        row.addStretch()
        return row
    
    def _create_number_row(self):
        row = QHBoxLayout()
        row.setSpacing(2)
        
        self._add_key(row, Qt.Key.Key_QuoteLeft, "`")
        for i in range(1, 10):
            self._add_key(row, getattr(Qt.Key, f"Key_{i}"), str(i))
        self._add_key(row, Qt.Key.Key_0, "0")
        self._add_key(row, Qt.Key.Key_Minus, "-")
        self._add_key(row, Qt.Key.Key_Equal, "=")
        
        backspace = self._add_key(row, Qt.Key.Key_Backspace, "Backspace")
        backspace.setMinimumWidth(80)
        
        row.addStretch()
        return row
    
    def _create_qwerty_row(self):
        row = QHBoxLayout()
        row.setSpacing(2)
        
        tab = self._add_key(row, Qt.Key.Key_Tab, "Tab")
        tab.setMinimumWidth(60)
        
        for c in "QWERTYUIOP":
            self._add_key(row, getattr(Qt.Key, f"Key_{c}"), c)
        
        self._add_key(row, Qt.Key.Key_BracketLeft, "[")
        self._add_key(row, Qt.Key.Key_BracketRight, "]")
        self._add_key(row, Qt.Key.Key_Backslash, "\\")
        
        row.addStretch()
        return row
    
    def _create_asdf_row(self):
        row = QHBoxLayout()
        row.setSpacing(2)
        
        caps = self._add_key(row, Qt.Key.Key_CapsLock, "Caps")
        caps.setMinimumWidth(70)
        
        for c in "ASDFGHJKL":
            self._add_key(row, getattr(Qt.Key, f"Key_{c}"), c)
        
        self._add_key(row, Qt.Key.Key_Semicolon, ";")
        self._add_key(row, Qt.Key.Key_Apostrophe, "'")
        
        enter = self._add_key(row, Qt.Key.Key_Return, "Enter")
        enter.setMinimumWidth(70)
        
        row.addStretch()
        return row
    
    def _create_zxcv_row(self):
        row = QHBoxLayout()
        row.setSpacing(2)
        
        lshift = self._add_key(row, Qt.Key.Key_Shift, "Shift")
        lshift.setMinimumWidth(90)
        
        for c in "ZXCVBNM":
            self._add_key(row, getattr(Qt.Key, f"Key_{c}"), c)
        
        self._add_key(row, Qt.Key.Key_Comma, ",")
        self._add_key(row, Qt.Key.Key_Period, ".")
        self._add_key(row, Qt.Key.Key_Slash, "/")
        
        rshift = self._add_key(row, Qt.Key.Key_Shift, "Shift", suffix="_R")
        rshift.setMinimumWidth(90)
        
        row.addStretch()
        return row
    
    def _create_bottom_row(self):
        row = QHBoxLayout()
        row.setSpacing(2)
        
        ctrl = self._add_key(row, Qt.Key.Key_Control, "Ctrl")
        ctrl.setMinimumWidth(50)
        
        meta = self._add_key(row, Qt.Key.Key_Meta, "Meta")
        meta.setMinimumWidth(50)
        
        alt = self._add_key(row, Qt.Key.Key_Alt, "Alt")
        alt.setMinimumWidth(50)
        
        space = self._add_key(row, Qt.Key.Key_Space, "Space")
        space.setMinimumWidth(300)
        
        alt_r = self._add_key(row, Qt.Key.Key_Alt, "Alt", suffix="_R")
        alt_r.setMinimumWidth(50)
        
        meta_r = self._add_key(row, Qt.Key.Key_Meta, "Meta", suffix="_R")
        meta_r.setMinimumWidth(50)
        
        menu = self._add_key(row, Qt.Key.Key_Menu, "Menu")
        menu.setMinimumWidth(50)
        
        ctrl_r = self._add_key(row, Qt.Key.Key_Control, "Ctrl", suffix="_R")
        ctrl_r.setMinimumWidth(50)
        
        row.addStretch()
        return row
    
    def _add_key(self, layout, qt_key, label, suffix=""):
        key_widget = KeyWidget(label)
        layout.addWidget(key_widget)
        
        key_id = f"{qt_key}{suffix}"
        self.key_map[key_id] = key_widget
        
        return key_widget
    
    def keyPressEvent(self, event: QKeyEvent):
        key = event.key()
        key_id = f"{key}"
        
        if event.modifiers() & Qt.KeyboardModifier.ShiftModifier:
            key_id = f"{Qt.Key.Key_Shift}"
            if event.nativeScanCode() == 0x36:
                key_id += "_R"
        elif event.modifiers() & Qt.KeyboardModifier.ControlModifier:
            key_id = f"{Qt.Key.Key_Control}"
        elif event.modifiers() & Qt.KeyboardModifier.AltModifier:
            key_id = f"{Qt.Key.Key_Alt}"
        elif event.modifiers() & Qt.KeyboardModifier.MetaModifier:
            key_id = f"{Qt.Key.Key_Meta}"
        
        if key_id in self.key_map:
            widget = self.key_map[key_id]
            widget.pressed = True
            widget.tested = True
        
        self.last_vk_label.setText(f"Last VK: {key:X}")
        self.last_sc_label.setText(f"Last SC: {event.nativeScanCode():X}")
        
        super().keyPressEvent(event)
    
    def keyReleaseEvent(self, event: QKeyEvent):
        key = event.key()
        key_id = f"{key}"
        
        if event.modifiers() & Qt.KeyboardModifier.ShiftModifier:
            key_id = f"{Qt.Key.Key_Shift}"
            if event.nativeScanCode() == 0x36:
                key_id += "_R"
        elif event.modifiers() & Qt.KeyboardModifier.ControlModifier:
            key_id = f"{Qt.Key.Key_Control}"
        elif event.modifiers() & Qt.KeyboardModifier.AltModifier:
            key_id = f"{Qt.Key.Key_Alt}"
        elif event.modifiers() & Qt.KeyboardModifier.MetaModifier:
            key_id = f"{Qt.Key.Key_Meta}"
        
        if key_id in self.key_map:
            widget = self.key_map[key_id]
            widget.pressed = False
        
        super().keyReleaseEvent(event)
    
    def showEvent(self, event):
        super().showEvent(event)
        self.setFocus()
