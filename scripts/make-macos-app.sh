#!/usr/bin/env bash
# make-macos-app.sh — Wrap a dotnet publish output into a macOS .app bundle,
# then produce a ZIP archive and DMG from it.
#
# Usage:  scripts/make-macos-app.sh [osx-arm64|osx-x64|osx-universal]   (default: osx-universal)
# Input:  publish-<rid>/  (created by publish-all.sh or dotnet publish -r <rid> -c Release)
# Output: publish-osx-dmg/QMK Toolbox.app
#         artifacts/QMK Toolbox.app.zip
#         artifacts/QMK Toolbox.dmg
# Deps:   Docker (ghcr.io/tzarc/qmk_toolchains:builder)

set -eEuo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

RID="${1:-osx-universal}"
ARTIFACTS_DIR="${REPO_ROOT}/artifacts"
PUBLISH_DIR="${REPO_ROOT}/publish-${RID}"
APP_DIR="${REPO_ROOT}/publish-osx-dmg/QMK Toolbox.app"
CONTENTS="${APP_DIR}/Contents"
MACOS_DIR="${CONTENTS}/MacOS"
RESOURCES_DIR="${CONTENTS}/Resources"
if [[ ! -d "${PUBLISH_DIR}" ]]; then
    echo "Error: publish directory not found: ${PUBLISH_DIR}" >&2
    echo "Run publish-all.sh (or dotnet publish -r ${RID}) first." >&2
    exit 1
fi
if [[ ! -d "${ARTIFACTS_DIR}" ]]; then
    mkdir -p "${ARTIFACTS_DIR}"
fi

echo "Building QMK Toolbox.app from ${PUBLISH_DIR} ..."

rm -rf "${APP_DIR}"
mkdir -p "${MACOS_DIR}" "${RESOURCES_DIR}"

cp -R "${PUBLISH_DIR}/." "${MACOS_DIR}/"
chmod +x "${MACOS_DIR}/qmk_toolbox"
# Make any extracted native dylibs executable too
find "${MACOS_DIR}" -name "*.dylib" -exec chmod +x {} \;

cp "${REPO_ROOT}/resources/macos-app-support/Info.plist" "${CONTENTS}/Info.plist"
cp "${REPO_ROOT}/resources/macos-app-support/AppIcon.icns" "${RESOURCES_DIR}/AppIcon.icns"

if [[ "$(uname -s)" == "Darwin" ]]; then
    xattr -cr "${APP_DIR}"
    echo "  Quarantine attributes removed."
fi

echo ""
echo "Done: ${APP_DIR}"
echo "Launch with:  open \"${APP_DIR}\""

# Make the ZIP and DMG in a container to avoid needing genisoimage and zip on the host.
# The -i flag is required so Docker forwards the heredoc to bash's stdin.
docker run --rm -i \
    -v "${REPO_ROOT}:/app" \
    -w /app \
    -e TC_WORKDIR=/app \
    ghcr.io/tzarc/qmk_toolchains:builder \
    bash << 'ZIPDMG'
set -euo pipefail
cd publish-osx-dmg && zip -r "../artifacts/QMK Toolbox.app.zip" "QMK Toolbox.app" && cd ..
genisoimage -V "QMK Toolbox" -D -R -apple -no-pad -o "artifacts/QMK Toolbox.dmg" publish-osx-dmg
ZIPDMG
