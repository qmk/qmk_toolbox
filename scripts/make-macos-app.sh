#!/usr/bin/env bash
# make-macos-app.sh — Wrap a dotnet publish output into a macOS .app bundle and
# zip it. The ZIP is produced here (alongside the bundle build).
#
# When signing, the bundle is code-signed in place *after* it is built, so the
# ZIP must be regenerated from the signed bundle. Pass --repack to skip building
# and only (re)create the ZIP from the existing publish-osx-dmg/QMK Toolbox.app.
#
# Usage:  scripts/make-macos-app.sh [osx-arm64|osx-x64|osx-universal]   (default: osx-universal)
#         scripts/make-macos-app.sh --repack        # re-zip the existing (signed) bundle
# Input:  publish-<rid>/  (created by publish-all.sh or dotnet publish -r <rid> -c Release)
# Output: publish-osx-dmg/QMK Toolbox.app
#         artifacts/QMK Toolbox.app.zip
# Deps:   Docker (ghcr.io/tzarc/qmk_toolchains:builder)  [for the zip]

set -eEuo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

REPACK=0
if [[ "${1:-}" == "--repack" ]]; then
    REPACK=1
    shift
fi

RID="${1:-osx-universal}"
ARTIFACTS_DIR="${REPO_ROOT}/artifacts"
PUBLISH_DIR="${REPO_ROOT}/publish-${RID}"
APP_DIR="${REPO_ROOT}/publish-osx-dmg/QMK Toolbox.app"
CONTENTS="${APP_DIR}/Contents"
MACOS_DIR="${CONTENTS}/MacOS"
RESOURCES_DIR="${CONTENTS}/Resources"

if [[ "${REPACK}" -eq 0 ]]; then
    if [[ ! -d "${PUBLISH_DIR}" ]]; then
        echo "Error: publish directory not found: ${PUBLISH_DIR}" >&2
        echo "Run publish-all.sh (or dotnet publish -r ${RID}) first." >&2
        exit 1
    fi

    echo "Building QMK Toolbox.app from ${PUBLISH_DIR} ..."

    rm -rf "${APP_DIR}"
    mkdir -p "${MACOS_DIR}" "${RESOURCES_DIR}"

    cp -R "${PUBLISH_DIR}/." "${MACOS_DIR}/"
    chmod +x "${MACOS_DIR}/qmk_toolbox"
    # Make any extracted native dylibs executable too
    find "${MACOS_DIR}" -name "*.dylib" -exec chmod +x {} +

    cp "${REPO_ROOT}/resources/macos-app-support/Info.plist" "${CONTENTS}/Info.plist"
    cp "${REPO_ROOT}/resources/macos-app-support/AppIcon.icns" "${RESOURCES_DIR}/AppIcon.icns"

    if [[ "$(uname -s)" == "Darwin" ]]; then
        xattr -cr "${APP_DIR}"
        echo "  Quarantine attributes removed."
    fi

    echo "Done: ${APP_DIR}"
else
    if [[ ! -d "${APP_DIR}" ]]; then
        echo "Error: .app bundle not found: ${APP_DIR}" >&2
        echo "Run ./scripts/make-macos-app.sh (without --repack) first." >&2
        exit 1
    fi
    echo "Repacking existing bundle: ${APP_DIR}"
fi

mkdir -p "${ARTIFACTS_DIR}"

# Zip inside the container to avoid needing zip on the host. The -i flag is
# required so Docker forwards the heredoc to bash's stdin. rm first so the ZIP is
# rebuilt rather than updated in place (matters when re-zipping a signed bundle).
rm -f "${ARTIFACTS_DIR}/QMK Toolbox.app.zip"
docker run --rm -i \
    -v "${REPO_ROOT}:/app" \
    -w /app \
    -e TC_WORKDIR=/app \
    ghcr.io/tzarc/qmk_toolchains:builder \
    bash << 'ZIP'
set -euo pipefail
cd publish-osx-dmg && zip -r "../artifacts/QMK Toolbox.app.zip" "QMK Toolbox.app"
ZIP

echo "  Done: ${ARTIFACTS_DIR}/QMK Toolbox.app.zip"
