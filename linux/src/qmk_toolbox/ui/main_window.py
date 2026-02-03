from PySide6.QtWidgets import (QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, 
                               QGroupBox, QComboBox, QPushButton, QLabel, QCheckBox,
                               QFileDialog, QMenuBar, QMenu, QMessageBox)
from PySide6.QtCore import Qt, Slot, QSettings
from PySide6.QtGui import QAction
import os
import asyncio
from pathlib import Path
from .log_widget import LogWidget
from ..window_state import WindowState
from ..usb.usb_listener import UsbListener
from ..hid.hid_listener import HidListener
from ..message_type import MessageType
from ..bootloader.bootloader_device import BootloaderDevice


class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.window_state = WindowState()
        self.usb_listener = None
        self.hid_listener = None
        self.current_firmware_path = ""
        self.settings = QSettings("QMK", "QMK Toolbox")
        
        self.init_ui()
        self.load_settings()
        self.setup_listeners()
        self.log_startup_info()
    
    def init_ui(self):
        self.setWindowTitle("QMK Toolbox")
        self.resize(933, 600)
        
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        main_layout = QVBoxLayout(central_widget)
        
        self.create_menu_bar()
        
        file_group = self.create_file_group()
        main_layout.addWidget(file_group)
        
        button_row = self.create_button_row()
        main_layout.addLayout(button_row)
        
        self.log_widget = LogWidget()
        main_layout.addWidget(self.log_widget, 1)
    
    def create_menu_bar(self):
        menubar = self.menuBar()
        
        file_menu = menubar.addMenu("&File")
        open_action = QAction("&Open", self)
        open_action.setShortcut("Ctrl+O")
        open_action.triggered.connect(self.open_file)
        file_menu.addAction(open_action)
        
        file_menu.addSeparator()
        
        exit_action = QAction("E&xit", self)
        exit_action.setShortcut("Ctrl+Q")
        exit_action.triggered.connect(self.close)
        file_menu.addAction(exit_action)
        
        tools_menu = menubar.addMenu("&Tools")
        
        self.flash_action = QAction("&Flash", self)
        self.flash_action.triggered.connect(self.flash_firmware)
        tools_menu.addAction(self.flash_action)
        
        self.reset_action = QAction("Exit &DFU", self)
        self.reset_action.triggered.connect(self.reset_device)
        tools_menu.addAction(self.reset_action)
        
        self.clear_eeprom_action = QAction("Clear &EEPROM", self)
        self.clear_eeprom_action.triggered.connect(self.clear_eeprom)
        tools_menu.addAction(self.clear_eeprom_action)
        
        tools_menu.addSeparator()
        
        self.auto_flash_action = QAction("&Auto-Flash", self)
        self.auto_flash_action.setCheckable(True)
        self.auto_flash_action.toggled.connect(self.toggle_auto_flash)
        tools_menu.addAction(self.auto_flash_action)
        
        self.show_all_action = QAction("Show &All Devices", self)
        self.show_all_action.setCheckable(True)
        self.show_all_action.toggled.connect(self.toggle_show_all_devices)
        tools_menu.addAction(self.show_all_action)
        
        tools_menu.addSeparator()
        
        key_tester_action = QAction("&Key Tester", self)
        key_tester_action.triggered.connect(self.open_key_tester)
        tools_menu.addAction(key_tester_action)
        
        hid_console_action = QAction("&HID Console", self)
        hid_console_action.triggered.connect(self.open_hid_console)
        tools_menu.addAction(hid_console_action)
        
        help_menu = menubar.addMenu("&Help")
        about_action = QAction("&About", self)
        about_action.triggered.connect(self.show_about)
        help_menu.addAction(about_action)
        
        self.window_state.canFlashChanged.connect(self.flash_action.setEnabled)
        self.window_state.canResetChanged.connect(self.reset_action.setEnabled)
        self.window_state.canClearEepromChanged.connect(self.clear_eeprom_action.setEnabled)
    
    def create_file_group(self):
        group = QGroupBox("Local file")
        layout = QVBoxLayout()
        
        file_row = QHBoxLayout()
        self.filepath_combo = QComboBox()
        self.filepath_combo.setEditable(True)
        self.filepath_combo.setPlaceholderText("Click Open or drag to window to select file")
        file_row.addWidget(self.filepath_combo, 1)
        
        browse_button = QPushButton("Open")
        browse_button.clicked.connect(self.open_file)
        file_row.addWidget(browse_button)
        
        mcu_label = QLabel("MCU (AVR only):")
        file_row.addWidget(mcu_label)
        
        self.mcu_combo = QComboBox()
        self.load_mcu_list()
        file_row.addWidget(self.mcu_combo)
        
        layout.addLayout(file_row)
        group.setLayout(layout)
        return group
    
    def create_button_row(self):
        button_row = QHBoxLayout()
        button_row.addStretch()
        
        self.auto_flash_checkbox = QCheckBox("Auto-Flash")
        self.auto_flash_checkbox.toggled.connect(self.toggle_auto_flash)
        button_row.addWidget(self.auto_flash_checkbox)
        
        self.flash_button = QPushButton("Flash")
        self.flash_button.clicked.connect(self.flash_firmware)
        self.flash_button.setEnabled(False)
        button_row.addWidget(self.flash_button)
        
        self.reset_button = QPushButton("Exit DFU")
        self.reset_button.clicked.connect(self.reset_device)
        self.reset_button.setEnabled(False)
        button_row.addWidget(self.reset_button)
        
        self.clear_eeprom_button = QPushButton("Clear EEPROM")
        self.clear_eeprom_button.clicked.connect(self.clear_eeprom)
        self.clear_eeprom_button.setEnabled(False)
        button_row.addWidget(self.clear_eeprom_button)
        
        self.window_state.canFlashChanged.connect(self.flash_button.setEnabled)
        self.window_state.canResetChanged.connect(self.reset_button.setEnabled)
        self.window_state.canClearEepromChanged.connect(self.clear_eeprom_button.setEnabled)
        
        return button_row
    
    def load_mcu_list(self):
        mcu_file = Path(__file__).parent.parent.parent.parent.parent / "common" / "mcu-list.txt"
        try:
            with open(mcu_file, 'r') as f:
                mcus = [line.strip() for line in f if line.strip()]
                self.mcu_combo.addItems(mcus)
                default_mcu = "atmega32u4"
                index = self.mcu_combo.findText(default_mcu)
                if index >= 0:
                    self.mcu_combo.setCurrentIndex(index)
        except Exception as e:
            print(f"Error loading MCU list: {e}")
    
    def setup_listeners(self):
        self.usb_listener = UsbListener()
        self.usb_listener.usb_device_connected = self.on_usb_device_connected
        self.usb_listener.usb_device_disconnected = self.on_usb_device_disconnected
        self.usb_listener.bootloader_device_connected = self.on_bootloader_device_connected
        self.usb_listener.bootloader_device_disconnected = self.on_bootloader_device_disconnected
        self.usb_listener.output_received = self.on_bootloader_output
        
        try:
            self.usb_listener.start()
        except Exception as e:
            self.log_widget.logError("USB device enumeration failed.")
            self.log_widget.logError(str(e))
        
        self.hid_listener = HidListener()
        self.hid_listener.hid_device_connected = self.on_hid_device_connected
        self.hid_listener.hid_device_disconnected = self.on_hid_device_disconnected
        self.hid_listener.console_report_received = self.on_console_report
        self.hid_listener.start()
    
    def log_startup_info(self):
        from .. import __version__
        self.log_widget.logInfo(f"QMK Toolbox {__version__} (https://qmk.fm/toolbox)")
        self.log_widget.logInfo("Supported bootloaders:")
        self.log_widget.logInfo(" - ARM DFU (APM32, Kiibohd, STM32, STM32duino) and RISC-V DFU (GD32V) via dfu-util")
        self.log_widget.logInfo(" - Atmel/LUFA/QMK DFU via dfu-programmer")
        self.log_widget.logInfo(" - Atmel SAM-BA (Massdrop) via Massdrop Loader")
        self.log_widget.logInfo(" - BootloadHID (Atmel, PS2AVRGB) via bootloadHID")
        self.log_widget.logInfo(" - Caterina (Arduino, Pro Micro) via avrdude")
        self.log_widget.logInfo(" - HalfKay (Teensy, Ergodox EZ) via Teensy Loader")
        self.log_widget.logInfo(" - LUFA/QMK HID via hid_bootloader_cli")
        self.log_widget.logInfo(" - WB32 DFU via wb32-dfu-updater_cli")
        self.log_widget.logInfo(" - LUFA Mass Storage")
        self.log_widget.logInfo("Supported ISP flashers:")
        self.log_widget.logInfo(" - AVRISP (Arduino ISP)")
        self.log_widget.logInfo(" - USBasp (AVR ISP)")
        self.log_widget.logInfo(" - USBTiny (AVR Pocket)")
    
    @Slot()
    def open_file(self):
        filename, _ = QFileDialog.getOpenFileName(
            self,
            "Open Firmware File",
            "",
            "Intel Hex and Binary (*.hex *.bin);;Intel Hex (*.hex);;Binary (*.bin);;All Files (*)"
        )
        if filename:
            self.set_file_path(filename)
    
    def set_file_path(self, path):
        self.current_firmware_path = path
        self.filepath_combo.setCurrentText(path)
        
        index = self.filepath_combo.findText(path)
        if index < 0:
            self.filepath_combo.addItem(path)
        
        self.update_ui_state()
    
    @Slot()
    def flash_firmware(self):
        asyncio.create_task(self._flash_firmware_async())
    
    async def _flash_firmware_async(self):
        mcu = self.mcu_combo.currentText()
        file_path = self.current_firmware_path
        
        if not file_path:
            self.log_widget.logError("Please select a file")
            return
        
        if not os.path.isfile(file_path):
            self.log_widget.logError("File does not exist!")
            return
        
        bootloaders = self._find_bootloaders()
        if not bootloaders:
            self.log_widget.logError("No bootloader devices connected")
            return
        
        if not self.window_state.autoFlashEnabled:
            self._disable_ui()
        
        for bootloader in bootloaders:
            self.log_widget.logBootloader("Attempting to flash, please don't remove device")
            try:
                result = await bootloader.flash(mcu, file_path)
                if result == 0:
                    self.log_widget.logBootloader("Flash complete")
                else:
                    self.log_widget.logError(f"Flash failed with exit code {result}")
            except Exception as e:
                self.log_widget.logError(f"Flash error: {e}")
        
        if not self.window_state.autoFlashEnabled:
            self._enable_ui()
    
    @Slot()
    def reset_device(self):
        asyncio.create_task(self._reset_device_async())
    
    async def _reset_device_async(self):
        mcu = self.mcu_combo.currentText()
        
        bootloaders = self._find_bootloaders()
        if not bootloaders:
            self.log_widget.logError("No bootloader devices connected")
            return
        
        if not self.window_state.autoFlashEnabled:
            self._disable_ui()
        
        for bootloader in bootloaders:
            if bootloader.is_resettable:
                try:
                    result = await bootloader.reset(mcu)
                    if result == 0:
                        self.log_widget.logBootloader("Reset complete")
                    else:
                        self.log_widget.logError(f"Reset failed with exit code {result}")
                except NotImplementedError:
                    self.log_widget.logWarning(f"{bootloader.name} does not support reset")
                except Exception as e:
                    self.log_widget.logError(f"Reset error: {e}")
            else:
                self.log_widget.logWarning(f"{bootloader.name} does not support reset")
        
        if not self.window_state.autoFlashEnabled:
            self._enable_ui()
    
    @Slot()
    def clear_eeprom(self):
        asyncio.create_task(self._clear_eeprom_async())
    
    async def _clear_eeprom_async(self):
        mcu = self.mcu_combo.currentText()
        eeprom_file = self._get_eeprom_file("reset.eep")
        
        if not eeprom_file or not os.path.isfile(eeprom_file):
            self.log_widget.logError("EEPROM reset file not found (reset.eep)")
            return
        
        bootloaders = self._find_bootloaders()
        if not bootloaders:
            self.log_widget.logError("No bootloader devices connected")
            return
        
        if not self.window_state.autoFlashEnabled:
            self._disable_ui()
        
        for bootloader in bootloaders:
            if bootloader.is_eeprom_flashable:
                self.log_widget.logBootloader("Attempting to clear EEPROM, please don't remove device")
                try:
                    result = await bootloader.flash_eeprom(mcu, eeprom_file)
                    if result == 0:
                        self.log_widget.logBootloader("EEPROM clear complete")
                    else:
                        self.log_widget.logError(f"EEPROM clear failed with exit code {result}")
                except NotImplementedError:
                    self.log_widget.logWarning(f"{bootloader.name} does not support EEPROM flashing")
                except Exception as e:
                    self.log_widget.logError(f"EEPROM clear error: {e}")
            else:
                self.log_widget.logWarning(f"{bootloader.name} does not support EEPROM flashing")
        
        if not self.window_state.autoFlashEnabled:
            self._enable_ui()
    
    def _find_bootloaders(self):
        if not self.usb_listener:
            return []
        return [d for d in self.usb_listener.devices if isinstance(d, BootloaderDevice)]
    
    def _get_eeprom_file(self, filename):
        common_dir = Path(__file__).parent.parent.parent.parent.parent / "common"
        eeprom_path = common_dir / filename
        return str(eeprom_path) if eeprom_path.exists() else None
    
    def _disable_ui(self):
        self.flash_button.setEnabled(False)
        self.reset_button.setEnabled(False)
        self.clear_eeprom_button.setEnabled(False)
        self.flash_action.setEnabled(False)
        self.reset_action.setEnabled(False)
        self.clear_eeprom_action.setEnabled(False)
    
    def _enable_ui(self):
        self.update_ui_state()
    
    @Slot(bool)
    def toggle_auto_flash(self, checked):
        self.window_state.autoFlashEnabled = checked
        self.auto_flash_checkbox.setChecked(checked)
        self.auto_flash_action.setChecked(checked)
        self.log_widget.logInfo(f"Auto-Flash {'enabled' if checked else 'disabled'}")
        
        if checked:
            self._disable_ui()
        else:
            self._enable_ui()
    
    @Slot(bool)
    def toggle_show_all_devices(self, checked):
        self.window_state.showAllDevices = checked
        self.show_all_action.setChecked(checked)
    
    @Slot()
    def open_key_tester(self):
        if not hasattr(self, 'key_tester_window') or self.key_tester_window is None:
            from .key_tester_window import KeyTesterWindow
            self.key_tester_window = KeyTesterWindow(self)
        
        self.key_tester_window.show()
        self.key_tester_window.raise_()
        self.key_tester_window.activateWindow()
    
    @Slot()
    def open_hid_console(self):
        if not hasattr(self, 'hid_console_window') or self.hid_console_window is None:
            from .hid_console_window import HidConsoleWindow
            self.hid_console_window = HidConsoleWindow(self)
        
        self.hid_console_window.show()
        self.hid_console_window.raise_()
        self.hid_console_window.activateWindow()
    
    @Slot()
    def show_about(self):
        from .. import __version__
        QMessageBox.about(
            self,
            "About QMK Toolbox",
            f"QMK Toolbox {__version__}\n\n"
            "A keyboard firmware flashing utility\n\n"
            "https://qmk.fm/toolbox"
        )
    
    def on_usb_device_connected(self, device):
        if self.window_state.showAllDevices:
            self.log_widget.logInfo(f"USB device connected: {device}")
    
    def on_usb_device_disconnected(self, device):
        if self.window_state.showAllDevices:
            self.log_widget.logInfo(f"USB device disconnected: {device}")
    
    def on_bootloader_device_connected(self, device):
        self.log_widget.logBootloader(f"{device.name} device connected: {device}")
        self.update_ui_state()
        
        if self.window_state.autoFlashEnabled and self.current_firmware_path:
            self.flash_firmware()
    
    def on_bootloader_device_disconnected(self, device):
        self.log_widget.logBootloader(f"{device.name} device disconnected: {device}")
        self.update_ui_state()
    
    def on_bootloader_output(self, device, data, msg_type):
        self.log_widget.log(data, msg_type)
    
    def on_hid_device_connected(self, device):
        self.log_widget.logHid(f"HID console connected: {device}")
    
    def on_hid_device_disconnected(self, device):
        self.log_widget.logHid(f"HID console disconnected: {device}")
    
    def on_console_report(self, device, data):
        self.log_widget.logHid(data)
    
    def update_ui_state(self):
        has_bootloader = self.usb_listener and len(self.usb_listener.devices) > 0
        has_file = bool(self.current_firmware_path and os.path.isfile(self.current_firmware_path))
        
        self.window_state.canFlash = has_bootloader and has_file
        self.window_state.canReset = has_bootloader
        self.window_state.canClearEeprom = has_bootloader
    
    def load_settings(self):
        file_history = self.settings.value("fileHistory", [])
        if file_history:
            self.filepath_combo.addItems(file_history)
        
        auto_flash = self.settings.value("autoFlash", False, type=bool)
        self.auto_flash_checkbox.setChecked(auto_flash)
        self.auto_flash_action.setChecked(auto_flash)
        
        show_all = self.settings.value("showAllDevices", False, type=bool)
        self.show_all_action.setChecked(show_all)
        self.window_state.showAllDevices = show_all
        
        mcu = self.settings.value("mcu", "atmega32u4")
        index = self.mcu_combo.findText(mcu)
        if index >= 0:
            self.mcu_combo.setCurrentIndex(index)
    
    def save_settings(self):
        file_history = [self.filepath_combo.itemText(i) for i in range(self.filepath_combo.count())]
        self.settings.setValue("fileHistory", file_history[:10])
        self.settings.setValue("autoFlash", self.window_state.autoFlashEnabled)
        self.settings.setValue("showAllDevices", self.window_state.showAllDevices)
        self.settings.setValue("mcu", self.mcu_combo.currentText())
    
    def closeEvent(self, event):
        self.save_settings()
        
        if self.usb_listener:
            self.usb_listener.stop()
        if self.hid_listener:
            self.hid_listener.stop()
        
        event.accept()
