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
    private readonly bool _requiresComPort;
    // Lazy: resolving the COM port via WMI on Windows is expensive. Defer until first use
    // and cache — Flash, FlashEeprom, and ToString all share the same resolved value so
    // only one WMI lookup ever occurs per device lifetime. A new device object is created
    // on every connect event, so the cached value is never stale.
    private readonly Lazy<string?>? _comPort;

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
        _requiresComPort = requiresComPort;
        _comPort = requiresComPort ? new Lazy<string?>(FindComPort) : null;
    }

    public override string ToString() =>
        _comPort != null ? $"{base.ToString()} [{_comPort.Value ?? "port not found"}]" : base.ToString();

    private string[] BuildArgs(string mcu, string target, string file, string? comPort) =>
        // The -U value is a single argument: "flash:w:/path/to/file:i"
        // ArgumentList passes it as one discrete argument, so no manual quoting is needed.
        _requiresComPort
            ? ["-p", mcu, "-c", _programmer, "-U", $"{target}:w:{file}:i", "-P", comPort!]
            : ["-p", mcu, "-c", _programmer, "-U", $"{target}:w:{file}:i"];

    public override Task FlashAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".hex");
        string? comPort = null;
        if (_requiresComPort)
            comPort = RequireComPort(_comPort!.Value);
        return RunToolAsync("avrdude", BuildArgs(mcu, "flash", file, comPort));
    }

    public override Task FlashEepromAsync(string mcu, string file)
    {
        ValidateFileExtension(file, ".eep", ".hex");
        string? comPort = null;
        if (_requiresComPort)
            comPort = RequireComPort(_comPort!.Value);
        return RunToolAsync("avrdude", BuildArgs(mcu, "eeprom", file, comPort));
    }
}
