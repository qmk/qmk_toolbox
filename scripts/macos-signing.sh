#!/usr/bin/env bash
#
# macos-signing.sh — code signing / notarization helper for QMK Toolbox (macOS)
#
# Runs on a macOS GitHub Actions runner. Configure it via environment
# variables (populated from repository/environment secrets).
#
# Usage:
#   scripts/macos-signing.sh setup-keychain
#   scripts/macos-signing.sh sign-app   "build/QMK Toolbox.app"
#   scripts/macos-signing.sh sign-pkg   "build/QMK Toolbox.pkg" ["out.pkg"]
#   scripts/macos-signing.sh notarize   "build/QMK Toolbox.app" (or a .pkg/.zip)
#   scripts/macos-signing.sh staple     "build/QMK Toolbox.app" (or a .pkg)
#   scripts/macos-signing.sh verify     "build/QMK Toolbox.app" (or a .pkg)
#   scripts/macos-signing.sh validate   "build/QMK Toolbox.app" (or a .pkg)
#   scripts/macos-signing.sh cleanup-keychain
#
# --------------------------------------------------------------------------
# Environment variables
# --------------------------------------------------------------------------
#
# Certificates -- base64-encoded .p12 exports, e.g. `base64 -i cert.p12 | pbcopy`.
# Requires both a "Developer ID Application" cert (app/binaries) and a
# "Developer ID Installer" cert (pkg) -- Apple treats these as distinct types.
#
#   MACOS_CERTIFICATE_P12_BASE64            "Developer ID Application" .p12
#   MACOS_CERTIFICATE_PASSWORD              password protecting that .p12
#   MACOS_INSTALLER_CERTIFICATE_P12_BASE64  "Developer ID Installer" .p12 (only
#                                            required for the sign-pkg command)
#   MACOS_INSTALLER_CERTIFICATE_PASSWORD    password protecting that .p12
#
# Signing identities -- the exact string as reported by
# `security find-identity -v -p codesigning`, e.g.
# "Developer ID Application: Jane Doe (ABCDE12345)".
#
#   MACOS_CODESIGN_IDENTITY      identity used for sign-app
#   MACOS_INSTALLER_IDENTITY     identity used for sign-pkg
#
# Ephemeral CI keychain (setup-keychain / cleanup-keychain):
#
#   MACOS_KEYCHAIN_PASSWORD      any random value, generated fresh per run is fine
#
# Notarization (notarize) -- App Store Connect API key auth is recommended:
#
#   APPLE_API_KEY_BASE64         base64-encoded AuthKey_XXXXXXXXXX.p8
#   APPLE_API_KEY_ID             Key ID
#   APPLE_API_ISSUER             Issuer ID
#
# ...or, alternatively, Apple ID based auth:
#
#   APPLE_ID                       Apple ID email address
#   APPLE_APP_SPECIFIC_PASSWORD    app-specific password for that Apple ID
#   APPLE_TEAM_ID                  Developer Team ID
#
# --------------------------------------------------------------------------

set -euo pipefail

# Restrict all files/dirs this script creates (certs, keys, temp keychain) to
# owner-only access -- several of them are private key material.
umask 077

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
KEYCHAIN_NAME="qmk-toolbox-signing.keychain-db"
KEYCHAIN_PATH="${RUNNER_TEMP:-/tmp}/$KEYCHAIN_NAME"
DEFAULT_ENTITLEMENTS="$SCRIPT_DIR/../macos/QMK Toolbox/QMK Toolbox.entitlements"

log() {
	echo "==> $*" >&2
}

die() {
	echo "error: $*" >&2
	exit 1
}

require_env() {
	local missing=()
	for var in "$@"; do
		if [[ -z "${!var:-}" ]]; then
			missing+=("$var")
		fi
	done
	if ((${#missing[@]} > 0)); then
		die "missing required environment variable(s): ${missing[*]}"
	fi
}

cmd_setup_keychain() {
	require_env MACOS_CERTIFICATE_P12_BASE64 MACOS_CERTIFICATE_PASSWORD MACOS_KEYCHAIN_PASSWORD

	log "Creating temporary keychain at $KEYCHAIN_PATH"
	security create-keychain -p "$MACOS_KEYCHAIN_PASSWORD" "$KEYCHAIN_PATH"
	security set-keychain-settings -lut 21600 "$KEYCHAIN_PATH"
	security unlock-keychain -p "$MACOS_KEYCHAIN_PASSWORD" "$KEYCHAIN_PATH"

	# A .p12 export only bundles the leaf cert; without Apple's intermediate CA
	# certs the chain won't validate, so find-identity reports 0 valid identities.
	local certs_dir
	certs_dir="$(mktemp -d)"
	log "Fetching Apple Developer ID intermediate certificates"
	curl -fsSL -o "$certs_dir/DeveloperIDCA.cer" https://www.apple.com/certificateauthority/DeveloperIDCA.cer
	curl -fsSL -o "$certs_dir/DeveloperIDG2CA.cer" https://www.apple.com/certificateauthority/DeveloperIDG2CA.cer
	security import "$certs_dir/DeveloperIDCA.cer" -k "$KEYCHAIN_PATH"
	security import "$certs_dir/DeveloperIDG2CA.cer" -k "$KEYCHAIN_PATH"
	rm -rf "$certs_dir"

	local cert_path
	cert_path="$(mktemp)"
	printf '%s' "$MACOS_CERTIFICATE_P12_BASE64" | tr -d '[:space:]' | base64 --decode >"$cert_path"
	log "Importing Developer ID Application certificate"
	# -A: allow all apps to use the key without a prompt (fine for this ephemeral keychain).
	security import "$cert_path" -k "$KEYCHAIN_PATH" -f pkcs12 -P "$MACOS_CERTIFICATE_PASSWORD" -A
	rm -f "$cert_path"

	if [[ -n "${MACOS_INSTALLER_CERTIFICATE_P12_BASE64:-}" ]]; then
		require_env MACOS_INSTALLER_CERTIFICATE_PASSWORD
		local installer_cert_path
		installer_cert_path="$(mktemp)"
		printf '%s' "$MACOS_INSTALLER_CERTIFICATE_P12_BASE64" | tr -d '[:space:]' | base64 --decode >"$installer_cert_path"
		log "Importing Developer ID Installer certificate"
		security import "$installer_cert_path" -k "$KEYCHAIN_PATH" -f pkcs12 -P "$MACOS_INSTALLER_CERTIFICATE_PASSWORD" -A
		rm -f "$installer_cert_path"
	fi

	# Add to the search list and make it default so codesign/productsign find it later.
	local existing_keychains
	existing_keychains="$(security list-keychains -d user | sed 's/[\" ]//g')"
	# shellcheck disable=SC2086
	security list-keychains -d user -s "$KEYCHAIN_PATH" $existing_keychains
	security default-keychain -s "$KEYCHAIN_PATH"

	log "Keychain ready"
}

cmd_cleanup_keychain() {
	if [[ -f "$KEYCHAIN_PATH" ]]; then
		log "Deleting temporary keychain"
		security delete-keychain "$KEYCHAIN_PATH" || true
	fi
}

# Sign a single Mach-O file (dylib or executable) with hardened runtime + timestamp.
sign_binary() {
	local path="$1"
	log "codesign: $path"
	codesign --force --options runtime --timestamp --keychain "$KEYCHAIN_PATH" --sign "$MACOS_CODESIGN_IDENTITY" "$path"
}

cmd_sign_app() {
	local app_path="${1:?usage: sign-app <path/to/App.app>}"
	local entitlements="${MACOS_ENTITLEMENTS:-$DEFAULT_ENTITLEMENTS}"

	require_env MACOS_CODESIGN_IDENTITY
	[[ -d "$app_path" ]] || die "not a directory: $app_path"
	[[ -f "$entitlements" ]] || die "entitlements file not found: $entitlements"
	security find-identity -v -p codesigning "$KEYCHAIN_PATH" | grep -qF "$MACOS_CODESIGN_IDENTITY" ||
		die "MACOS_CODESIGN_IDENTITY not found in keychain: $MACOS_CODESIGN_IDENTITY"

	log "Signing bundled binaries and dylibs in $app_path"
	local dir file
	for dir in "$app_path/Contents/Resources" "$app_path/Contents/Frameworks"; do
		[[ -d "$dir" ]] || continue
		while IFS= read -r -d '' file; do
			if file -b "$file" | grep -q "Mach-O"; then
				sign_binary "$file"
			fi
		done < <(find "$dir" -type f -print0)
	done

	# Sign any nested bundles (frameworks/helper apps) as whole units.
	local bundle
	while IFS= read -r -d '' bundle; do
		log "codesign (bundle): $bundle"
		codesign --force --options runtime --timestamp --keychain "$KEYCHAIN_PATH" --sign "$MACOS_CODESIGN_IDENTITY" "$bundle"
	done < <(find "$app_path/Contents" -depth \( -name "*.framework" -o -name "*.app" \) -print0 2>/dev/null || true)

	log "Signing app bundle: $app_path"
	codesign --force --options runtime --timestamp \
		--keychain "$KEYCHAIN_PATH" \
		--entitlements "$entitlements" \
		--sign "$MACOS_CODESIGN_IDENTITY" "$app_path"

	cmd_verify "$app_path"
}

cmd_sign_pkg() {
	local pkg_in="${1:?usage: sign-pkg <path/to/Installer.pkg> [output.pkg]}"
	local pkg_out="${2:-$pkg_in}"

	require_env MACOS_INSTALLER_IDENTITY
	[[ -f "$pkg_in" ]] || die "not a file: $pkg_in"

	local tmp_out
	tmp_out="$(mktemp)"
	rm -f "$tmp_out" # productsign refuses to write to an existing file

	log "Signing installer package: $pkg_in"
	productsign --sign "$MACOS_INSTALLER_IDENTITY" --keychain "$KEYCHAIN_PATH" --timestamp "$pkg_in" "$tmp_out"
	mv "$tmp_out" "$pkg_out"

	cmd_verify "$pkg_out"
}

cmd_notarize() {
	local target="${1:?usage: notarize <path/to/App.app|Installer.pkg|archive.zip>}"
	local submit_path="$target"
	local tmp_zip=""

	if [[ "$target" == *.app ]]; then
		tmp_zip="${RUNNER_TEMP:-/tmp}/$(basename "$target" .app)-notarize.zip"
		log "Zipping $target for submission"
		ditto -c -k --keepParent "$target" "$tmp_zip"
		submit_path="$tmp_zip"
	fi

	local auth_args=()
	if [[ -n "${APPLE_API_KEY_BASE64:-}" ]]; then
		require_env APPLE_API_KEY_ID APPLE_API_ISSUER
		local key_path="${RUNNER_TEMP:-/tmp}/AuthKey_${APPLE_API_KEY_ID}.p8"
		printf '%s' "$APPLE_API_KEY_BASE64" | tr -d '[:space:]' | base64 --decode >"$key_path"
		auth_args=(--key "$key_path" --key-id "$APPLE_API_KEY_ID" --issuer "$APPLE_API_ISSUER")
	else
		require_env APPLE_ID APPLE_APP_SPECIFIC_PASSWORD APPLE_TEAM_ID
		auth_args=(--apple-id "$APPLE_ID" --password "$APPLE_APP_SPECIFIC_PASSWORD" --team-id "$APPLE_TEAM_ID")
	fi

	log "Submitting $submit_path to Apple notary service"
	local submit_log
	submit_log="$(mktemp)"
	local failed=0
	xcrun notarytool submit "$submit_path" "${auth_args[@]}" --wait | tee "$submit_log" || failed=1

	local submission_id
	submission_id="$(grep -m1 '^  id:' "$submit_log" | awk '{print $2}')"
	# notarytool's exit code can miss a rejected submission on some versions --
	# check the printed status explicitly too.
	grep -q '^  status: Accepted' "$submit_log" || failed=1
	rm -f "$submit_log"
	[[ -n "$tmp_zip" ]] && rm -f "$tmp_zip"

	if [[ $failed -ne 0 ]]; then
		if [[ -n "$submission_id" ]]; then
			log "Fetching notarization log for submission $submission_id"
			xcrun notarytool log "$submission_id" "${auth_args[@]}" || true
		fi
		[[ -n "${key_path:-}" ]] && rm -f "$key_path"
		die "notarization failed for $target"
	fi

	[[ -n "${key_path:-}" ]] && rm -f "$key_path"
}

cmd_staple() {
	local target="${1:?usage: staple <path/to/App.app|Installer.pkg>}"
	log "Stapling notarization ticket to $target"
	xcrun stapler staple "$target"
}

cmd_verify() {
	local target="${1:?usage: verify <path/to/App.app|Installer.pkg>}"
	case "$target" in
	*.pkg)
		pkgutil --check-signature "$target"
		;;
	*)
		codesign --verify --deep --strict --verbose=2 "$target"
		# Non-fatal: verify runs right after signing, before notarization, so
		# Gatekeeper rejects it here as expected. The `validate` command runs the
		# strict post-staple check.
		spctl --assess --type execute --verbose "$target" || true
		;;
	esac
}

# Strict post-staple gate: unlike `verify` (which runs pre-notarization and
# tolerates a Gatekeeper rejection), every check here is fatal.
cmd_validate() {
	local target="${1:?usage: validate <path/to/App.app|Installer.pkg>}"
	log "Validating notarization + stapling of $target"
	xcrun stapler validate "$target"
	case "$target" in
	*.pkg)
		pkgutil --check-signature "$target"
		spctl --assess --type install --verbose "$target"
		;;
	*)
		codesign --verify --deep --strict --verbose=2 "$target"
		spctl --assess --type execute --verbose "$target"
		;;
	esac
}

main() {
	local command="${1:-}"
	[[ -n "$command" ]] || die "usage: $0 <setup-keychain|sign-app|sign-pkg|notarize|staple|verify|validate|cleanup-keychain> [args...]"
	shift || true

	case "$command" in
	setup-keychain) cmd_setup_keychain "$@" ;;
	sign-app) cmd_sign_app "$@" ;;
	sign-pkg) cmd_sign_pkg "$@" ;;
	notarize) cmd_notarize "$@" ;;
	staple) cmd_staple "$@" ;;
	verify) cmd_verify "$@" ;;
	validate) cmd_validate "$@" ;;
	cleanup-keychain) cmd_cleanup_keychain "$@" ;;
	*) die "unknown command: $command" ;;
	esac
}

main "$@"
