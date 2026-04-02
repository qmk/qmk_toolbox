#!/usr/bin/env bash
# publish-all.sh — Run dotnet publish for every supported platform RID.
#
# When all five platforms are built (the default), also runs lipo to produce
# the osx-universal binary from the osx-x64 and osx-arm64 outputs.
# Pass one or more RIDs to build only those targets (skips the lipo step).
#
# Usage:  ./scripts/publish-all.sh [RID...]
# Example: ./scripts/publish-all.sh linux-x64 win-x64
# Deps:   Docker (mcr.microsoft.com/dotnet/sdk:10.0, ghcr.io/tzarc/qmk_toolchains:builder)

set -eEuo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"
ALL_BUILD_TARGETS="linux-x64 linux-arm64 osx-x64 osx-arm64 win-x64"
REQUESTED_BUILD_TARGETS="${@:-$ALL_BUILD_TARGETS}"

# If we're not root, then use -u to run the container with the same UID and GID as the current user, so that the generated files are owned by the current user instead of root.
if [ "$(id -u)" -ne 0 ]; then
    DOCKER_RUN_USER="-u $(id -u):$(id -g)"
else
    DOCKER_RUN_USER=""
fi

cd "${REPO_ROOT}"
rm -rf "${REPO_ROOT}/"publish-* "${REPO_ROOT}/artifacts"
for RID in $REQUESTED_BUILD_TARGETS; do
    docker run --rm \
        ${DOCKER_RUN_USER} \
        -e HOME=/tmp \
        -v "${REPO_ROOT}":/app \
        -w /app/src/QmkToolbox.Desktop \
        mcr.microsoft.com/dotnet/sdk:10.0 \
        dotnet publish -o ../../publish-${RID} -r ${RID} -c Release
done

# If specific targets were requested, skip the macOS universal lipo step — the
# caller is doing a single-platform build and may not have both osx-x64 and
# osx-arm64 outputs available.
if [ -n "${1:-}" ]; then
    exit 0
fi

# The publish output is a single self-contained executable (PublishSingleFile=true),
# so we only need to lipo the one binary to create the universal build.
mkdir -p "${REPO_ROOT}/publish-osx-universal"
docker run --rm \
    -v "${REPO_ROOT}":/app \
    -e TC_WORKDIR=/app \
    -w /app \
    ghcr.io/tzarc/qmk_toolchains:builder \
    aarch64-apple-darwin24-lipo -create \
        publish-osx-x64/qmk_toolbox \
        publish-osx-arm64/qmk_toolbox \
        -output publish-osx-universal/qmk_toolbox
