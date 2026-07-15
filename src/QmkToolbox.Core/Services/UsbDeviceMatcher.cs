using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Services;

/// <summary>
/// Resolves which tracked device a USB removal event refers to. Removal events are lossy — some
/// platforms drop VID/PID, or report a differently-cased path — so matching prefers an exact device
/// path, then optionally falls back to VID/PID.
/// </summary>
public static class UsbDeviceMatcher
{
    /// <param name="tracked">Currently-tracked devices.</param>
    /// <param name="eventPath">Device path from the event; may be empty.</param>
    /// <param name="vidPidFallback">When non-null, its VID/PID is matched if the path doesn't (Linux
    /// and macOS removal events often drop the path); pass null to match by path only (Windows, whose
    /// interface paths are canonical and always present).</param>
    /// <param name="pathComparison">Ordinal on Unix; case-insensitive for Windows interface paths.</param>
    public static IUsbDevice? Find(
        IReadOnlyList<IUsbDevice> tracked,
        string eventPath,
        IUsbDevice? vidPidFallback,
        StringComparison pathComparison = StringComparison.Ordinal)
    {
        if (!string.IsNullOrEmpty(eventPath))
        {
            foreach (IUsbDevice d in tracked)
            {
                if (!string.IsNullOrEmpty(d.DevicePath) && string.Equals(d.DevicePath, eventPath, pathComparison))
                    return d;
            }
        }

        if (vidPidFallback is not null)
        {
            foreach (IUsbDevice d in tracked)
            {
                if (d.VendorId == vidPidFallback.VendorId && d.ProductId == vidPidFallback.ProductId)
                    return d;
            }
        }

        return null;
    }
}
