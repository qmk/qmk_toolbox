## Setting up certificates

### Requirements

Three certificates are needed for signing and notarization:

- Apple Development certificate for signing (renewed every year)
- Developer ID Application - For distributing .app
- Developer ID Installer - For distributing .pkg

### Getting Certificates into GitHub Actions

Here's how to export the certificates from Keychain and import them into GitHub Actions secrets:

1. Make Developer ID Application, Developer ID Installer, and Apple Development certificates
2. Go to QMK Toolbox repository Settings -> Security -> Secrets -> Actions
3. Go into Keychain Access and export both the certificate and private key to .p12 and set passphrase
4. Run `base64 <cert_name>.p12 | pbcopy` to get certificate into clipboard
5. Paste into certificate data secret `<prefix>_CERTIFICATE_DATA`
6. Input passphrase for the exported certificate into `<prefix>_CERTIFICATE_PASSPHRASE`

Prefixes for the certificate secrets:

- Apple Development: `DEVELOPMENT`
- Developer ID Application: `APP_DISTRIBUTION`
- Developer ID Installer: `INSTALLER_DISTRIBUTION`

### Other Secrets

- `KEYCHAIN_PASSWORD`: Can be set to anything
- `NOTARIZATION_USERNAME`: Apple ID account
- `NOTARIZATION_PASSWORD`: App-specific password generated for Apple ID account
- `DEVELOPER_ID_INSTALLER_NAME`: Name of certificate for Developer ID Application (ex. `Developer ID Application: Big Organization (XXXXXXXXX)`)
