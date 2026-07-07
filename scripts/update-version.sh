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

CSPROJ="${REPO_ROOT}/windows/QMK Toolbox/QMK Toolbox.csproj"
INFO_PLIST="${REPO_ROOT}/macos/QMK Toolbox/Info.plist"
PKGPROJ="${REPO_ROOT}/macos/QMK Toolbox.pkgproj"
INSTALLER_ISS="${REPO_ROOT}/windows/install_compiler.iss"
README="${REPO_ROOT}/readme.md"

# Detect current version from the csproj's AssemblyVersion as the source of truth.
# (<Version> is ambiguous here -- the PackageReference children use it too.)
OLD="$(sed -n 's|.*<AssemblyVersion>\([0-9.]*\)</AssemblyVersion>.*|\1|p' "${CSPROJ}")"

if [[ "${OLD}" == "${NEW}" ]]; then
    echo "Already at version ${NEW}, nothing to do."
    exit 0
fi

echo "Updating version: ${OLD} -> ${NEW}"

# Windows app: AssemblyVersion / FileVersion / Version all track the release version.
sed -i \
    -e "s|<AssemblyVersion>${OLD}</AssemblyVersion>|<AssemblyVersion>${NEW}</AssemblyVersion>|" \
    -e "s|<FileVersion>${OLD}</FileVersion>|<FileVersion>${NEW}</FileVersion>|" \
    -e "s|<Version>${OLD}</Version>|<Version>${NEW}</Version>|" \
    "${CSPROJ}"

# macOS app: CFBundleShortVersionString and CFBundleVersion both carry the version.
sed -i "s|<string>${OLD}</string>|<string>${NEW}</string>|g" "${INFO_PLIST}"

# macOS installer: the Packages project's VERSION string.
sed -i "s|<string>${OLD}</string>|<string>${NEW}</string>|g" "${PKGPROJ}"

# Windows installer: the Inno Setup MyAppVersion define.
sed -i "s|#define MyAppVersion \"${OLD}\"|#define MyAppVersion \"${NEW}\"|" "${INSTALLER_ISS}"

# Readme: the "current version is **X.Y.Z**" line.
sed -i "s|\*\*${OLD}\*\*|**${NEW}**|" "${README}"

echo "Done. Files updated:"
echo "  windows/QMK Toolbox/QMK Toolbox.csproj"
echo "  macos/QMK Toolbox/Info.plist"
echo "  macos/QMK Toolbox.pkgproj"
echo "  windows/install_compiler.iss"
echo "  readme.md"
