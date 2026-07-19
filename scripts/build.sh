#!/usr/bin/env bash
# build.sh — Build the QmkToolbox solution inside Docker.
#
# Usage:  ./scripts/build.sh [extra dotnet-build args...]
#         e.g. ./scripts/build.sh --configuration Release   (as CI does)
# Deps:   Docker (mcr.microsoft.com/dotnet/sdk:10.0)

set -eEuo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

if [ "$(id -u)" -ne 0 ]; then
    DOCKER_RUN_USER="-u $(id -u):$(id -g)"
else
    DOCKER_RUN_USER=""
fi

cd "${REPO_ROOT}"
docker run --rm \
    ${DOCKER_RUN_USER} \
    -e HOME=/tmp \
    -v "${REPO_ROOT}":/app \
    -w /app/src \
    mcr.microsoft.com/dotnet/sdk:10.0 \
    dotnet build QmkToolbox.slnx "$@"
