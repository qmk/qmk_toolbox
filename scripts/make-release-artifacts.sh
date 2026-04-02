#!/usr/bin/env bash
# make-release-artifacts.sh — Assemble all release artifacts from existing publish-<rid>/ outputs.
#
# Expects publish-* directories to already exist (run publish-all.sh first).
# Clears and recreates artifacts/, then:
#   - Builds the macOS .app bundle, ZIP, and DMG  (make-macos-app.sh)
#   - Copies Linux and Windows executables
#   - Builds the Windows installer               (make-win-installer.sh)
#   - Builds the macOS .pkg installer            (make-macos-pkg.sh)
#
# Usage:  ./scripts/make-release-artifacts.sh
# Deps:   Docker (ghcr.io/tzarc/qmk_toolchains:builder, amake/innosetup)
set -eEuo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

# Copy out executables from publish-* to artifacts/
ARTIFACTS_DIR="${REPO_ROOT}/artifacts"
rm -rf "${ARTIFACTS_DIR}"
mkdir -p "${ARTIFACTS_DIR}"

# Make the macOS app bundle and DMG
"${REPO_ROOT}/scripts/make-macos-app.sh"

cp "${REPO_ROOT}/publish-linux-x64/qmk_toolbox"    "${ARTIFACTS_DIR}/qmk_toolbox-linux-x64"
cp "${REPO_ROOT}/publish-linux-arm64/qmk_toolbox"  "${ARTIFACTS_DIR}/qmk_toolbox-linux-arm64"
cp "${REPO_ROOT}/publish-win-x64/qmk_toolbox.exe"  "${ARTIFACTS_DIR}/qmk_toolbox.exe"

# Windows installer
"${REPO_ROOT}/scripts/make-win-installer.sh"

# macOS .pkg installer
"${REPO_ROOT}/scripts/make-macos-pkg.sh"
