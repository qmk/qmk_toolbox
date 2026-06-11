#!/usr/bin/env bash
# make-win-installer.sh — Build the Windows installer using Inno Setup (Wine/Docker)
#
# Input:  publish-win-x64/qmk_toolbox.exe  (from publish-all.sh)
# Output: artifacts/qmk_toolbox_install.exe
#
# Usage:  ./scripts/make-win-installer.sh
# Deps:   Docker (pulls amake/innosetup on first run)

set -eEuo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"
ARTIFACTS_DIR="${REPO_ROOT}/artifacts"
PUBLISH_DIR="${REPO_ROOT}/publish-win-x64"

if [[ ! -f "${PUBLISH_DIR}/qmk_toolbox.exe" ]]; then
    echo "Error: ${PUBLISH_DIR}/qmk_toolbox.exe not found." >&2
    echo "Run ./scripts/publish-all.sh win-x64 first." >&2
    exit 1
fi

mkdir -p "${ARTIFACTS_DIR}"

echo "=== Building Windows installer (Inno Setup) ==="

# The amake/innosetup container runs as xclient (uid 1000). On GitHub Actions
# the runner uid differs, so bind-mounting a host directory for output fails
# with "Access denied" — the container can't write to a directory it doesn't own.
# Avoid bind-mounted output entirely: let Inno Setup write to the container's own
# filesystem, then extract the result with `docker cp`.
CONTAINER_NAME="qmk-innosetup-$$"
trap 'docker rm -f "${CONTAINER_NAME}" 2>/dev/null || true' EXIT

docker run --name "${CONTAINER_NAME}" \
    -v "${REPO_ROOT}:/work:ro" \
    --entrypoint /bin/sh \
    amake/innosetup \
    -c 'mkdir -p /home/xclient/output && \
        iscc "resources/windows-installer/qmk_toolbox_install.iss" \
        "/DSourceDir=Z:/work/publish-win-x64" \
        "/DOutputDir=Z:/home/xclient/output" \
        "/DIconFile=Z:/work/src/QmkToolbox.Desktop/Resources/output.ico"'

docker cp "${CONTAINER_NAME}:/home/xclient/output/qmk_toolbox_install.exe" "${ARTIFACTS_DIR}/"

echo "  Done: ${ARTIFACTS_DIR}/qmk_toolbox_install.exe"
