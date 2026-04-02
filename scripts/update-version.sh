#!/usr/bin/env bash
# update-version.sh — Update the app version across all relevant source files.
#
# Usage: ./scripts/update-version.sh <new-version>
# Example: ./scripts/update-version.sh 0.9.9

set -eEuo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $(basename "$0") <new-version>" >&2
    exit 1
fi

NEW="$1"

if [[ ! "${NEW}" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    echo "Error: version must be in X.Y.Z format, got: ${NEW}" >&2
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

# Detect current version from the csproj as the source of truth
CSPROJ="${REPO_ROOT}/src/QmkToolbox.Desktop/QmkToolbox.Desktop.csproj"
OLD="$(grep '<Version>' "${CSPROJ}" | sed 's/.*<Version>\([^<]*\)<.*/\1/')"

if [[ "${OLD}" == "${NEW}" ]]; then
    echo "Already at version ${NEW}, nothing to do."
    exit 0
fi

echo "Updating version: ${OLD} -> ${NEW}"

sed -i "s|<Version>${OLD}</Version>|<Version>${NEW}</Version>|" \
    "${CSPROJ}"

sed -i "s|<string>${OLD}</string>|<string>${NEW}</string>|g" \
    "${REPO_ROOT}/resources/macos-app-support/Info.plist"

sed -i "s|#define MyAppVersion   \"${OLD}\"|#define MyAppVersion   \"${NEW}\"|" \
    "${REPO_ROOT}/resources/windows-installer/qmk_toolbox_install.iss"

echo "Done. Files updated:"
echo "  src/QmkToolbox.Desktop/QmkToolbox.Desktop.csproj"
echo "  resources/macos-app-support/Info.plist"
echo "  resources/windows-installer/qmk_toolbox_install.iss"
