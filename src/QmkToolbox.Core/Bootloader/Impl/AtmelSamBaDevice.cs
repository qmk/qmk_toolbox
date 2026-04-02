using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>Atmel SAM-BA bootloader device (Massdrop, via mdloader).</summary>
internal sealed class AtmelSamBaDevice : BootloaderDevice
{
    // Lazy: resolving the COM port via WMI on Windows is expensive. Defer until first use
    // and cache — Flash, Reset, and ToString all share the same resolved value so only one
    // WMI lookup ever occurs per device lifetime. A new device object is created on every
    // connect event, so the cached value is never stale.
    private readonly Lazy<string?> _comPort;

    public AtmelSamBaDevice(IUsbDevice device, IFlashToolProvider toolProvider, ISerialPortService? serialPortService = null)
        : base(device, toolProvider, serialPortService)
    {
        Type = BootloaderType.AtmelSamBa;
        Name = "Atmel SAM-BA";
        PreferredDriver = "usbser";
        IsResettable = true;
        _comPort = new Lazy<string?>(FindComPort);
    }

    public override Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".bin");
        string port = RequireComPort(_comPort.Value);
        return RunToolAsync("mdloader", "-p", port, "-D", file, "--restart");
    }

    public override Task ResetAsync(string mcu)
    {
        string port = RequireComPort(_comPort.Value);
        return RunToolAsync("mdloader", "-p", port, "--restart");
    }

    public override string ToString() => $"{base.ToString()} [{_comPort.Value ?? "port not found"}]";
}
