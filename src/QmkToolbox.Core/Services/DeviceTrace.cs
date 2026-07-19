using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Services;

/// <summary>
/// The shared grammar for device-event diagnostic trace lines, used by both USB detectors and
/// the orchestrator so identifiers and paths format identically (and stay greppable) everywhere.
/// </summary>
public static class DeviceTrace
{
    /// <summary>Formats a VID/PID pair as <c>VID:XXXX PID:XXXX</c> (uppercase hex).</summary>
    public static string VidPid(ushort vendorId, ushort productId) =>
        $"VID:{vendorId:X4} PID:{productId:X4}";

    /// <inheritdoc cref="VidPid(ushort, ushort)"/>
    public static string VidPid(IUsbDevice device) => VidPid(device.VendorId, device.ProductId);

    /// <summary>Quotes a device path, or <c>(empty)</c> when absent.</summary>
    public static string Path(string? path) =>
        string.IsNullOrEmpty(path) ? "(empty)" : $"\"{path}\"";
}
