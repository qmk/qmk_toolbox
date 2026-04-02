namespace QmkToolbox.Core.Bootloader;

/// <summary>
/// Thrown when a firmware file's extension is not supported by the target bootloader.
/// </summary>
public class UnsupportedFileFormatException(string file, string[] supportedExtensions)
    : NotSupportedException($"Only firmware files in {string.Join("/", supportedExtensions)} format can be flashed with this bootloader!")
{
    public string File { get; } = file;
    public string[] SupportedExtensions { get; } = supportedExtensions;
}
