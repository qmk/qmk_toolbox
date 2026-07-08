#!/usr/bin/env bash
# macos-signing.sh — code signing / notarization for QMK Toolbox using rcodesign.
#
# The bundled flash tools under resources/flashutils/osx are embedded *inside*
# the single-file executable at publish time, so they must be signed BEFORE the
# build. A full signed build therefore looks like:
#
#   scripts/macos-signing.sh setup
#   scripts/macos-signing.sh sign-resources resources/*/osx
#   scripts/publish-all.sh                       # rebuild, embedding signed tools
#   scripts/make-macos-app.sh                    # build the .app bundle
#   scripts/macos-signing.sh sign-app "publish-osx-dmg/QMK Toolbox.app"
#   scripts/macos-signing.sh notarize "publish-osx-dmg/QMK Toolbox.app"
#   # ... repackage the signed .app into the zip/dmg, then ...
#   scripts/make-macos-pkg.sh                    # build the .pkg from the signed .app
#   scripts/macos-signing.sh sign-pkg "artifacts/QMK Toolbox.pkg"
#   scripts/macos-signing.sh notarize "artifacts/QMK Toolbox.pkg"
#   scripts/macos-signing.sh cleanup
#
# --------------------------------------------------------------------------
# Environment variables (identical to the existing repository secret set)
# --------------------------------------------------------------------------
#   MACOS_CERTIFICATE_P12_BASE64            base64 "Developer ID Application" .p12
#   MACOS_CERTIFICATE_PASSWORD              password for that .p12
#   MACOS_INSTALLER_CERTIFICATE_P12_BASE64  base64 "Developer ID Installer" .p12
#   MACOS_INSTALLER_CERTIFICATE_PASSWORD    password for that .p12
#   APPLE_API_KEY_BASE64                    base64 App Store Connect AuthKey_*.p8
#   APPLE_API_KEY_ID                        App Store Connect Key ID
#   APPLE_API_ISSUER                        App Store Connect Issuer ID
#
# Optional overrides:
#   QMK_SIGN_DIR        where decoded secrets live (default ${RUNNER_TEMP:-/tmp}/qmk-signing)
#   QMK_BUILDER_IMAGE   override the builder image
#   MACOS_ENTITLEMENTS  path to a custom entitlements plist
# --------------------------------------------------------------------------

set -eEuo pipefail

# Owner-only for everything we create — most of it is private key material.
umask 077

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"
BUILDER="${QMK_BUILDER_IMAGE:-ghcr.io/tzarc/qmk_toolchains:builder}"
SIGN_DIR="${QMK_SIGN_DIR:-${RUNNER_TEMP:-/tmp}/qmk-signing}"

log() { echo "==> $*" >&2; }
die() { echo "error: $*" >&2; exit 1; }

require_env() {
	local missing=()
	local var
	for var in "$@"; do
		[[ -n "${!var:-}" ]] || missing+=("$var")
	done
	((${#missing[@]} == 0)) || die "missing required environment variable(s): ${missing[*]}"
}

# Run rcodesign from the builder image: repo at /app, secrets at /signing, as
# the host user so it can read the umask-077 secrets and own the outputs.
rcodesign_() {
	local docker_user=""
	if [[ "$(id -u)" -ne 0 ]]; then
		docker_user="-u $(id -u):$(id -g)"
	fi
	# shellcheck disable=SC2086
	docker run --rm ${docker_user} -e HOME=/tmp \
		-v "${REPO_ROOT}:/app" -v "${SIGN_DIR}:/signing" -w /app \
		"${BUILDER}" rcodesign "$@"
}

# Translate a host path (absolute or relative to $PWD) into a path relative to
# REPO_ROOT, which is what the container sees under its /app working directory.
repo_rel() {
	local p="$1" abs
	abs="$(cd "$(dirname "$p")" && pwd)/$(basename "$p")"
	case "$abs" in
	"${REPO_ROOT}/"*) printf '%s' "${abs#"${REPO_ROOT}"/}" ;;
	*) die "path is outside the repository, cannot sign in container: $p" ;;
	esac
}

is_macho() { file -b "$1" | grep -q "Mach-O"; }

# The bundle/package paths contain a space ("QMK Toolbox.app"/.pkg), which the
# containerised rcodesign invocation mishandles. Run the operation against a
# space-free sibling name, restoring the original name afterwards (even on
# failure). The (container) target path is appended as the final argument.
# Usage: rcodesign_on <host-target-path> <rcodesign subcommand + flags...>
rcodesign_on() {
	local target="$1"
	shift
	local dir base safe rv=0
	dir="$(cd "$(dirname "$target")" && pwd)"
	base="$(basename "$target")"
	safe="${base// /_}"

	if [[ "$safe" == "$base" ]]; then
		rcodesign_ "$@" "$(repo_rel "$target")"
		return
	fi

	[[ -e "${dir}/${safe}" ]] && die "cannot rename '${base}': '${safe}' already exists in ${dir}"
	mv "${dir}/${base}" "${dir}/${safe}"
	rcodesign_ "$@" "$(repo_rel "${dir}/${safe}")" || rv=$?
	mv "${dir}/${safe}" "${dir}/${base}"
	return "$rv"
}

require_setup() {
	[[ -f "${SIGN_DIR}/app.p12" ]] || die "signing material not found in ${SIGN_DIR}; run '$0 setup' first"
}

cmd_setup() {
	require_env MACOS_CERTIFICATE_P12_BASE64 MACOS_CERTIFICATE_PASSWORD \
		MACOS_INSTALLER_CERTIFICATE_P12_BASE64 MACOS_INSTALLER_CERTIFICATE_PASSWORD \
		APPLE_API_KEY_BASE64 APPLE_API_KEY_ID APPLE_API_ISSUER

	log "Preparing signing material in ${SIGN_DIR}"
	rm -rf "${SIGN_DIR}"
	mkdir -p "${SIGN_DIR}"

	# Developer ID Application certificate (binaries, the .app and the .dmg).
	printf '%s' "$MACOS_CERTIFICATE_P12_BASE64" | tr -d '[:space:]' | base64 -d >"${SIGN_DIR}/app.p12"
	printf '%s' "$MACOS_CERTIFICATE_PASSWORD" >"${SIGN_DIR}/app.pass"

	# Developer ID Installer certificate (the .pkg).
	printf '%s' "$MACOS_INSTALLER_CERTIFICATE_P12_BASE64" | tr -d '[:space:]' | base64 -d >"${SIGN_DIR}/installer.p12"
	printf '%s' "$MACOS_INSTALLER_CERTIFICATE_PASSWORD" >"${SIGN_DIR}/installer.pass"

	# App Store Connect API key, encoded into rcodesign's unified JSON form.
	printf '%s' "$APPLE_API_KEY_BASE64" | tr -d '[:space:]' | base64 -d >"${SIGN_DIR}/AuthKey.p8"
	rcodesign_ encode-app-store-connect-api-key -o /signing/asc-api-key.json \
		"$APPLE_API_ISSUER" "$APPLE_API_KEY_ID" /signing/AuthKey.p8
	rm -f "${SIGN_DIR}/AuthKey.p8"

	# Hardened-runtime entitlements. Unlike the old Cocoa app (which shipped an
	# empty entitlements file), the .NET/Avalonia app JITs and dlopen()s bundled
	# native libraries, so it needs the JIT / unsigned-executable-memory /
	# library-validation exceptions or it will not launch under the hardened
	# runtime that notarization requires.
	if [[ -n "${MACOS_ENTITLEMENTS:-}" ]]; then
		[[ -f "$MACOS_ENTITLEMENTS" ]] || die "entitlements file not found: $MACOS_ENTITLEMENTS"
		cp "$MACOS_ENTITLEMENTS" "${SIGN_DIR}/entitlements.plist"
	else
		cp "${REPO_ROOT}/resources/macos-app-support/entitlements.plist" \
		   "${SIGN_DIR}/entitlements.plist" || die "default entitlements not found"
	fi

	log "Signing material ready"
}

# Sign every Mach-O binary directly inside the given directories (non-recursive)
# with the Developer ID Application certificate and the hardened runtime. Used
# for the bundled flash tools and the hidapi dylib under resources/*/osx, which
# get embedded into the single-file executable before the build.
cmd_sign_resources() {
	[[ $# -gt 0 ]] || die "usage: sign-resources <dir> [dir...]"
	require_setup

	shopt -s nullglob
	local dir f signed=0
	local -a rels=()
	for dir in "$@"; do
		[[ -d "$dir" ]] || die "not a directory: $dir"
		for f in "$dir"/*; do
			[[ -f "$f" ]] || continue
			if is_macho "$f"; then
				log "Signing $f"
				rcodesign_ sign \
					--p12-file /signing/app.p12 --p12-password-file /signing/app.pass \
					--code-signature-flags runtime \
					"$(repo_rel "$f")"
				rels+=("$(repo_rel "$f")")
				signed=$((signed + 1))
			fi
		done
	done
	log "Signed ${signed} Mach-O binaries in: $*"

	# These are committed binaries; signing them in place dirties the working
	# tree, which would make GitInfo stamp the build version "-dirty". Tell git
	# to ignore the in-place changes so the subsequent rebuild reports the real
	# (clean) commit. Only tracked files can be marked.
	local rel
	for rel in "${rels[@]}"; do
		if git -C "${REPO_ROOT}" ls-files --error-unmatch -- "$rel" >/dev/null 2>&1; then
			git -C "${REPO_ROOT}" update-index --assume-unchanged -- "$rel"
		fi
	done
}

cmd_sign_app() {
	local app="${1:?usage: sign-app <path/to/App.app>}"
	require_setup
	[[ -d "$app" ]] || die "not a directory: $app"

	# --for-notarization requires a Developer ID certificate + timestamp and
	# turns on the hardened runtime for every Mach-O in the bundle.
	rcodesign_on "$app" sign \
		--p12-file /signing/app.p12 --p12-password-file /signing/app.pass \
		--for-notarization \
		--entitlements-xml-file /signing/entitlements.plist
}

cmd_sign_pkg() {
	local pkg="${1:?usage: sign-pkg <path/to/Installer.pkg>}"
	require_setup
	[[ -f "$pkg" ]] || die "not a file: $pkg"

	rcodesign_on "$pkg" sign \
		--p12-file /signing/installer.p12 --p12-password-file /signing/installer.pass
}

cmd_notarize() {
	local target="${1:?usage: notarize <path/to/App.app|Installer.pkg>}"
	require_setup
	[[ -e "$target" ]] || die "not found: $target"

	# --staple implies --wait; rcodesign zips .app bundles automatically for
	# upload and staples the ticket back onto the bundle/package in place.
	rcodesign_on "$target" notary-submit \
		--api-key-file /signing/asc-api-key.json \
		--staple
}

cmd_cleanup() {
	log "Removing signing material in ${SIGN_DIR}"
	rm -rf "${SIGN_DIR}"
}

main() {
	local command="${1:-}"
	[[ -n "$command" ]] || die "usage: $0 <setup|sign-resources|sign-app|sign-pkg|notarize|cleanup> [args...]"
	shift || true

	case "$command" in
	setup) cmd_setup "$@" ;;
	sign-resources) cmd_sign_resources "$@" ;;
	sign-app) cmd_sign_app "$@" ;;
	sign-pkg) cmd_sign_pkg "$@" ;;
	notarize) cmd_notarize "$@" ;;
	cleanup) cmd_cleanup "$@" ;;
	*) die "unknown command: $command" ;;
	esac
}

main "$@"
