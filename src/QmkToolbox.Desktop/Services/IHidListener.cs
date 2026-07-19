using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Raises HID device hotplug and console-report events. Events fire on the listener's own
/// thread — consumers must marshal to the UI thread. Disposing stops listening and releases
/// all tracked devices.
/// </summary>
public interface IHidListener : IDisposable
{
    /// <summary>Raised when an HID device is connected.</summary>
    event Action<IHidDevice> HidDeviceConnected;

    /// <summary>Raised when an HID device is disconnected.</summary>
    event Action<IHidDevice> HidDeviceDisconnected;

    /// <summary>Raised when a console report is received from a QMK console device.</summary>
    event Action<IHidDevice, string> ConsoleReportReceived;

    /// <summary>Raised when the listener fails unexpectedly.</summary>
    event Action<string> ErrorOccurred;

    /// <summary>Starts listening for HID devices.</summary>
    void Start();
}
