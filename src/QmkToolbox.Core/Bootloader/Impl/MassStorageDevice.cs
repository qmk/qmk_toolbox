using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>Mass-storage bootloader device (copies the firmware file to a mounted volume).</summary>
internal sealed class MassStorageDevice : BootloaderDevice
{
    private readonly MassStorageBootloader _family;

    public string? MountPoint { get; private set; }

    public MassStorageDevice(MassStorageBootloader family, IUsbDevice device, IFlashToolProvider toolProvider, IMountPointService? mountPointService = null, string? boardId = null, string? mountPoint = null)
        : base(device, toolProvider, mountPointService: mountPointService)
    {
        _family = family;
        Type = family.Type;
        Name = boardId == null ? family.Name : $"{family.Name} ({boardId})";
        PreferredDriver = "USBSTOR";
        MountPoint = mountPoint;
    }

    public override async Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, _family.Extensions);

        MountPoint = await FindMountPointAsync(_family.MarkerFile).ConfigureAwait(false);
        if (MountPoint == null)
        {
            PrintMessage("Mount point not found! The volume must be mounted before flashing.", MessageType.Error);
            return;
        }
        string destFile = Path.Combine(MountPoint, _family.DestFileName);

        // File.Delete/Copy are blocking synchronous calls that can be slow on USB
        // mass storage; Task.Run offloads them to a thread pool thread so the UI
        // stays responsive. PrintMessage/OutputReceived are safe from any thread —
        // callers (FlashOrchestrator) always marshal to the UI thread via Invoke.
        await Task.Run(() =>
        {
            try
            {
                if (_family.DeleteBeforeCopy)
                {
                    PrintMessage($"Deleting {destFile}...", MessageType.Command);
                    File.Delete(destFile);
                }
                PrintMessage($"Copying {file} to {destFile}...", MessageType.Command);
                File.Copy(file, destFile, overwrite: true);
                PrintMessage(_family.CompletionMessage, MessageType.Bootloader);
            }
            catch (IOException e)
            {
                PrintMessage($"IO ERROR: {e.Message}", MessageType.Error);
            }
        }).ConfigureAwait(false);
    }

    public override string ToString() =>
        MountPoint == null ? base.ToString() : $"{base.ToString()} [{MountPoint}]";
}
