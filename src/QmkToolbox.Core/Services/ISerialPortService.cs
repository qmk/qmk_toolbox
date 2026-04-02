using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Services;

/// <summary>
/// Resolves the serial port name associated with a USB device.
/// </summary>
public interface ISerialPortService
{
    /// <summary>
    /// Returns the serial port name (e.g. <c>/dev/ttyACM0</c> or <c>COM3</c>) for
    /// <paramref name="device"/>, or <see langword="null"/> if none can be found.
    /// </summary>
    string? FindSerialPort(IUsbDevice device);
}
