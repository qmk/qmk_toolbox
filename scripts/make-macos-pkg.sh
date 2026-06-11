#!/usr/bin/env bash
# make-macos-pkg.sh — Build a macOS product-archive .pkg from the existing .app bundle
#
# Input:  publish-osx-dmg/QMK Toolbox.app  (from make-macos-app.sh)
# Output: artifacts/QMK Toolbox.pkg
#
# Usage:  ./scripts/make-macos-pkg.sh
# Deps:   Docker (ghcr.io/tzarc/qmk_toolchains:builder)

set -eEuo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"
ARTIFACTS_DIR="${REPO_ROOT}/artifacts"
APP_SRC="${REPO_ROOT}/publish-osx-dmg/QMK Toolbox.app"

if [[ ! -d "${APP_SRC}" ]]; then
    echo "Error: .app bundle not found: ${APP_SRC}" >&2
    echo "Run ./scripts/make-macos-app.sh first." >&2
    exit 1
fi

mkdir -p "${ARTIFACTS_DIR}"

VERSION="$(grep -A1 'CFBundleShortVersionString' \
    "${APP_SRC}/Contents/Info.plist" \
    | grep '<string>' | sed 's/.*<string>\(.*\)<\/string>.*/\1/')"
BUNDLE_ID="fm.qmk.toolbox"

echo "=== Building macOS .pkg (${BUNDLE_ID} ${VERSION}) ==="

docker run --rm -i \
    -e VERSION="${VERSION}" \
    -e BUNDLE_ID="${BUNDLE_ID}" \
    -e HOST_UID="$(id -u)" \
    -e HOST_GID="$(id -g)" \
    -v "${REPO_ROOT}:/work" \
    ghcr.io/tzarc/qmk_toolchains:builder \
    bash << 'PKGSCRIPT'
set -euo pipefail

WORK=/work
BUILD="${WORK}/_pkg_build"
rm -rf "${BUILD}"

mkdir -p "${BUILD}/root/Applications"
cp -R "${WORK}/publish-osx-dmg/QMK Toolbox.app" "${BUILD}/root/Applications/QMK Toolbox.app"

mkdir -p "${BUILD}/component"
cd "${BUILD}/root"

# uid=0 (root), gid=80 (staff on macOS)
mkbom -u 0 -g 80 . "${BUILD}/component/Bom"

# Payload: odc (POSIX.1 portable) cpio, gzip-compressed
find . | cpio -o --format odc --owner 0:80 | gzip -c > "${BUILD}/component/Payload"

NUM_FILES=$(find "${BUILD}/root" -not -type d | wc -l)
INSTALL_KB=$(du -sk "${BUILD}/root" | cut -f1)

cat > "${BUILD}/component/PackageInfo" << PKGINFO
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<pkg-info format-version="2" identifier="${BUNDLE_ID}.component" version="${VERSION}" install-location="/" auth="root">
    <payload numberOfFiles="${NUM_FILES}" installKBytes="${INSTALL_KB}"/>
    <bundle-version>
        <bundle id="${BUNDLE_ID}" CFBundleIdentifier="${BUNDLE_ID}"
                path="./Applications/QMK Toolbox.app" CFBundleVersion="${VERSION}"/>
    </bundle-version>
</pkg-info>
PKGINFO

# The component package must be a DIRECTORY inside the outer xar, not a
# nested xar file — macOS Installer reads Payload/Bom/PackageInfo directly.
COMP_DIR="${BUILD}/product/${BUNDLE_ID}.component.pkg"
mkdir -p "${COMP_DIR}"
cp "${BUILD}/component/Bom"         "${COMP_DIR}/Bom"
cp "${BUILD}/component/Payload"     "${COMP_DIR}/Payload"
cp "${BUILD}/component/PackageInfo" "${COMP_DIR}/PackageInfo"

cat > "${BUILD}/product/Distribution" << DIST
<?xml version="1.0" encoding="utf-8"?>
<installer-gui-script minSpecVersion="2">
    <title>QMK Toolbox</title>
    <choices-outline>
        <line choice="default"/>
    </choices-outline>
    <choice id="default" title="QMK Toolbox" description="Install QMK Toolbox to Applications.">
        <pkg-ref id="${BUNDLE_ID}.component"/>
    </choice>
    <pkg-ref id="${BUNDLE_ID}.component" version="${VERSION}" installKBytes="${INSTALL_KB}">#${BUNDLE_ID}.component.pkg</pkg-ref>
    <allowed-os-versions>
        <os-version min="13.0"/>
    </allowed-os-versions>
    <domains enable_currentUserHome="false" enable_localSystem="true"/>
    <options customize="never" require-scripts="false"/>
    <product id="${BUNDLE_ID}" version="${VERSION}"/>
</installer-gui-script>
DIST

cd "${BUILD}/product"
xar -c --compression none -f "/work/artifacts/QMK Toolbox.pkg" \
    Distribution "${BUNDLE_ID}.component.pkg"

rm -rf "${BUILD}"
chown "${HOST_UID}:${HOST_GID}" "/work/artifacts/QMK Toolbox.pkg"
echo "Done."
PKGSCRIPT

echo "  Done: ${ARTIFACTS_DIR}/QMK Toolbox.pkg"
