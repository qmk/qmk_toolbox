using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Core.Bootloader;

/// <summary>
/// Abstract base class for all bootloader device implementations.
/// Wraps an <see cref="IUsbDevice"/> and provides common plumbing for flashing,
/// EEPROM operations, reset, and tool invocation.
/// </summary>
public abstract class BootloaderDevice(IUsbDevice device, IFlashToolProvider toolProvider, ISerialPortService? serialPortService = null)
{
    public const ushort QmkRevisionMarker = 0x0936;

    public delegate void FlashOutputReceivedDelegate(BootloaderDevice device, string data, MessageType type);

    public event FlashOutputReceivedDelegate? OutputReceived;

    public IUsbDevice Device { get; } = device;
    protected IFlashToolProvider ToolProvider { get; } = toolProvider;
    protected ISerialPortService? SerialPortService { get; } = serialPortService;

    public ushort VendorId => Device.VendorId;
    public ushort ProductId => Device.ProductId;
    public ushort RevisionBcd => Device.RevisionBcd;
    public string ManufacturerString => Device.ManufacturerString;
    public string ProductString => Device.ProductString;
    public string Driver => Device.Driver;
    public string DevicePath => Device.DevicePath;

    public string PreferredDriver { get; init; } = "";
    public bool IsEepromFlashable { get; init; }
    public bool IsResettable { get; init; }
    public BootloaderType Type { get; init; }
    public string Name { get; init; } = "";

    public override string ToString() =>
        $"{ManufacturerString} {ProductString} ({VendorId:X4}:{ProductId:X4}:{RevisionBcd:X4})";

    public abstract Task FlashAsync(string mcu, string file);

    public virtual Task FlashEepromAsync(string mcu, string file) => Task.CompletedTask;

    public virtual Task ResetAsync(string mcu) => Task.CompletedTask;

    protected async Task<int> RunToolAsync(string toolName, params string[] args) =>
        await FlashService.RunToolAsync(toolName, args, ToolProvider, PrintMessage).ConfigureAwait(false);

    protected void PrintMessage(string message, MessageType type) =>
        OutputReceived?.Invoke(this, message, type);

    /// <summary>
    /// Throws <see cref="UnsupportedFileFormatException"/> if the file's extension is not in the accepted list.
    /// </summary>
    protected static void ValidateFileExtension(string file, params string[] extensions)
    {
        string ext = Path.GetExtension(file);
        foreach (string allowed in extensions)
        {
            if (string.Equals(ext, allowed, StringComparison.OrdinalIgnoreCase))
                return;
        }
        throw new UnsupportedFileFormatException(file, extensions);
    }

    protected string? FindComPort() => SerialPortService?.FindSerialPort(Device);

    /// <summary>
    /// Returns <paramref name="comPort"/> if non-null, or throws <see cref="ComPortNotFoundException"/>.
    /// </summary>
    protected string RequireComPort(string? comPort) =>
        comPort ?? throw new ComPortNotFoundException(Name);
}
