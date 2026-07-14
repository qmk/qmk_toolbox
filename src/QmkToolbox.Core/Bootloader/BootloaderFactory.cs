using QmkToolbox.Core.Bootloader.Impl;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader;

public static class BootloaderFactory
{
    private static readonly Dictionary<(ushort VID, ushort PID), BootloaderType> DeviceMap = new()
    {
        // ── Atmel Corporation (0x03EB) ──────────────────────────────────
        [(0x03EB, 0x2045)] = BootloaderType.LufaMs,
        [(0x03EB, 0x2067)] = BootloaderType.LufaHid,
        [(0x03EB, 0x2FEF)] = BootloaderType.AtmelDfu, // ATmega16U2
        [(0x03EB, 0x2FF0)] = BootloaderType.AtmelDfu, // ATmega32U2
        [(0x03EB, 0x2FF3)] = BootloaderType.AtmelDfu, // ATmega16U4
        [(0x03EB, 0x2FF4)] = BootloaderType.AtmelDfu, // ATmega32U4
        [(0x03EB, 0x2FF9)] = BootloaderType.AtmelDfu, // AT90USB64
        [(0x03EB, 0x2FFA)] = BootloaderType.AtmelDfu, // AT90USB162
        [(0x03EB, 0x2FFB)] = BootloaderType.AtmelDfu, // AT90USB128
        [(0x03EB, 0x6124)] = BootloaderType.AtmelSamBa,

        // ── STMicroelectronics (0x0483) ─────────────────────────────────
        [(0x0483, 0xDF11)] = BootloaderType.Stm32Dfu,

        // ── pid.codes (0x1209) ──────────────────────────────────────────
        [(0x1209, 0x2302)] = BootloaderType.Caterina,

        // ── Van Ooijen Technische Informatica (0x16C0) ──────────────────
        [(0x16C0, 0x0478)] = BootloaderType.HalfKay,
        [(0x16C0, 0x0483)] = BootloaderType.AvrIsp,
        [(0x16C0, 0x05DC)] = BootloaderType.UsbAsp,
        [(0x16C0, 0x05DF)] = BootloaderType.BootloadHid,

        // ── MECANIQUE (0x1781) ──────────────────────────────────────────
        [(0x1781, 0x0C9F)] = BootloaderType.UsbTinyIsp,

        // ── Spark Fun Electronics (0x1B4F) ──────────────────────────────
        [(0x1B4F, 0x9203)] = BootloaderType.Caterina,
        [(0x1B4F, 0x9205)] = BootloaderType.Caterina,
        [(0x1B4F, 0x9207)] = BootloaderType.Caterina,

        // ── Input Club Inc. (0x1C11) ────────────────────────────────────
        [(0x1C11, 0xB007)] = BootloaderType.KiibohdDfu,

        // ── Leaflabs (0x1EAF) ───────────────────────────────────────────
        [(0x1EAF, 0x0003)] = BootloaderType.Stm32Duino,

        // ── Pololu Corporation (0x1FFB) ─────────────────────────────────
        [(0x1FFB, 0x0101)] = BootloaderType.Caterina,

        // ── Arduino SA (0x2341) ─────────────────────────────────────────
        [(0x2341, 0x0036)] = BootloaderType.Caterina,
        [(0x2341, 0x0037)] = BootloaderType.Caterina,

        // ── Adafruit (0x239A) ───────────────────────────────────────────
        [(0x239A, 0x000C)] = BootloaderType.Caterina,
        [(0x239A, 0x000D)] = BootloaderType.Caterina,
        [(0x239A, 0x000E)] = BootloaderType.Caterina,

        // ── dog hunter AG (0x2A03) ──────────────────────────────────────
        [(0x2A03, 0x0036)] = BootloaderType.Caterina,
        [(0x2A03, 0x0037)] = BootloaderType.Caterina,
        [(0x2A03, 0x0040)] = BootloaderType.Caterina,

        // ── GigaDevice Semiconductor (0x28E9) ──────────────────────────
        [(0x28E9, 0x0189)] = BootloaderType.Gd32VDfu,

        // ── ArteryTek (0x2E3C) ──────────────────────────────────────────
        [(0x2E3C, 0xDF11)] = BootloaderType.At32Dfu,

        // ── Geehy Semiconductor (0x314B) ────────────────────────────────
        [(0x314B, 0x0106)] = BootloaderType.Apm32Dfu,

        // ── WestBerryTech (0x342D) ──────────────────────────────────────
        [(0x342D, 0xDFA0)] = BootloaderType.Wb32Dfu,

        // ── Raspberry Pi (0x2E8A) ────────────────────────────────────────
        [(0x2E8A, 0x0003)] = BootloaderType.Picotool, // RP2040 BOOTSEL
        [(0x2E8A, 0x000F)] = BootloaderType.Picotool, // RP2350 BOOTSEL
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
        if (!DeviceMap.TryGetValue((vendorId, productId), out BootloaderType type))
            return BootloaderType.None;
        // The QMK revision marker distinguishes QMK-flavoured bootloaders sharing stock VID/PIDs.
        if (revisionBcd == BootloaderDevice.QmkRevisionMarker)
        {
            if (type == BootloaderType.AtmelDfu)
                return BootloaderType.QmkDfu;
            if (type == BootloaderType.LufaHid)
                return BootloaderType.QmkHid;
        }
        return type;
    }
}
