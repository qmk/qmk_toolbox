using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Bootloader;

/// <summary>
/// A mass-storage bootloader family, flashed by copying the firmware file onto a mounted
/// volume identified by its marker file. Adding a family is one row below — plus a
/// <see cref="BootloaderFactory"/> DeviceMap entry when it has a fixed VID/PID, or
/// <see cref="ProbeOnUnknownDevice"/> when it can only be recognised by its marker.
/// </summary>
internal sealed record MassStorageBootloader(
    BootloaderType Type,
    string Name,
    string MarkerFile,
    string DestFileName,
    string[] Extensions,
    bool DeleteBeforeCopy,
    string CompletionMessage,
    bool ProbeOnUnknownDevice,
    Func<string, string?>? BoardIdReader = null)
{
    public static readonly MassStorageBootloader[] All =
    [
        // The LUFA MS virtual FAT needs the old file deleted so the new directory entry and
        // data blocks are rewritten in full.
        new(BootloaderType.LufaMs, "LUFA MS", "FLASH.BIN", "FLASH.BIN", [".bin"],
            DeleteBeforeCopy: true, "Done, please eject drive now.", ProbeOnUnknownDevice: false),

        // UF2 bootloaders intercept UF2 block writes regardless of file name and reboot when
        // the write completes; NEW.UF2 is a safe 8.3 name (and what uf2conv.py uses) since
        // firmware file names can exceed the volume's FAT limits.
        new(BootloaderType.Uf2, "UF2", "INFO_UF2.TXT", "NEW.UF2", [".uf2"],
            DeleteBeforeCopy: false, "Done, device should reboot automatically.", ProbeOnUnknownDevice: true,
            ReadUf2BoardId),
    ];

    /// <summary>Families detected by marker probe on unknown mass-storage devices.</summary>
    public static IEnumerable<MassStorageBootloader> Probeable => All.Where(f => f.ProbeOnUnknownDevice);

    // Returns the Board-ID: value from a UF2 marker file, or null when absent or unreadable
    // (UF2 spec requires the file, but a missing ID must never break detection).
    private static string? ReadUf2BoardId(string markerPath)
    {
        const string prefix = "Board-ID:";
        try
        {
            foreach (string line in File.ReadLines(markerPath))
            {
                if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return line[prefix.Length..].Trim();
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
        return null;
    }
}
