using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

public class UsbDeviceMatcherTests
{
    private static UsbDeviceInfo Dev(string path, ushort vid = 0, ushort pid = 0) =>
        new(vid, pid, 0, "", "", "", path);

    private static IReadOnlyList<IUsbDevice> Set(params IUsbDevice[] devices) => devices;

    [Fact]
    public void Find_ExactPath_Matches()
    {
        IUsbDevice b = Dev("/b", 3, 4);
        IReadOnlyList<IUsbDevice> tracked = Set(Dev("/a", 1, 2), b);

        Assert.Same(b, UsbDeviceMatcher.Find(tracked, "/b", vidPidFallback: Dev("", 9, 9)));
    }

    [Fact]
    public void Find_PathTakesPrecedenceOverVidPid()
    {
        IUsbDevice pathHit = Dev("/target", 1, 1);
        IReadOnlyList<IUsbDevice> tracked = Set(Dev("/other", 9, 9), pathHit);

        Assert.Same(pathHit, UsbDeviceMatcher.Find(tracked, "/target", vidPidFallback: Dev("", 9, 9)));
    }

    [Fact]
    public void Find_NoPathMatch_FallsBackToVidPid()
    {
        IUsbDevice b = Dev("/b", 3, 4);
        IReadOnlyList<IUsbDevice> tracked = Set(Dev("/a", 1, 2), b);

        Assert.Same(b, UsbDeviceMatcher.Find(tracked, "/missing", vidPidFallback: Dev("", 3, 4)));
    }

    [Fact]
    public void Find_EmptyEventPath_UsesVidPidFallback()
    {
        IUsbDevice a = Dev("/a", 1, 2);
        IReadOnlyList<IUsbDevice> tracked = Set(a);

        Assert.Same(a, UsbDeviceMatcher.Find(tracked, "", vidPidFallback: Dev("", 1, 2)));
    }

    [Fact]
    public void Find_NoFallback_MatchesPathOnly()
    {
        IReadOnlyList<IUsbDevice> tracked = Set(Dev("/a", 1, 2));

        Assert.Null(UsbDeviceMatcher.Find(tracked, "/missing", vidPidFallback: null));
        Assert.NotNull(UsbDeviceMatcher.Find(tracked, "/a", vidPidFallback: null));
    }

    [Fact]
    public void Find_TrackedWithEmptyPath_NotMatchedByNonEmptyEventPath()
    {
        IReadOnlyList<IUsbDevice> tracked = Set(Dev("", 1, 2));

        Assert.Null(UsbDeviceMatcher.Find(tracked, "/a", vidPidFallback: null));
    }

    [Fact]
    public void Find_PathComparison_RespectsCaseSensitivity()
    {
        IReadOnlyList<IUsbDevice> tracked = Set(Dev(@"\\?\USB#VID_1", 1, 2));

        Assert.Null(UsbDeviceMatcher.Find(tracked, @"\\?\usb#vid_1", null, StringComparison.Ordinal));
        Assert.NotNull(UsbDeviceMatcher.Find(tracked, @"\\?\usb#vid_1", null, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Find_MultipleVidPidCandidates_ReturnsFirst()
    {
        IUsbDevice first = Dev("/a", 5, 6);
        IReadOnlyList<IUsbDevice> tracked = Set(first, Dev("/b", 5, 6));

        Assert.Same(first, UsbDeviceMatcher.Find(tracked, "/missing", vidPidFallback: Dev("", 5, 6)));
    }

    [Fact]
    public void Find_NoMatch_ReturnsNull()
    {
        IReadOnlyList<IUsbDevice> tracked = Set(Dev("/a", 1, 2));

        Assert.Null(UsbDeviceMatcher.Find(tracked, "/x", vidPidFallback: Dev("", 7, 8)));
    }
}
