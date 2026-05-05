using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader.Impl;

/// <summary>Atmel SAM-BA bootloader device (Massdrop, via mdloader).</summary>
internal sealed class AtmelSamBaDevice : BootloaderDevice
{
    // Port resolution starts immediately on device connect and runs in the background.
    // FlashAsync and ResetAsync await the same Task, so resolution happens at most once.
    private readonly Task<string?> _comPort;

    public AtmelSamBaDevice(IUsbDevice device, IFlashToolProvider toolProvider, ISerialPortService? serialPortService = null)
        : base(device, toolProvider, serialPortService)
    {
        Type = BootloaderType.AtmelSamBa;
        Name = "Atmel SAM-BA";
        PreferredDriver = "usbser";
        IsResettable = true;
        _comPort = FindComPortAsync();
    }

    public override async Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".bin");
        string port = RequireComPort(await _comPort.ConfigureAwait(false));
        await RunToolAsync("mdloader", "-p", port, "-D", file, "--restart");
    }

    public override async Task ResetAsync(string mcu)
    {
        string port = RequireComPort(await _comPort.ConfigureAwait(false));
        await RunToolAsync("mdloader", "-p", port, "--restart");
    }

    public override Task WhenReadyAsync() => _comPort;

    public override string ToString() =>
        _comPort.IsCompletedSuccessfully
            ? $"{base.ToString()} [{_comPort.Result ?? "port not found"}]"
            : base.ToString();
}
