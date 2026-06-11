using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>LUFA Mass Storage bootloader device (copies .bin to mounted volume).</summary>
internal sealed class LufaMsDevice : BootloaderDevice
{
    public string? MountPoint { get; }

    public LufaMsDevice(IUsbDevice device, IFlashToolProvider toolProvider, IMountPointService? mountPointService = null)
        : base(device, toolProvider)
    {
        Type = BootloaderType.LufaMs;
        Name = "LUFA MS";
        PreferredDriver = "USBSTOR";
        MountPoint = mountPointService?.FindMountPoint(device);
    }

    public override async Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".bin");

        // File.Delete/Copy are blocking synchronous calls that can be slow on USB
        // mass storage; Task.Run offloads them to a thread pool thread so the UI
        // stays responsive. PrintMessage/OutputReceived are safe from any thread —
        // callers (FlashOrchestrator) always marshal to the UI thread via Invoke.
        await Task.Run(() =>
        {
            if (MountPoint == null)
            {
                PrintMessage("Mount point not found!", MessageType.Error);
                return;
            }

            string destFile = Path.Combine(MountPoint, "FLASH.BIN");
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

    public override string ToString() => $"{base.ToString()} [{MountPoint}]";
}
