using NSubstitute;
using QmkToolbox.Core.Bootloader;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Verifies the CLI command strings produced by each bootloader device class.
///
/// Strategy: IFlashToolProvider.GetToolPath() returns "/bin/true" so the child
/// process starts and exits immediately without side-effects.  The full command
/// string ("{toolName} {args}") is captured via the OutputReceived delegate,
/// which FlashService emits as MessageType.Command before launching the process.
/// </summary>
public class FlashCommandTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static IUsbDevice Usb(ushort vid, ushort pid, ushort rev = 0) =>
        new UsbDeviceInfo(vid, pid, rev, "", "", "", "");

    private static IFlashToolProvider MockToolProvider()
    {
        IFlashToolProvider p = Substitute.For<IFlashToolProvider>();
        p.GetToolPath(Arg.Any<string>()).Returns("/bin/true");
        p.GetResourceFolder().Returns(Path.GetTempPath());
        return p;
    }

    private static ISerialPortService MockSerialPort(string port = "ttyACM0")
    {
        ISerialPortService s = Substitute.For<ISerialPortService>();
        s.FindSerialPort(Arg.Any<IUsbDevice>()).Returns(port);
        return s;
    }

    /// <summary>Creates a device via the factory and collects MessageType.Command messages.</summary>
    private static async Task<List<string>> Commands(
        IUsbDevice usb,
        IFlashToolProvider tool,
        ISerialPortService? serial,
        IMountPointService? mount,
        Func<BootloaderDevice, Task> action)
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(usb, tool, serial, mount)!;
        var cmds = new List<string>();
        bd.OutputReceived += (_, data, type) => { if (type == MessageType.Command) cmds.Add(data); };
        await action(bd);
        return cmds;
    }

    // ── AtmelDfuDevice ────────────────────────────────────────────────────────

    [Fact]
    public async Task AtmelDfuDevice_Flash_ThreeSequentialCommands()
    {
        List<string> cmds = await Commands(
            Usb(0x03EB, 0x2FEF, 0), MockToolProvider(), null, null,
            bd => bd.FlashAsync("at90usb1286", "test.hex"));

        Assert.Equal(3, cmds.Count);
        Assert.Equal("dfu-programmer at90usb1286 erase --force", cmds[0]);
        Assert.Equal("dfu-programmer at90usb1286 flash --force test.hex", cmds[1]);
        Assert.Equal("dfu-programmer at90usb1286 reset", cmds[2]);
    }

    [Fact]
    public async Task AtmelDfuDevice_FlashEeprom_IncludesErase()
    {
        List<string> cmds = await Commands(
            Usb(0x03EB, 0x2FEF, 0), MockToolProvider(), null, null,
            bd => bd.FlashEepromAsync("at90usb1286", "reset.eep"));

        Assert.Equal(2, cmds.Count);
        Assert.Equal("dfu-programmer at90usb1286 erase --force", cmds[0]);
        Assert.Equal("dfu-programmer at90usb1286 flash --force --suppress-validation --eeprom reset.eep", cmds[1]);
    }

    [Fact]
    public async Task QmkDfuDevice_FlashEeprom_NoErase()
    {
        List<string> cmds = await Commands(
            Usb(0x03EB, 0x2FEF, 0x0936), MockToolProvider(), null, null,
            bd => bd.FlashEepromAsync("at90usb1286", "reset.eep"));

        Assert.Single(cmds);
        Assert.Equal("dfu-programmer at90usb1286 flash --force --suppress-validation --eeprom reset.eep", cmds[0]);
    }

    [Fact]
    public async Task AtmelDfuDevice_FlashEeprom_RejectsUnsupportedFormat()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x03EB, 0x2FEF, 0), MockToolProvider())!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashEepromAsync("at90usb1286", "firmware.uf2"));
        Assert.Contains(".eep", ex.Message);
    }

    [Fact]
    public async Task AtmelDfuDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x03EB, 0x2FEF, 0), MockToolProvider(), null, null,
            bd => bd.ResetAsync("at90usb1286"));

        Assert.Single(cmds);
        Assert.Equal("dfu-programmer at90usb1286 reset", cmds[0]);
    }

    // ── Apm32DfuDevice ────────────────────────────────────────────────────────

    [Fact]
    public async Task Apm32DfuDevice_Flash_Bin()
    {
        List<string> cmds = await Commands(
            Usb(0x314B, 0x0106), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.bin"));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 314B:0106 -s 0x08000000:leave -D test.bin", cmds[0]);
    }

    [Fact]
    public async Task Apm32DfuDevice_Flash_NonBin_IsRejected()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x314B, 0x0106), MockToolProvider())!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashAsync("", "test.hex"));
        Assert.Contains(".bin", ex.Message);
    }

    [Fact]
    public async Task Apm32DfuDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x314B, 0x0106), MockToolProvider(), null, null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 314B:0106 -s 0x08000000:leave", cmds[0]);
    }

    // ── At32DfuDevice ─────────────────────────────────────────────────────────

    [Fact]
    public async Task At32DfuDevice_Flash_Bin()
    {
        List<string> cmds = await Commands(
            Usb(0x2E3C, 0xDF11), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.bin"));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 2E3C:DF11 -s 0x08000000:leave -D test.bin", cmds[0]);
    }

    [Fact]
    public async Task At32DfuDevice_Flash_NonBin_IsRejected()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x2E3C, 0xDF11), MockToolProvider())!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashAsync("", "test.hex"));
        Assert.Contains(".bin", ex.Message);
    }

    [Fact]
    public async Task At32DfuDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x2E3C, 0xDF11), MockToolProvider(), null, null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 2E3C:DF11 -s 0x08000000:leave", cmds[0]);
    }

    // ── AtmelSamBaDevice ──────────────────────────────────────────────────────

    [Fact]
    public async Task AtmelSamBaDevice_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x03EB, 0x6124), MockToolProvider(), MockSerialPort("ttyACM0"), null,
            bd => bd.FlashAsync("", "test.bin"));

        Assert.Single(cmds);
        Assert.Equal("mdloader -p ttyACM0 -D test.bin --restart", cmds[0]);
    }

    [Fact]
    public async Task AtmelSamBaDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x03EB, 0x6124), MockToolProvider(), MockSerialPort("ttyACM0"), null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("mdloader -p ttyACM0 --restart", cmds[0]);
    }

    [Fact]
    public async Task AtmelSamBaDevice_Flash_NoComPort_Throws()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x03EB, 0x6124), MockToolProvider(), null, null)!;
        await Assert.ThrowsAsync<ComPortNotFoundException>(() => bd.FlashAsync("", "test.bin"));
    }

    [Fact]
    public async Task AtmelSamBaDevice_Reset_NoComPort_Throws()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x03EB, 0x6124), MockToolProvider(), null, null)!;
        await Assert.ThrowsAsync<ComPortNotFoundException>(() => bd.ResetAsync(""));
    }

    // ── AvrIspDevice ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AvrIspDevice_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x16C0, 0x0483), MockToolProvider(), MockSerialPort("ttyACM0"), null,
            bd => bd.FlashAsync("atmega32u4", "test.hex"));

        Assert.Single(cmds);
        Assert.Equal("avrdude -p atmega32u4 -c avrisp -U flash:w:test.hex:i -P ttyACM0", cmds[0]);
    }

    // ── BootloadHidDevice ─────────────────────────────────────────────────────

    [Fact]
    public async Task BootloadHidDevice_Flash_RejectsUnsupportedFormat()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x16C0, 0x05DF), MockToolProvider())!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashAsync("", "test.uf2"));
        Assert.Contains(".hex", ex.Message);
    }

    [Fact]
    public async Task BootloadHidDevice_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x16C0, 0x05DF), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.hex"));

        Assert.Single(cmds);
        Assert.Equal("bootloadHID -r test.hex", cmds[0]);
    }

    [Fact]
    public async Task BootloadHidDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x16C0, 0x05DF), MockToolProvider(), null, null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("bootloadHID -r", cmds[0]);
    }

    // ── CaterinaDevice ────────────────────────────────────────────────────────

    [Fact]
    public async Task CaterinaDevice_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x1209, 0x2302), MockToolProvider(), MockSerialPort("ttyACM0"), null,
            bd => bd.FlashAsync("atmega32u4", "test.hex"));

        Assert.Single(cmds);
        Assert.Equal("avrdude -p atmega32u4 -c avr109 -U flash:w:test.hex:i -P ttyACM0", cmds[0]);
    }

    [Fact]
    public async Task CaterinaDevice_FlashEeprom()
    {
        List<string> cmds = await Commands(
            Usb(0x1209, 0x2302), MockToolProvider(), MockSerialPort("ttyACM0"), null,
            bd => bd.FlashEepromAsync("atmega32u4", "reset.eep"));

        Assert.Single(cmds);
        Assert.Equal("avrdude -p atmega32u4 -c avr109 -U eeprom:w:reset.eep:i -P ttyACM0", cmds[0]);
    }

    [Fact]
    public async Task CaterinaDevice_FlashEeprom_RejectsUnsupportedFormat()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x1209, 0x2302), MockToolProvider(), MockSerialPort())!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashEepromAsync("atmega32u4", "firmware.uf2"));
        Assert.Contains(".eep", ex.Message);
    }

    [Fact]
    public async Task CaterinaDevice_Flash_NoComPort_Throws()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x1209, 0x2302), MockToolProvider(), null, null)!;
        await Assert.ThrowsAsync<ComPortNotFoundException>(() => bd.FlashAsync("atmega32u4", "test.hex"));
    }

    // ── Gd32VDfuDevice ────────────────────────────────────────────────────────

    [Fact]
    public async Task Gd32VDfuDevice_Flash_Bin()
    {
        List<string> cmds = await Commands(
            Usb(0x28E9, 0x0189), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.bin"));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 28E9:0189 -s 0x08000000:leave -D test.bin", cmds[0]);
    }

    [Fact]
    public async Task Gd32VDfuDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x28E9, 0x0189), MockToolProvider(), null, null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 28E9:0189 -s 0x08000000:leave", cmds[0]);
    }

    // ── HalfKayDevice ─────────────────────────────────────────────────────────

    [Fact]
    public async Task HalfKayDevice_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x16C0, 0x0478), MockToolProvider(), null, null,
            bd => bd.FlashAsync("at90usb1286", "test.hex"));

        Assert.Single(cmds);
        Assert.Equal("teensy_loader_cli -mmcu=at90usb1286 test.hex -v", cmds[0]);
    }

    [Fact]
    public async Task HalfKayDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x16C0, 0x0478), MockToolProvider(), null, null,
            bd => bd.ResetAsync("at90usb1286"));

        Assert.Single(cmds);
        Assert.Equal("teensy_loader_cli -mmcu=at90usb1286 -bv", cmds[0]);
    }

    // ── KiibohdDfuDevice ──────────────────────────────────────────────────────

    [Fact]
    public async Task KiibohdDfuDevice_Flash_Bin()
    {
        List<string> cmds = await Commands(
            Usb(0x1C11, 0xB007), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.bin"));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 1C11:B007 -D test.bin", cmds[0]);
    }

    [Fact]
    public async Task KiibohdDfuDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x1C11, 0xB007), MockToolProvider(), null, null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 1C11:B007 -e", cmds[0]);
    }

    // ── LufaHidDevice ─────────────────────────────────────────────────────────

    [Fact]
    public async Task LufaHidDevice_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x03EB, 0x2067, 0), MockToolProvider(), null, null,
            bd => bd.FlashAsync("atmega32u4", "test.hex"));

        Assert.Single(cmds);
        Assert.Equal("hid_bootloader_cli -mmcu=atmega32u4 test.hex -v", cmds[0]);
    }

    // ── LufaMsDevice ──────────────────────────────────────────────────────────

    [Fact]
    public async Task LufaMsDevice_Flash_CopiesFileToMountPoint()
    {
        string mountDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(mountDir);
        string src = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bin");
        File.WriteAllBytes(src, [0x01, 0x02, 0x03]);

        try
        {
            IMountPointService mount = Substitute.For<IMountPointService>();
            mount.FindMountPoint(Arg.Any<IUsbDevice>()).Returns(mountDir);

            BootloaderDevice bd = BootloaderFactory.CreateDevice(
                Usb(0x03EB, 0x2045), MockToolProvider(), null, mount)!;

            await bd.FlashAsync("", src);

            string dest = Path.Combine(mountDir, "FLASH.BIN");
            Assert.True(File.Exists(dest));
            Assert.Equal(File.ReadAllBytes(src), File.ReadAllBytes(dest));
        }
        finally
        {
            if (Directory.Exists(mountDir))
                Directory.Delete(mountDir, true);
            if (File.Exists(src))
                File.Delete(src);
        }
    }

    [Fact]
    public async Task LufaMsDevice_Flash_RejectsNonBinFile()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(
            Usb(0x03EB, 0x2045), MockToolProvider(), null, null)!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashAsync("", "firmware.hex"));
        Assert.Contains(".bin", ex.Message);
    }

    // ── Stm32DfuDevice ────────────────────────────────────────────────────────

    [Fact]
    public async Task Stm32DfuDevice_Flash_Bin()
    {
        List<string> cmds = await Commands(
            Usb(0x0483, 0xDF11), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.bin"));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 0483:DF11 -s 0x08000000:leave -D test.bin", cmds[0]);
    }

    [Fact]
    public async Task Stm32DfuDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x0483, 0xDF11), MockToolProvider(), null, null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 0 -d 0483:DF11 -s 0x08000000:leave", cmds[0]);
    }

    // ── Stm32DuinoDevice ──────────────────────────────────────────────────────

    [Fact]
    public async Task Stm32DuinoDevice_Flash_Bin()
    {
        List<string> cmds = await Commands(
            Usb(0x1EAF, 0x0003), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.bin"));

        Assert.Single(cmds);
        Assert.Equal("dfu-util -a 2 -d 1EAF:0003 -R -D test.bin", cmds[0]);
    }

    // ── UsbAspDevice ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UsbAspDevice_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x16C0, 0x05DC), MockToolProvider(), null, null,
            bd => bd.FlashAsync("atmega32u4", "test.hex"));

        Assert.Single(cmds);
        Assert.Equal("avrdude -p atmega32u4 -c usbasp -U flash:w:test.hex:i", cmds[0]);
    }

    [Fact]
    public async Task UsbAspDevice_FlashEeprom()
    {
        List<string> cmds = await Commands(
            Usb(0x16C0, 0x05DC), MockToolProvider(), null, null,
            bd => bd.FlashEepromAsync("atmega32u4", "reset.eep"));

        Assert.Single(cmds);
        Assert.Equal("avrdude -p atmega32u4 -c usbasp -U eeprom:w:reset.eep:i", cmds[0]);
    }

    [Fact]
    public async Task UsbAspDevice_FlashEeprom_RejectsUnsupportedFormat()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x16C0, 0x05DC), MockToolProvider())!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashEepromAsync("atmega32u4", "firmware.uf2"));
        Assert.Contains(".eep", ex.Message);
    }

    // ── UsbTinyIspDevice ──────────────────────────────────────────────────────

    [Fact]
    public async Task UsbTinyIspDevice_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x1781, 0x0C9F), MockToolProvider(), null, null,
            bd => bd.FlashAsync("atmega32u4", "test.hex"));

        Assert.Single(cmds);
        Assert.Equal("avrdude -p atmega32u4 -c usbtiny -U flash:w:test.hex:i", cmds[0]);
    }

    [Fact]
    public async Task UsbTinyIspDevice_FlashEeprom()
    {
        List<string> cmds = await Commands(
            Usb(0x1781, 0x0C9F), MockToolProvider(), null, null,
            bd => bd.FlashEepromAsync("atmega32u4", "reset.eep"));

        Assert.Single(cmds);
        Assert.Equal("avrdude -p atmega32u4 -c usbtiny -U eeprom:w:reset.eep:i", cmds[0]);
    }

    // ── PicotoolDevice ────────────────────────────────────────────────────────

    [Theory]
    [InlineData("test.uf2")]
    [InlineData("test.bin")]
    public async Task PicotoolDevice_Flash_AcceptedFormats(string filename)
    {
        List<string> cmds = await Commands(
            Usb(0x2E8A, 0x0003), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", filename));

        Assert.Equal(2, cmds.Count);
        Assert.Equal($"picotool load {filename}", cmds[0]);
        Assert.Equal("picotool reboot", cmds[1]);
    }

    [Fact]
    public async Task PicotoolDevice_Flash_RejectsHex()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x2E8A, 0x0003), MockToolProvider())!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashAsync("", "test.hex"));
        Assert.Contains(".uf2", ex.Message);
    }

    [Fact]
    public async Task PicotoolDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x2E8A, 0x0003), MockToolProvider(), null, null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("picotool reboot", cmds[0]);
    }

    [Fact]
    public async Task PicotoolDevice_Rp2350_Flash()
    {
        List<string> cmds = await Commands(
            Usb(0x2E8A, 0x000F), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.uf2"));

        Assert.Equal(2, cmds.Count);
        Assert.Equal("picotool load test.uf2", cmds[0]);
        Assert.Equal("picotool reboot", cmds[1]);
    }

    // ── Wb32DfuDevice ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Wb32DfuDevice_Flash_RejectsUnsupportedFormat()
    {
        BootloaderDevice bd = BootloaderFactory.CreateDevice(Usb(0x342D, 0xDFA0), MockToolProvider())!;

        UnsupportedFileFormatException ex = await Assert.ThrowsAsync<UnsupportedFileFormatException>(() => bd.FlashAsync("", "test.uf2"));
        Assert.Contains(".bin", ex.Message);
    }

    [Fact]
    public async Task Wb32DfuDevice_Flash_Bin()
    {
        List<string> cmds = await Commands(
            Usb(0x342D, 0xDFA0), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.bin"));

        Assert.Single(cmds);
        Assert.Equal("wb32-dfu-updater_cli --toolbox-mode --dfuse-address 0x08000000 --download test.bin", cmds[0]);
    }

    [Fact]
    public async Task Wb32DfuDevice_Flash_Hex()
    {
        List<string> cmds = await Commands(
            Usb(0x342D, 0xDFA0), MockToolProvider(), null, null,
            bd => bd.FlashAsync("", "test.hex"));

        Assert.Single(cmds);
        Assert.Equal("wb32-dfu-updater_cli --toolbox-mode --download test.hex", cmds[0]);
    }

    [Fact]
    public async Task Wb32DfuDevice_Reset()
    {
        List<string> cmds = await Commands(
            Usb(0x342D, 0xDFA0), MockToolProvider(), null, null,
            bd => bd.ResetAsync(""));

        Assert.Single(cmds);
        Assert.Equal("wb32-dfu-updater_cli --reset", cmds[0]);
    }
}
