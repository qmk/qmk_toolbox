namespace QmkToolbox.Core.Services;

/// <summary>
/// Manages the extraction and resolution of bundled flash tool binaries and data files.
/// </summary>
public interface IFlashToolProvider
{
    /// <summary>Returns the absolute path to the named tool binary.</summary>
    string GetToolPath(string toolName);

    /// <summary>Returns the absolute path to the local resource folder where tools are extracted.</summary>
    string GetResourceFolder();

    /// <summary>
    /// Ensures the resource folder is present and up to date.
    /// If the installed manifest's commit date differs from the embedded one, the folder is
    /// wiped and fully re-extracted; otherwise any missing individual files are filled in.
    /// </summary>
    void EnsureResourceFolder();

    /// <summary>Deletes the resource folder and all its contents.</summary>
    void ClearResourceFolder();

    /// <summary>
    /// Extracts a single embedded resource file to the resource folder.
    /// Does nothing if the file already exists.
    /// </summary>
    void ExtractResource(string file);

    /// <summary>Extracts all embedded resource files, skipping any that already exist.</summary>
    void ExtractAllResources();

    /// <summary>Clears the resource folder and fully re-extracts all bundled resources.</summary>
    void ClearAndReExtract();

    /// <summary>
    /// Returns version strings for the flash utils, hidapi, and (on Linux) udev release manifests.
    /// Each value is <c>"host:hash"</c> if the manifest was found, or <c>"unknown"</c>.
    /// The <c>UdevRules</c> field is <see langword="null"/> on non-Linux platforms.
    /// </summary>
    (string? FlashUtils, string? HidApi, string? UdevRules) GetManifestInfo();
}
