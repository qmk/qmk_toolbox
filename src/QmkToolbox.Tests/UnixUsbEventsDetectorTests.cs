using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Services;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Drives the Unix detector's arrival-payload conversion with realistic Usb.Events payloads
/// against a fixture-owned fake sysfs node — the test that would have caught F1 (RevisionBcd
/// never populated). Linux-only: the revision read goes through the real sysfs path.
/// </summary>
public sealed class UnixUsbEventsDetectorTests : IDisposable
{
    private readonly string _syspath = Directory.CreateTempSubdirectory("qmk-sysfs-test-").FullName;

    public void Dispose() => Directory.Delete(_syspath, recursive: true);

    // The values a Usb.Events arrival carries on Linux: udev hex IDs and the device syspath.
    private UsbDeviceInfo? Convert(bool includeRevision) => UnixUsbEventsDetector.ToUsbDeviceInfo(
        vendorId: "0x03eb", productId: "0x2ff4", vendor: "QMK", product: "Atmel DFU",
        deviceSystemPath: _syspath, includeRevision: includeRevision);

    [FactOnLinux]
    public void Arrival_QmkDfuPayload_YieldsRevisionFromSysfs()
    {
        File.WriteAllText(Path.Combine(_syspath, "bcdDevice"), "0936\n");

        UsbDeviceInfo? device = Convert(includeRevision: true);

        Assert.NotNull(device);
        Assert.Equal(0x03EB, device.VendorId);
        Assert.Equal(0x2FF4, device.ProductId);
        Assert.Equal(0x0936, device.RevisionBcd);
    }

    [FactOnLinux]
    public void Arrival_NoBcdDeviceAttribute_YieldsZeroRevision()
    {
        UsbDeviceInfo? device = Convert(includeRevision: true);

        Assert.NotNull(device);
        Assert.Equal(0, device.RevisionBcd);
    }

    [FactOnLinux]
    public void Arrival_UnreadableBcdDeviceAttribute_YieldsZeroRevision()
    {
        File.WriteAllText(Path.Combine(_syspath, "bcdDevice"), "not hex at all");

        UsbDeviceInfo? device = Convert(includeRevision: true);

        Assert.NotNull(device);
        Assert.Equal(0, device.RevisionBcd);
    }

    [FactOnLinux]
    public void Removal_DoesNotReadRevision()
    {
        File.WriteAllText(Path.Combine(_syspath, "bcdDevice"), "0936\n");

        UsbDeviceInfo? device = Convert(includeRevision: false);

        Assert.NotNull(device);
        Assert.Equal(0, device.RevisionBcd);
    }
}
