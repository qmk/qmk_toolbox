using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>LUFA Mass Storage bootloader device (copies .bin to mounted volume).</summary>
internal sealed class LufaMsDevice : BootloaderDevice
{
    private readonly IMountPointService? _mountPointService;

    public string? MountPoint { get; private set; }

    public LufaMsDevice(IUsbDevice device, IFlashToolProvider toolProvider, IMountPointService? mountPointService = null)
        : base(device, toolProvider)
    {
        Type = BootloaderType.LufaMs;
        Name = "LUFA MS";
        PreferredDriver = "USBSTOR";
        _mountPointService = mountPointService;
    }

    public override async Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".bin");

        // Automounting completes after the USB arrival event, so the volume is resolved here
        // at flash time — with the same poll-and-retry treatment serial ports get — rather
        // than once at connect time, when the volume usually doesn't exist yet.
        MountPoint = await FindMountPointAsync().ConfigureAwait(false);
        if (MountPoint == null)
        {
            PrintMessage("Mount point not found!", MessageType.Error);
            return;
        }
        string destFile = Path.Combine(MountPoint, "FLASH.BIN");

        // File.Delete/Copy are blocking synchronous calls that can be slow on USB
        // mass storage; Task.Run offloads them to a thread pool thread so the UI
        // stays responsive. PrintMessage/OutputReceived are safe from any thread —
        // callers (FlashOrchestrator) always marshal to the UI thread via Invoke.
        await Task.Run(() =>
        {
            try
            {
                PrintMessage($"Deleting {destFile}...", MessageType.Command);
                File.Delete(destFile);
                PrintMessage($"Copying {file} to {destFile}...", MessageType.Command);
                File.Copy(file, destFile);
                PrintMessage("Done, please eject drive now.", MessageType.Bootloader);
            }
            catch (IOException e)
            {
                PrintMessage($"IO ERROR: {e.Message}", MessageType.Error);
            }
        }).ConfigureAwait(false);
    }

    private async Task<string?> FindMountPointAsync()
    {
        if (_mountPointService == null)
            return null;
        const int attempts = 10;
        const int delayMs = 250;
        for (int i = 0; i < attempts; i++)
        {
            string? mount = _mountPointService.FindMountPoint(Device);
            if (mount != null)
                return mount;
            if (i < attempts - 1)
                await Task.Delay(delayMs).ConfigureAwait(false);
        }
        return null;
    }

    public override string ToString() =>
        MountPoint == null ? base.ToString() : $"{base.ToString()} [{MountPoint}]";
}
