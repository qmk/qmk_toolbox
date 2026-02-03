#!/bin/bash
set -e

cd "$(dirname "$0")/../.."

echo "Building QMK Toolbox AppImage..."

VERSION=$(python -c "from src/qmk_toolbox import __version__; print(__version__)")

mkdir -p AppDir/usr/bin
mkdir -p AppDir/usr/share/applications
mkdir -p AppDir/usr/share/icons/hicolor/scalable/apps
mkdir -p AppDir/usr/lib/python3/dist-packages

pip install --target=AppDir/usr/lib/python3/dist-packages .

cat > AppDir/usr/bin/qmk-toolbox << 'EOF'
#!/bin/sh
HERE="$(dirname "$(readlink -f "${0}")")"
export PYTHONPATH="${HERE}/../lib/python3/dist-packages:${PYTHONPATH}"
exec python3 -m qmk_toolbox.main "$@"
EOF

chmod +x AppDir/usr/bin/qmk-toolbox

cp resources/qmk-toolbox.desktop AppDir/
cp resources/icons/qmk-toolbox.svg AppDir/
cp resources/qmk-toolbox.desktop AppDir/usr/share/applications/
cp resources/icons/qmk-toolbox.svg AppDir/usr/share/icons/hicolor/scalable/apps/

cat > AppDir/AppRun << 'EOF'
#!/bin/sh
HERE="$(dirname "$(readlink -f "${0}")")"
export PATH="${HERE}/usr/bin:${PATH}"
export LD_LIBRARY_PATH="${HERE}/usr/lib:${LD_LIBRARY_PATH}"
export PYTHONPATH="${HERE}/usr/lib/python3/dist-packages:${PYTHONPATH}"
exec "${HERE}/usr/bin/qmk-toolbox" "$@"
EOF

chmod +x AppDir/AppRun

wget -q https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
chmod +x appimagetool-x86_64.AppImage

./appimagetool-x86_64.AppImage AppDir qmk-toolbox-${VERSION}-x86_64.AppImage

echo "AppImage built: qmk-toolbox-${VERSION}-x86_64.AppImage"
