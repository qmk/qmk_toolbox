#!/usr/bin/env bash
# run-tests.sh — Run the QmkToolbox test suite inside Docker.
#
# Usage:  ./scripts/run-tests.sh
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
    dotnet test QmkToolbox.Tests/QmkToolbox.Tests.csproj
