#!/usr/bin/env bash
# check-deps.sh — Report outdated NuGet packages and dotnet tools, or upgrade them.
#
# Usage:
#   ./scripts/check-deps.sh            # check only
#   ./scripts/check-deps.sh --upgrade  # update Directory.Packages.props and dotnet-tools.json in place

set -euo pipefail

UPGRADE=false
for arg in "$@"; do
    case "$arg" in
        -u|--upgrade) UPGRADE=true ;;
        *) echo "Unknown argument: $arg" >&2; exit 1 ;;
    esac
done

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

cd "$REPO_ROOT"
dotnet tool restore

# ── NuGet packages ────────────────────────────────────────────────────────────
echo "=== NuGet packages ==="
if $UPGRADE; then
    dotnet tool run dotnet-outdated src/QmkToolbox.slnx --upgrade
else
    dotnet tool run dotnet-outdated src/QmkToolbox.slnx
fi

# ── dotnet tools ──────────────────────────────────────────────────────────────
echo ""
echo "=== dotnet tools ==="

TOOLS_MANIFEST="$REPO_ROOT/.config/dotnet-tools.json"
NUGET_API="https://api.nuget.org/v3-flatcontainer"

# latest_stable <package-id> — prints the highest non-prerelease version
latest_stable() {
    local id="${1,,}"  # NuGet IDs are case-insensitive; API wants lowercase
    curl -fsSL "$NUGET_API/$id/index.json" \
        | jq -r '[.versions[] | select(test("-") | not)] | last'
}

tools_outdated=false

while IFS= read -r tool_id; do
    current="$(jq -r --arg id "$tool_id" '.tools[$id].version' "$TOOLS_MANIFEST")"
    latest="$(latest_stable "$tool_id")"

    if [[ "$current" == "$latest" ]]; then
        printf "  %-35s %s (up to date)\n" "$tool_id" "$current"
    else
        printf "  %-35s %s -> %s\n" "$tool_id" "$current" "$latest"
        tools_outdated=true
        if $UPGRADE; then
            dotnet tool update --local "$tool_id"
        fi
    fi
done < <(jq -r '.tools | keys[]' "$TOOLS_MANIFEST")

if ! $tools_outdated; then
    echo "  All tools are up to date."
fi
