#!/bin/bash
set -e

cd "$(dirname "$0")/../.."

echo "Building QMK Toolbox Debian package..."

pip install --upgrade build
python -m build

VERSION=$(python -c "from qmk_toolbox import __version__; print(__version__)")

mkdir -p debian/DEBIAN
mkdir -p debian/usr/bin
mkdir -p debian/usr/share/applications
mkdir -p debian/usr/share/icons/hicolor/scalable/apps
mkdir -p debian/usr/share/udev/rules.d

cat > debian/DEBIAN/control << EOF
Package: qmk-toolbox
Version: ${VERSION}
Section: utils
Priority: optional
Architecture: all
Depends: python3 (>= 3.8), python3-pyside6.qtwidgets, python3-pyudev, python3-hid, python3-usb, dfu-util, dfu-programmer, avrdude
Maintainer: QMK <hello@qmk.fm>
Description: A keyboard firmware flashing utility
 QMK Toolbox is a collection of flashing tools packaged into one app.
 It supports auto-detection and auto-flashing of firmware to keyboards
 running QMK Firmware.
Homepage: https://qmk.fm/toolbox
EOF

pip install --target=debian/usr/lib/python3/dist-packages .

ln -sf ../lib/python3/dist-packages/qmk_toolbox/main.py debian/usr/bin/qmk-toolbox
chmod +x debian/usr/bin/qmk-toolbox

cp resources/qmk-toolbox.desktop debian/usr/share/applications/
cp resources/icons/qmk-toolbox.svg debian/usr/share/icons/hicolor/scalable/apps/
cp packaging/99-qmk.rules debian/usr/share/udev/rules.d/

dpkg-deb --build debian qmk-toolbox_${VERSION}_all.deb

echo "Package built: qmk-toolbox_${VERSION}_all.deb"
