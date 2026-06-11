using QmkToolbox.Core.Bootloader.Impl;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader;

public static class BootloaderFactory
{
    /// <summary>
    /// Lookup entry for a VID/PID pair. When <see cref="RevisionSelector"/> is non-null,
    /// the revision is passed through to choose between two bootloader types (e.g.
    /// QMK DFU vs plain Atmel DFU). Otherwise <see cref="Type"/> is returned directly.
    /// </summary>
    private readonly record struct DeviceEntry(
        BootloaderType Type,
        Func<ushort, BootloaderType>? RevisionSelector = null)
    {
        public BootloaderType Resolve(ushort rev) =>
            RevisionSelector?.Invoke(rev) ?? Type;
    }

    private static BootloaderType QmkOrAtmelDfu(ushort rev) =>
        rev == BootloaderDevice.QmkRevisionMarker ? BootloaderType.QmkDfu : BootloaderType.AtmelDfu;

    private static BootloaderType QmkOrLufaHid(ushort rev) =>
        rev == BootloaderDevice.QmkRevisionMarker ? BootloaderType.QmkHid : BootloaderType.LufaHid;

    private static readonly Dictionary<(ushort VID, ushort PID), DeviceEntry> DeviceMap = new()
    {
        // ── Atmel Corporation (0x03EB) ──────────────────────────────────
        [(0x03EB, 0x2045)] = new(BootloaderType.LufaMs),
        [(0x03EB, 0x2067)] = new(BootloaderType.LufaHid, QmkOrLufaHid),
        [(0x03EB, 0x2FEF)] = new(BootloaderType.AtmelDfu, QmkOrAtmelDfu), // ATmega16U2
        [(0x03EB, 0x2FF0)] = new(BootloaderType.AtmelDfu, QmkOrAtmelDfu), // ATmega32U2
        [(0x03EB, 0x2FF3)] = new(BootloaderType.AtmelDfu, QmkOrAtmelDfu), // ATmega16U4
        [(0x03EB, 0x2FF4)] = new(BootloaderType.AtmelDfu, QmkOrAtmelDfu), // ATmega32U4
        [(0x03EB, 0x2FF9)] = new(BootloaderType.AtmelDfu, QmkOrAtmelDfu), // AT90USB64
        [(0x03EB, 0x2FFA)] = new(BootloaderType.AtmelDfu, QmkOrAtmelDfu), // AT90USB162
        [(0x03EB, 0x2FFB)] = new(BootloaderType.AtmelDfu, QmkOrAtmelDfu), // AT90USB128
        [(0x03EB, 0x6124)] = new(BootloaderType.AtmelSamBa),

        // ── STMicroelectronics (0x0483) ─────────────────────────────────
        [(0x0483, 0xDF11)] = new(BootloaderType.Stm32Dfu),

        // ── pid.codes (0x1209) ──────────────────────────────────────────
        [(0x1209, 0x2302)] = new(BootloaderType.Caterina),

        // ── Van Ooijen Technische Informatica (0x16C0) ──────────────────
        [(0x16C0, 0x0478)] = new(BootloaderType.HalfKay),
        [(0x16C0, 0x0483)] = new(BootloaderType.AvrIsp),
        [(0x16C0, 0x05DC)] = new(BootloaderType.UsbAsp),
        [(0x16C0, 0x05DF)] = new(BootloaderType.BootloadHid),

        // ── MECANIQUE (0x1781) ──────────────────────────────────────────
        [(0x1781, 0x0C9F)] = new(BootloaderType.UsbTinyIsp),

        // ── Spark Fun Electronics (0x1B4F) ──────────────────────────────
        [(0x1B4F, 0x9203)] = new(BootloaderType.Caterina),
        [(0x1B4F, 0x9205)] = new(BootloaderType.Caterina),
        [(0x1B4F, 0x9207)] = new(BootloaderType.Caterina),

        // ── Input Club Inc. (0x1C11) ────────────────────────────────────
        [(0x1C11, 0xB007)] = new(BootloaderType.KiibohdDfu),

        // ── Leaflabs (0x1EAF) ───────────────────────────────────────────
        [(0x1EAF, 0x0003)] = new(BootloaderType.Stm32Duino),

        // ── Pololu Corporation (0x1FFB) ─────────────────────────────────
        [(0x1FFB, 0x0101)] = new(BootloaderType.Caterina),

        // ── Arduino SA (0x2341) ─────────────────────────────────────────
        [(0x2341, 0x0036)] = new(BootloaderType.Caterina),
        [(0x2341, 0x0037)] = new(BootloaderType.Caterina),

        // ── Adafruit (0x239A) ───────────────────────────────────────────
        [(0x239A, 0x000C)] = new(BootloaderType.Caterina),
        [(0x239A, 0x000D)] = new(BootloaderType.Caterina),
        [(0x239A, 0x000E)] = new(BootloaderType.Caterina),

        // ── dog hunter AG (0x2A03) ──────────────────────────────────────
        [(0x2A03, 0x0036)] = new(BootloaderType.Caterina),
        [(0x2A03, 0x0037)] = new(BootloaderType.Caterina),
        [(0x2A03, 0x0040)] = new(BootloaderType.Caterina),

        // ── GigaDevice Semiconductor (0x28E9) ──────────────────────────
        [(0x28E9, 0x0189)] = new(BootloaderType.Gd32VDfu),

        // ── ArteryTek (0x2E3C) ──────────────────────────────────────────
        [(0x2E3C, 0xDF11)] = new(BootloaderType.At32Dfu),

        // ── Geehy Semiconductor (0x314B) ────────────────────────────────
        [(0x314B, 0x0106)] = new(BootloaderType.Apm32Dfu),

        // ── WestBerryTech (0x342D) ──────────────────────────────────────
        [(0x342D, 0xDFA0)] = new(BootloaderType.Wb32Dfu),

        // ── Raspberry Pi (0x2E8A) ────────────────────────────────────────
        [(0x2E8A, 0x0003)] = new(BootloaderType.Picotool), // RP2040 BOOTSEL
        [(0x2E8A, 0x000F)] = new(BootloaderType.Picotool), // RP2350 BOOTSEL
    };

    public static BootloaderDevice? CreateDevice(
        IUsbDevice device,
        IFlashToolProvider toolProvider,
        ISerialPortService? serialPortService = null,
        IMountPointService? mountPointService = null)
    {
        return GetDeviceType(device.VendorId, device.ProductId, device.RevisionBcd) switch
        {
            BootloaderType.Apm32Dfu => new Apm32DfuDevice(device, toolProvider),
            BootloaderType.At32Dfu => new At32DfuDevice(device, toolProvider),
            BootloaderType.AtmelDfu => new AtmelDfuDevice(device, toolProvider, serialPortService),
            BootloaderType.AtmelSamBa => new AtmelSamBaDevice(device, toolProvider, serialPortService),
            BootloaderType.AvrIsp => new AvrIspDevice(device, toolProvider, serialPortService),
            BootloaderType.BootloadHid => new BootloadHidDevice(device, toolProvider),
            BootloaderType.Caterina => new CaterinaDevice(device, toolProvider, serialPortService),
            BootloaderType.Gd32VDfu => new Gd32VDfuDevice(device, toolProvider),
            BootloaderType.HalfKay => new HalfKayDevice(device, toolProvider),
            BootloaderType.KiibohdDfu => new KiibohdDfuDevice(device, toolProvider),
            BootloaderType.LufaHid => new LufaHidDevice(device, toolProvider),
            BootloaderType.LufaMs => new LufaMsDevice(device, toolProvider, mountPointService),
            BootloaderType.Stm32Dfu => new Stm32DfuDevice(device, toolProvider),
            BootloaderType.Stm32Duino => new Stm32DuinoDevice(device, toolProvider),
            BootloaderType.UsbAsp => new UsbAspDevice(device, toolProvider),
            BootloaderType.UsbTinyIsp => new UsbTinyIspDevice(device, toolProvider),
            BootloaderType.Wb32Dfu => new Wb32DfuDevice(device, toolProvider),
            BootloaderType.Picotool => new PicotoolDevice(device, toolProvider),
            BootloaderType.QmkDfu => new AtmelDfuDevice(device, toolProvider, serialPortService),
            BootloaderType.QmkHid => new LufaHidDevice(device, toolProvider),
            BootloaderType.None => null,
            var type => throw new ArgumentOutOfRangeException(nameof(BootloaderType), type, "No device implementation for this bootloader type"),
        };
    }

    public static BootloaderType GetDeviceType(ushort vendorId, ushort productId, ushort revisionBcd)
    {
        return DeviceMap.TryGetValue((vendorId, productId), out DeviceEntry entry)
            ? entry.Resolve(revisionBcd)
            : BootloaderType.None;
    }
}
