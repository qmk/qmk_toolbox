#!/usr/bin/env bash
# run-tests.sh — Run the QmkToolbox test suite inside Docker.
#
# Coverage (cobertura) is collected via coverlet using src/coverage.runsettings and
# rendered with ReportGenerator into coveragereport/ at the repo root (gitignored):
# a browsable HTML report, a GitHub-flavoured markdown summary (consumed by CI for
# the job summary), and a text summary printed at the end of the run.
#
# Usage:  ./scripts/run-tests.sh [extra dotnet-test args...]
#         e.g. ./scripts/run-tests.sh --configuration Release   (as CI does)
# Deps:   Docker (mcr.microsoft.com/dotnet/sdk:10.0)

set -eEuo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"
REPORTGENERATOR_VERSION="5.5.10"

if [ "$(id -u)" -ne 0 ]; then
    DOCKER_RUN_USER="-u $(id -u):$(id -g)"
else
    DOCKER_RUN_USER=""
fi

cd "${REPO_ROOT}"
rm -rf "${REPO_ROOT}/TestResults" "${REPO_ROOT}/coveragereport"
docker run --rm \
    ${DOCKER_RUN_USER} \
    -e HOME=/tmp \
    -e REPORTGENERATOR_VERSION="${REPORTGENERATOR_VERSION}" \
    -v "${REPO_ROOT}":/app \
    -w /app/src \
    mcr.microsoft.com/dotnet/sdk:10.0 \
    sh -c '
        dotnet test QmkToolbox.Tests/QmkToolbox.Tests.csproj \
            --collect:"XPlat Code Coverage" \
            --settings coverage.runsettings \
            --results-directory /app/TestResults \
            "$@"
        dotnet tool install --tool-path /tmp/rg dotnet-reportgenerator-globaltool \
            --version "${REPORTGENERATOR_VERSION}" >/dev/null
        /tmp/rg/reportgenerator \
            "-reports:/app/TestResults/*/coverage.cobertura.xml" \
            -targetdir:/app/coveragereport \
            "-reporttypes:HtmlInline;MarkdownSummaryGithub;TextSummary" >/dev/null
    ' sh "$@"

echo
cat "${REPO_ROOT}/coveragereport/Summary.txt"
echo
echo "HTML report: ${REPO_ROOT}/coveragereport/index.html"
