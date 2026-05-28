using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader;

/// <summary>
/// Base class for bootloader devices that use avrdude for flashing.
/// Subclasses provide the programmer name, driver, and COM port requirements.
/// </summary>
internal abstract class AvrdudeDevice : BootloaderDevice
{
    private readonly string _programmer;
    // Port resolution starts immediately on device connect and runs in the background.
    // Both FlashAsync and FlashEepromAsync await the same Task, so resolution happens
    // at most once per device lifetime regardless of how many operations are performed.
    private readonly Task<string?>? _comPort;

    protected AvrdudeDevice(
        IUsbDevice device,
        IFlashToolProvider toolProvider,
        ISerialPortService? serialPortService,
        BootloaderType type,
        string name,
        string programmer,
        string preferredDriver,
        bool requiresComPort,
        bool isEepromFlashable)
        : base(device, toolProvider, serialPortService)
    {
        Type = type;
        Name = name;
        PreferredDriver = preferredDriver;
        IsEepromFlashable = isEepromFlashable;
        _programmer = programmer;
        _comPort = requiresComPort ? FindComPortAsync() : null;
    }

    public override Task WhenReadyAsync() => _comPort ?? Task.CompletedTask;

    public override string ToString() =>
        _comPort is { IsCompletedSuccessfully: true }
            ? $"{base.ToString()} [{_comPort.Result ?? "port not found"}]"
            : base.ToString();

    private string[] BuildArgs(string mcu, string target, string file, string? comPort) =>
        // The -U value is a single argument: "flash:w:/path/to/file:i"
        // ArgumentList passes it as one discrete argument, so no manual quoting is needed.
        _comPort != null
            ? ["-p", mcu, "-c", _programmer, "-U", $"{target}:w:{file}:i", "-P", comPort!]
            : ["-p", mcu, "-c", _programmer, "-U", $"{target}:w:{file}:i"];

    public override async Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".hex");
        string? comPort = _comPort != null ? RequireComPort(await _comPort.ConfigureAwait(false)) : null;
        await RunToolAsync("avrdude", BuildArgs(mcu, "flash", file, comPort));
    }

    public override async Task FlashEepromAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".eep", ".hex");
        string? comPort = _comPort != null ? RequireComPort(await _comPort.ConfigureAwait(false)) : null;
        await RunToolAsync("avrdude", BuildArgs(mcu, "eeprom", file, comPort));
    }
}
