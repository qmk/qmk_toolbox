using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Services;

/// <summary>
/// Resolves the filesystem mount point associated with a USB mass-storage device.
/// </summary>
public interface IMountPointService
{
    /// <summary>
    /// Returns the mount point path for <paramref name="device"/>,
    /// or <see langword="null"/> if the device is not mounted.
    /// </summary>
    string? FindMountPoint(IUsbDevice device);
}
