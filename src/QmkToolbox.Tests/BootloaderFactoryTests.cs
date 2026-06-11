using NSubstitute;
using QmkToolbox.Core.Bootloader;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

public class BootloaderFactoryTests
{
    private static IUsbDevice Usb(ushort vid, ushort pid, ushort rev = 0) =>
        new UsbDeviceInfo(vid, pid, rev, "", "", "", "");

    // ── GetDeviceType ──────────────────────────────────────────────────────────

    [Theory]
    // Atmel
    [InlineData(0x03EB, 0x2045, 0x0000, BootloaderType.LufaMs)]
    [InlineData(0x03EB, 0x2067, 0x0936, BootloaderType.QmkHid)]
    [InlineData(0x03EB, 0x2067, 0x0001, BootloaderType.LufaHid)]
    [InlineData(0x03EB, 0x2FEF, 0x0936, BootloaderType.QmkDfu)]
    [InlineData(0x03EB, 0x2FEF, 0x0001, BootloaderType.AtmelDfu)]
    [InlineData(0x03EB, 0x2FF0, 0x0936, BootloaderType.QmkDfu)]
    [InlineData(0x03EB, 0x2FF0, 0x0001, BootloaderType.AtmelDfu)]
    [InlineData(0x03EB, 0x2FF3, 0x0936, BootloaderType.QmkDfu)]
    [InlineData(0x03EB, 0x2FF3, 0x0001, BootloaderType.AtmelDfu)]
    [InlineData(0x03EB, 0x2FF4, 0x0936, BootloaderType.QmkDfu)]
    [InlineData(0x03EB, 0x2FF4, 0x0001, BootloaderType.AtmelDfu)]
    [InlineData(0x03EB, 0x2FF9, 0x0936, BootloaderType.QmkDfu)]
    [InlineData(0x03EB, 0x2FF9, 0x0001, BootloaderType.AtmelDfu)]
    [InlineData(0x03EB, 0x2FFA, 0x0936, BootloaderType.QmkDfu)]
    [InlineData(0x03EB, 0x2FFA, 0x0001, BootloaderType.AtmelDfu)]
    [InlineData(0x03EB, 0x2FFB, 0x0936, BootloaderType.QmkDfu)]
    [InlineData(0x03EB, 0x2FFB, 0x0001, BootloaderType.AtmelDfu)]
    [InlineData(0x03EB, 0x6124, 0x0000, BootloaderType.AtmelSamBa)]
    // STMicro
    [InlineData(0x0483, 0xDF11, 0x0000, BootloaderType.Stm32Dfu)]
    // pid.codes
    [InlineData(0x1209, 0x2302, 0x0000, BootloaderType.Caterina)]
    // Van Ooijen
    [InlineData(0x16C0, 0x0478, 0x0000, BootloaderType.HalfKay)]
    [InlineData(0x16C0, 0x0483, 0x0000, BootloaderType.AvrIsp)]
    [InlineData(0x16C0, 0x05DC, 0x0000, BootloaderType.UsbAsp)]
    [InlineData(0x16C0, 0x05DF, 0x0000, BootloaderType.BootloadHid)]
    // MECANIQUE
    [InlineData(0x1781, 0x0C9F, 0x0000, BootloaderType.UsbTinyIsp)]
    // Spark Fun
    [InlineData(0x1B4F, 0x9203, 0x0000, BootloaderType.Caterina)]
    [InlineData(0x1B4F, 0x9205, 0x0000, BootloaderType.Caterina)]
    [InlineData(0x1B4F, 0x9207, 0x0000, BootloaderType.Caterina)]
    // Input Club
    [InlineData(0x1C11, 0xB007, 0x0000, BootloaderType.KiibohdDfu)]
    // Leaflabs
    [InlineData(0x1EAF, 0x0003, 0x0000, BootloaderType.Stm32Duino)]
    // Pololu
    [InlineData(0x1FFB, 0x0101, 0x0000, BootloaderType.Caterina)]
    // Arduino
    [InlineData(0x2341, 0x0036, 0x0000, BootloaderType.Caterina)]
    [InlineData(0x2341, 0x0037, 0x0000, BootloaderType.Caterina)]
    // Adafruit
    [InlineData(0x239A, 0x000C, 0x0000, BootloaderType.Caterina)]
    [InlineData(0x239A, 0x000D, 0x0000, BootloaderType.Caterina)]
    [InlineData(0x239A, 0x000E, 0x0000, BootloaderType.Caterina)]
    // GigaDevice
    [InlineData(0x28E9, 0x0189, 0x0000, BootloaderType.Gd32VDfu)]
    // dog hunter
    [InlineData(0x2A03, 0x0036, 0x0000, BootloaderType.Caterina)]
    [InlineData(0x2A03, 0x0037, 0x0000, BootloaderType.Caterina)]
    [InlineData(0x2A03, 0x0040, 0x0000, BootloaderType.Caterina)]
    // Geehy
    [InlineData(0x314B, 0x0106, 0x0000, BootloaderType.Apm32Dfu)]
    // WestBerryTech
    [InlineData(0x342D, 0xDFA0, 0x0000, BootloaderType.Wb32Dfu)]
    // Raspberry Pi
    [InlineData(0x2E8A, 0x0003, 0x0000, BootloaderType.Picotool)]
    [InlineData(0x2E8A, 0x000F, 0x0000, BootloaderType.Picotool)]
    public void GetDeviceType_ReturnsExpected(ushort vid, ushort pid, ushort rev, BootloaderType expected) => Assert.Equal(expected, BootloaderFactory.GetDeviceType(vid, pid, rev));

    [Fact]
    public void GetDeviceType_UnknownVidPid_ReturnsNone() => Assert.Equal(BootloaderType.None, BootloaderFactory.GetDeviceType(0xFFFF, 0xFFFF, 0));

    // ── CreateDevice ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0x03EB, 0x2FEF, 0x0001, BootloaderType.AtmelDfu)]
    [InlineData(0x03EB, 0x2FEF, 0x0936, BootloaderType.QmkDfu)]
    [InlineData(0x03EB, 0x6124, 0x0000, BootloaderType.AtmelSamBa)]
    [InlineData(0x0483, 0xDF11, 0x0000, BootloaderType.Stm32Dfu)]
    [InlineData(0x1209, 0x2302, 0x0000, BootloaderType.Caterina)]
    [InlineData(0x16C0, 0x0478, 0x0000, BootloaderType.HalfKay)]
    [InlineData(0x16C0, 0x0483, 0x0000, BootloaderType.AvrIsp)]
    [InlineData(0x16C0, 0x05DC, 0x0000, BootloaderType.UsbAsp)]
    [InlineData(0x16C0, 0x05DF, 0x0000, BootloaderType.BootloadHid)]
    [InlineData(0x1781, 0x0C9F, 0x0000, BootloaderType.UsbTinyIsp)]
    [InlineData(0x1C11, 0xB007, 0x0000, BootloaderType.KiibohdDfu)]
    [InlineData(0x1EAF, 0x0003, 0x0000, BootloaderType.Stm32Duino)]
    [InlineData(0x28E9, 0x0189, 0x0000, BootloaderType.Gd32VDfu)]
    [InlineData(0x314B, 0x0106, 0x0000, BootloaderType.Apm32Dfu)]
    [InlineData(0x342D, 0xDFA0, 0x0000, BootloaderType.Wb32Dfu)]
    [InlineData(0x03EB, 0x2045, 0x0000, BootloaderType.LufaMs)]
    [InlineData(0x03EB, 0x2067, 0x0936, BootloaderType.QmkHid)]
    [InlineData(0x03EB, 0x2067, 0x0001, BootloaderType.LufaHid)]
    [InlineData(0x2E8A, 0x0003, 0x0000, BootloaderType.Picotool)]
    [InlineData(0x2E8A, 0x000F, 0x0000, BootloaderType.Picotool)]
    public void CreateDevice_ReturnsBootloaderDeviceWithCorrectType(
        ushort vid, ushort pid, ushort rev, BootloaderType expected)
    {
        IUsbDevice device = Usb(vid, pid, rev);
        IFlashToolProvider toolProvider = Substitute.For<IFlashToolProvider>();

        BootloaderDevice? bd = BootloaderFactory.CreateDevice(device, toolProvider);

        Assert.NotNull(bd);
        Assert.Equal(expected, bd!.Type);
    }

    [Theory]
    [InlineData(0x0003, "Picotool (RP2040)")]
    [InlineData(0x000F, "Picotool (RP2350)")]
    public void CreateDevice_Picotool_NameReflectsChip(ushort pid, string expectedName)
    {
        IUsbDevice device = Usb(0x2E8A, pid);
        IFlashToolProvider toolProvider = Substitute.For<IFlashToolProvider>();

        BootloaderDevice? bd = BootloaderFactory.CreateDevice(device, toolProvider);

        Assert.NotNull(bd);
        Assert.Equal(expectedName, bd!.Name);
    }

    [Fact]
    public void CreateDevice_UnknownVidPid_ReturnsNull()
    {
        IUsbDevice device = Usb(0xFFFF, 0xFFFF);
        IFlashToolProvider toolProvider = Substitute.For<IFlashToolProvider>();

        BootloaderDevice? result = BootloaderFactory.CreateDevice(device, toolProvider);

        Assert.Null(result);
    }
}
