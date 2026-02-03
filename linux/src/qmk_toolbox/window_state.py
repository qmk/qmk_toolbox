from PySide6.QtCore import QObject, Signal, Property


class WindowState(QObject):
    autoFlashEnabledChanged = Signal(bool)
    showAllDevicesChanged = Signal(bool)
    canFlashChanged = Signal(bool)
    canResetChanged = Signal(bool)
    canClearEepromChanged = Signal(bool)
    
    def __init__(self):
        super().__init__()
        self._auto_flash_enabled = False
        self._show_all_devices = False
        self._can_flash = False
        self._can_reset = False
        self._can_clear_eeprom = False
    
    @Property(bool, notify=autoFlashEnabledChanged)
    def autoFlashEnabled(self):
        return self._auto_flash_enabled
    
    @autoFlashEnabled.setter
    def autoFlashEnabled(self, value):
        if self._auto_flash_enabled != value:
            self._auto_flash_enabled = value
            self.autoFlashEnabledChanged.emit(value)
    
    @Property(bool, notify=showAllDevicesChanged)
    def showAllDevices(self):
        return self._show_all_devices
    
    @showAllDevices.setter
    def showAllDevices(self, value):
        if self._show_all_devices != value:
            self._show_all_devices = value
            self.showAllDevicesChanged.emit(value)
    
    @Property(bool, notify=canFlashChanged)
    def canFlash(self):
        return self._can_flash
    
    @canFlash.setter
    def canFlash(self, value):
        if self._can_flash != value:
            self._can_flash = value
            self.canFlashChanged.emit(value)
    
    @Property(bool, notify=canResetChanged)
    def canReset(self):
        return self._can_reset
    
    @canReset.setter
    def canReset(self, value):
        if self._can_reset != value:
            self._can_reset = value
            self.canResetChanged.emit(value)
    
    @Property(bool, notify=canClearEepromChanged)
    def canClearEeprom(self):
        return self._can_clear_eeprom
    
    @canClearEeprom.setter
    def canClearEeprom(self, value):
        if self._can_clear_eeprom != value:
            self._can_clear_eeprom = value
            self.canClearEepromChanged.emit(value)
