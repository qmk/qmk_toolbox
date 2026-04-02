#!/usr/bin/env bash
# fetch-tools.sh — Download release binaries from four upstream repositories and
# place them in the repository's resources/ tree:
#
#   qmk_flashutils        flash tool binaries (avrdude, dfu-util, etc.) for all platforms
#   qmk_hidapi            hidapi native library for all platforms
#   qmk_driver_installer  WinUSB driver installer (Windows only)
#   qmk_udev              udev rules + qmk_id helper binary (Linux only)
#
# All outputs are committed to version control so that builds and CI require no
# network access.  Run this script whenever upstream tools need to be updated.
#
# Usage:  ./scripts/fetch-tools.sh
# Deps:   curl, jq, zstd, tar

set -eEuo pipefail

for cmd in curl jq zstd tar; do
    if ! command -v "${cmd}" >/dev/null 2>&1; then
        echo "Error: required command '${cmd}' not found." >&2
        exit 1
    fi
done

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

# Map: resource directory name -> qmk_flashutils platform tag
# Both macOS architectures share single universal binaries.
declare -A PLATFORMS=(
    ["linux-x64"]="linuxX64"
    ["linux-arm64"]="linuxARM64"
    ["osx"]="macosUNIVERSAL"
    ["win-x64"]="windowsX64"
)

# Name that HidApi.Net 1.x searches for on each platform
# (see badcel/HidApi.Net NativeHidApiLibrary.cs)
declare -A HIDAPI_NAMES=(
    ["linux-x64"]="libhidapi-hidraw.so.0"
    ["linux-arm64"]="libhidapi-hidraw.so.0"
    ["osx"]="libhidapi.dylib"
    ["win-x64"]="hidapi.dll"
)

TOOLS_ROOT="${REPO_ROOT}/resources/flashutils"
HIDAPI_ROOT="${REPO_ROOT}/resources/hidapi"

# Local download cache to avoid re-downloading the same archive more than once.
CACHE_DIR="$(mktemp -d)"
trap 'rm -rf "${CACHE_DIR}"' EXIT

CURL_OPTS=(-fsSL)
if ! curl "${CURL_OPTS[@]}" --head "https://github.com" >/dev/null 2>&1; then
    echo "Error: TLS connection to github.com failed." >&2
    echo "Fix your CA certificates or set CURL_CA_BUNDLE to your CA bundle path." >&2
    exit 1
fi

FLASHUTILS_TAG="$(curl "${CURL_OPTS[@]}" "https://api.github.com/repos/qmk/qmk_flashutils/releases/latest" | jq -r '.tag_name')"
echo "qmk_flashutils release: ${FLASHUTILS_TAG}"
BASE_URL="https://github.com/qmk/qmk_flashutils/releases/download/${FLASHUTILS_TAG}"

fetch_archive() {
    local name="$1"
    local cached="${CACHE_DIR}/${name}"
    if [[ ! -f "${cached}" ]]; then
        echo "  Downloading ${name}..." >&2
        curl "${CURL_OPTS[@]}" -o "${cached}" "${BASE_URL}/${name}" >&2
    else
        echo "  Using cached ${name}" >&2
    fi
    echo "${cached}"
}

for RID in "${!PLATFORMS[@]}"; do
    PLATFORM="${PLATFORMS[$RID]}"
    HIDAPI_NAME="${HIDAPI_NAMES[$RID]}"

    echo "=== ${RID} (qmk platform: ${PLATFORM}) ==="

    TOOLS_DIR="${TOOLS_ROOT}/${RID}"
    HIDAPI_DIR="${HIDAPI_ROOT}/${RID}"
    mkdir -p "${TOOLS_DIR}" "${HIDAPI_DIR}"

    echo "  qmk_flashutils-${FLASHUTILS_TAG}-${PLATFORM}.tar.zst -> ${TOOLS_DIR}"
    TOOLS_ARCHIVE="$(fetch_archive "qmk_flashutils-${FLASHUTILS_TAG}-${PLATFORM}.tar.zst")"
    zstd -d --stdout "${TOOLS_ARCHIVE}" \
        | tar -x --strip-components=1 -C "${TOOLS_DIR}"

    # Remove tools that are not needed by QMK Toolbox
    rm -f "${TOOLS_DIR}"/dfu-prefix "${TOOLS_DIR}"/dfu-prefix.exe \
          "${TOOLS_DIR}"/dfu-suffix "${TOOLS_DIR}"/dfu-suffix.exe

    # hidapi native library — extract to a temp dir then rename to the name
    # HidApi.Net 1.x actually searches for on this platform.
    echo "  qmk_hidapi-${FLASHUTILS_TAG}-${PLATFORM}.tar.zst -> ${HIDAPI_DIR}"
    HIDAPI_ARCHIVE="$(fetch_archive "qmk_hidapi-${FLASHUTILS_TAG}-${PLATFORM}.tar.zst")"
    HIDAPI_TMP="$(mktemp -d)"
    zstd -d --stdout "${HIDAPI_ARCHIVE}" \
        | tar -x --strip-components=1 -C "${HIDAPI_TMP}"

    # Move the library (whatever it was named in the archive) to the expected name
    HIDAPI_SRC="$(find "${HIDAPI_TMP}" -maxdepth 1 -type f \( \
        -name '*.so' -o -name '*.so.*' -o -name '*.dylib' -o -name '*.dll' \
    \) | head -1)"

    if [[ -z "${HIDAPI_SRC}" ]]; then
        echo "  ERROR: no library file found in qmk_hidapi-${PLATFORM} archive" >&2
        rm -rf "${HIDAPI_TMP}"
        exit 1
    fi

    cp "${HIDAPI_SRC}" "${HIDAPI_DIR}/${HIDAPI_NAME}"

    # Also copy the release manifest (hidapi_release_<platform>) if present
    HIDAPI_MANIFEST="$(find "${HIDAPI_TMP}" -maxdepth 1 -name 'hidapi_release_*' | head -1)"
    if [[ -n "${HIDAPI_MANIFEST}" ]]; then
        cp "${HIDAPI_MANIFEST}" "${HIDAPI_DIR}/$(basename "${HIDAPI_MANIFEST}")"
    fi

    rm -rf "${HIDAPI_TMP}"

    if [[ "${RID}" != win-* ]]; then
        chmod +x "${TOOLS_DIR}"/* 2>/dev/null || true
    fi

    echo "  Done."
done

# ── Windows-only: qmk_driver_installer ───────────────────────────────────────
# Download from the latest GitHub release into resources/windows-drivers/ so it
# gets embedded as a resource for Windows builds.
DRIVER_INSTALLER_ROOT="${REPO_ROOT}/resources/windows-drivers"
mkdir -p "${DRIVER_INSTALLER_ROOT}"

echo ""
echo "=== qmk_driver_installer (win-x64 only) ==="

DRIVER_INSTALLER_REPO="https://github.com/qmk/qmk_driver_installer"
DRIVER_INSTALLER_URL="${DRIVER_INSTALLER_REPO}/releases/latest/download/qmk_driver_installer.exe"
DRIVER_INSTALLER_DEST="${DRIVER_INSTALLER_ROOT}/qmk_driver_installer.exe"

echo "  Downloading qmk_driver_installer.exe..."
curl "${CURL_OPTS[@]}" -L -o "${DRIVER_INSTALLER_DEST}" "${DRIVER_INSTALLER_URL}"
echo "  Saved to ${DRIVER_INSTALLER_DEST}"
echo "  Done."

# ── Linux-only: qmk_udev (qmk_id helper + udev rules) ───────────────────────
declare -A UDEV_PLATFORMS=(
    ["linux-x64"]="linuxX64"
    ["linux-arm64"]="linuxARM64"
)

UDEV_ROOT="${REPO_ROOT}/resources/udev"
mkdir -p "${UDEV_ROOT}"

echo ""
echo "=== qmk_udev ==="

UDEV_TAG="$(curl "${CURL_OPTS[@]}" "https://api.github.com/repos/qmk/qmk_udev/releases/latest" | jq -r '.tag_name')"
echo "qmk_udev release: ${UDEV_TAG}"
UDEV_BASE_URL="https://github.com/qmk/qmk_udev/releases/download/${UDEV_TAG}"

echo "  Downloading 50-qmk.rules..."
curl "${CURL_OPTS[@]}" -o "${UDEV_ROOT}/50-qmk.rules" "${UDEV_BASE_URL}/50-qmk.rules"

for RID in "${!UDEV_PLATFORMS[@]}"; do
    PLATFORM="${UDEV_PLATFORMS[$RID]}"
    UDEV_DIR="${UDEV_ROOT}/${RID}"
    mkdir -p "${UDEV_DIR}"
    echo "  Downloading qmk_id-${PLATFORM} -> ${UDEV_DIR}/qmk_id"
    curl "${CURL_OPTS[@]}" -o "${UDEV_DIR}/qmk_id" "${UDEV_BASE_URL}/qmk_id-${PLATFORM}"
    chmod +x "${UDEV_DIR}/qmk_id"

    # Per-arch release manifest for version-checking at runtime.
    cat > "${UDEV_DIR}/qmk_udev_release_${PLATFORM}" <<MANIFEST
COMMIT_DATE=${UDEV_TAG}
COMMIT_HASH=${UDEV_TAG}
MANIFEST
done

echo "  Done."

echo ""
echo "All platforms fetched into resources/."
echo "Commit the result to update the bundled binaries."
