using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Services;

/// <summary>
/// Monitors USB device arrival and removal events.
/// </summary>
public interface IUsbDetector : IDisposable
{
    /// <summary>Raised when a USB device is connected.</summary>
    event Action<IUsbDevice> DeviceConnected;

    /// <summary>Raised when a USB device is disconnected.</summary>
    event Action<IUsbDevice> DeviceDisconnected;

    /// <summary>Starts monitoring for USB device events.</summary>
    void Start();

    /// <summary>Stops monitoring for USB device events.</summary>
    void Stop();
}
