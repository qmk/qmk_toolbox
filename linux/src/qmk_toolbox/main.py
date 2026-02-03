#!/usr/bin/env python3
import sys
from PySide6.QtWidgets import QApplication
from PySide6.QtCore import Qt
from qmk_toolbox.ui.main_window import MainWindow
from qmk_toolbox import __version__


def main():
    app = QApplication(sys.argv)
    app.setApplicationName("QMK Toolbox")
    app.setOrganizationName("QMK")
    app.setOrganizationDomain("qmk.fm")
    app.setApplicationVersion(__version__)
    
    app.setAttribute(Qt.ApplicationAttribute.AA_UseHighDpiPixmaps)
    
    window = MainWindow()
    window.show()
    
    sys.exit(app.exec())


if __name__ == "__main__":
    main()
