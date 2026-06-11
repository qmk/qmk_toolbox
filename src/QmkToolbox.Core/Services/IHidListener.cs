using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Services;

/// <summary>
/// Monitors HID devices and delivers raw HID console reports.
/// </summary>
public interface IHidListener : IDisposable
{
    /// <summary>Raised when a HID device is connected.</summary>
    event Action<IHidDevice> HidDeviceConnected;

    /// <summary>Raised when a HID device is disconnected.</summary>
    event Action<IHidDevice> HidDeviceDisconnected;

    /// <summary>Raised when a raw HID console report is received from a device.</summary>
    event Action<IHidDevice, string> ConsoleReportReceived;

    /// <summary>Raised when an error occurs in the listener.</summary>
    event Action<string> ErrorOccurred;

    /// <summary>Starts monitoring for HID device events.</summary>
    void Start();

    /// <summary>Stops monitoring for HID device events.</summary>
    void Stop();
}
