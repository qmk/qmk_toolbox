from PySide6.QtWidgets import QWidget, QLabel
from PySide6.QtCore import Qt, Property, Signal
from PySide6.QtGui import QPalette, QColor


class KeyWidget(QWidget):
    pressedChanged = Signal(bool)
    testedChanged = Signal(bool)
    
    def __init__(self, label: str, parent=None):
        super().__init__(parent)
        self._pressed = False
        self._tested = False
        self._label = label
        
        self.setMinimumSize(40, 40)
        self.setAutoFillBackground(True)
        self._update_style()
        
        self.label_widget = QLabel(label, self)
        self.label_widget.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.label_widget.setStyleSheet("background: transparent; color: black;")
        self.label_widget.setGeometry(0, 0, self.width(), self.height())
    
    @Property(bool, notify=pressedChanged)
    def pressed(self):
        return self._pressed
    
    @pressed.setter
    def pressed(self, value):
        if self._pressed != value:
            self._pressed = value
            self._update_style()
            self.pressedChanged.emit(value)
    
    @Property(bool, notify=testedChanged)
    def tested(self):
        return self._tested
    
    @tested.setter
    def tested(self, value):
        if self._tested != value:
            self._tested = value
            self._update_style()
            self.testedChanged.emit(value)
    
    def _update_style(self):
        palette = self.palette()
        
        if self._pressed:
            palette.setColor(QPalette.ColorRole.Window, QColor(0, 255, 0))
        elif self._tested:
            palette.setColor(QPalette.ColorRole.Window, QColor(144, 238, 144))
        else:
            palette.setColor(QPalette.ColorRole.Window, QColor(211, 211, 211))
        
        self.setPalette(palette)
    
    def resizeEvent(self, event):
        super().resizeEvent(event)
        if hasattr(self, 'label_widget'):
            self.label_widget.setGeometry(0, 0, self.width(), self.height())
