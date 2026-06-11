using HidApi;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Services.Hid;

public abstract class BaseHidDevice(DeviceInfo deviceInfo) : IHidDevice
{
    public DeviceInfo DeviceInfo { get; } = deviceInfo;
    public string ManufacturerString { get; } = deviceInfo.ManufacturerString ?? "";
    public string ProductString { get; } = deviceInfo.ProductString ?? "";
    public ushort VendorId { get; } = deviceInfo.VendorId;
    public ushort ProductId { get; } = deviceInfo.ProductId;
    public ushort RevisionBcd { get; } = deviceInfo.ReleaseNumber;
    public ushort UsagePage { get; } = deviceInfo.UsagePage;
    public ushort Usage { get; } = deviceInfo.Usage;
    public string DevicePath { get; } = deviceInfo.Path;

    /// <inheritdoc />
    public abstract bool IsConsoleDevice { get; }

    public override string ToString() =>
        $"{ManufacturerString} {ProductString} ({VendorId:X4}:{ProductId:X4}:{RevisionBcd:X4})";
}
