using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

public class UsbDeviceParserTests
{
    // ── TryParseUsbId ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("0x0483", 0x0483)] // Linux: "0x"-prefixed hex, lower case prefix
    [InlineData("0X0483", 0x0483)] // Linux: "0X"-prefixed hex, upper case prefix
    [InlineData("0xFFFF", 0xFFFF)] // max value with prefix
    [InlineData("0483", 0x0483)]   // Windows/Linux: bare 4-digit hex
    [InlineData("DF11", 0xDF11)]   // Windows/Linux: bare hex with letters
    [InlineData("FFFF", 0xFFFF)]   // Windows/Linux: max bare hex
    public void TryParseUsbId_NonMacOS_ParsesHex(string input, ushort expected)
    {
        bool ok = UsbDeviceParser.TryParseUsbId(input, isMacOS: false, out ushort value);
        Assert.True(ok);
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("1155", 0x0483)]   // macOS IOKit decimal: 1155d == 0x0483
    [InlineData("65535", 0xFFFF)]  // macOS IOKit decimal: max ushort
    [InlineData("0", 0x0000)]      // macOS IOKit decimal: zero
    public void TryParseUsbId_MacOS_ParsesDecimal(string input, ushort expected)
    {
        bool ok = UsbDeviceParser.TryParseUsbId(input, isMacOS: true, out ushort value);
        Assert.True(ok);
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TryParseUsbId_NullOrEmpty_ReturnsFalse(string? input) =>
        Assert.False(UsbDeviceParser.TryParseUsbId(input, isMacOS: false, out _));

    [Theory]
    [InlineData("ZZZZ")]   // not valid hex
    [InlineData("0xGGGG")] // invalid after 0x prefix
    [InlineData("10000")]  // overflows ushort (hex) — 65536
    public void TryParseUsbId_Invalid_ReturnsFalse(string input) =>
        Assert.False(UsbDeviceParser.TryParseUsbId(input, isMacOS: false, out _));

    // ── TryParseHwId ──────────────────────────────────────────────────────────

    [Fact]
    public void TryParseHwId_BasicVidPid_ParsesCorrectly()
    {
        bool ok = UsbDeviceParser.TryParseHwId(
            @"USB\VID_0483&PID_DF11\5&2D4F03CB&0&2",
            out ushort vid, out ushort pid, out ushort rev);

        Assert.True(ok);
        Assert.Equal(0x0483, vid);
        Assert.Equal(0xDF11, pid);
        Assert.Equal(0x0000, rev);
    }

    [Fact]
    public void TryParseHwId_WithRevision_ParsesRev()
    {
        bool ok = UsbDeviceParser.TryParseHwId(
            @"USB\VID_03EB&PID_2FFB&REV_0200\5&0",
            out ushort vid, out ushort pid, out ushort rev);

        Assert.True(ok);
        Assert.Equal(0x03EB, vid);
        Assert.Equal(0x2FFB, pid);
        Assert.Equal(0x0200, rev);
    }

    [Fact]
    public void TryParseHwId_LowercasePath_ParsesCorrectly()
    {
        bool ok = UsbDeviceParser.TryParseHwId(
            @"usb\vid_0483&pid_df11\5",
            out ushort vid, out ushort pid, out ushort rev);

        Assert.True(ok);
        Assert.Equal(0x0483, vid);
        Assert.Equal(0xDF11, pid);
        Assert.Equal(0x0000, rev);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a usb path")]
    [InlineData(@"ACPI\ACPI0005\2&1ABC&0")] // no VID_/PID_ pattern at all
    public void TryParseHwId_NoMatch_ReturnsFalse(string input) =>
        Assert.False(UsbDeviceParser.TryParseHwId(input, out _, out _, out _));

    // ── IsWindowsRootUsbDevice ─────────────────────────────────────────────────

    [Theory]
    [InlineData(@"USB\VID_0483&PID_DF11\5&2D4F03CB&0&2")] // root USB device
    [InlineData(@"USB\VID_2E8A&PID_0003\5&ABC")]           // Raspberry Pi root
    public void IsWindowsRootUsbDevice_OnWindows_AcceptsRootUsbDevice(string path) =>
        Assert.True(UsbDeviceParser.IsWindowsRootUsbDevice(path, isWindows: true));

    [Theory]
    [InlineData(@"USB\VID_0483&PID_DF11&MI_00\7&123")] // composite interface child
    [InlineData(@"USB\VID_2E8A&PID_0003&MI_02\7&456")] // another interface child
    public void IsWindowsRootUsbDevice_OnWindows_RejectsCompositeInterface(string path) =>
        Assert.False(UsbDeviceParser.IsWindowsRootUsbDevice(path, isWindows: true));

    [Theory]
    [InlineData(@"HID\VID_0483&PID_DF11\1&ABC")]   // HID child
    [InlineData(@"USBSTOR\DISK&VEN_&PROD_\1&ABC")] // USB storage
    [InlineData("")]                                // empty path
    public void IsWindowsRootUsbDevice_OnWindows_RejectsNonUsbRoot(string path) =>
        Assert.False(UsbDeviceParser.IsWindowsRootUsbDevice(path, isWindows: true));

    [Theory]
    [InlineData(@"HID\VID_0483&PID_DF11\1&ABC")]    // would be rejected on Windows
    [InlineData(@"USB\VID_0483&PID_DF11&MI_00\7")] // would be rejected on Windows
    [InlineData("")]
    public void IsWindowsRootUsbDevice_OnNonWindows_AlwaysReturnsTrue(string path) =>
        Assert.True(UsbDeviceParser.IsWindowsRootUsbDevice(path, isWindows: false));
}
