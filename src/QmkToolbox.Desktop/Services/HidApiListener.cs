using HidApi;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using QmkToolbox.Desktop.Services.Hid;

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Polling-based HID listener using HidApi.Net (no hotplug callbacks available).
/// Polls every 500ms and fires device connect/disconnect events.
/// </summary>
public class HidApiListener : IHidListener
{
    public event Action<IHidDevice>? HidDeviceConnected;
    public event Action<IHidDevice>? HidDeviceDisconnected;
    public event Action<IHidDevice, string>? ConsoleReportReceived;
    public event Action<string>? ErrorOccurred;

    private readonly List<BaseHidDevice> _devices = [];
    private readonly Lock _deviceLock = new();
    private CancellationTokenSource? _cts;

    public void Start()
    {
        HidApi.Hid.Init();
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;
        // The polling loop runs indefinitely; Task.Run moves it to a thread pool thread
        // so it never blocks the UI thread.
        Task.Run(async () =>
        {
            try
            {
                Poll();
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(500, token);
                    }
                    catch (OperationCanceledException) { break; }
                    if (!token.IsCancellationRequested)
                        Poll();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ErrorOccurred?.Invoke($"HID polling stopped unexpectedly: {ex.Message}");
            }
        }, token);
    }

    public void Stop() => _cts?.Cancel();

    private void Poll()
    {
        var all = HidApi.Hid.Enumerate().ToList();
        var current = all
            .Where(HidConsoleDevice.Match)
            .ToList();
        List<BaseHidDevice> disconnected;
        List<BaseHidDevice> connected = [];

        lock (_deviceLock)
        {
            // Disconnected — key on (path, usagePage, usage) so two collections sharing
            // the same hidraw path (Linux multi-collection devices) are tracked independently
            var currentKeys = current.Select(d => (d.Path, d.UsagePage, d.Usage)).ToHashSet();
            disconnected = [.. _devices.Where(d => !currentKeys.Contains((d.DevicePath, d.UsagePage, d.Usage)))];
            foreach (BaseHidDevice device in disconnected)
                _devices.Remove(device);

            // Connected
            var knownKeys = _devices.Select(d => (d.DevicePath, d.UsagePage, d.Usage)).ToHashSet();
            foreach (DeviceInfo? info in current)
            {
                if (knownKeys.Contains((info.Path, info.UsagePage, info.Usage)))
                    continue;
                BaseHidDevice? device = CreateDevice(info);
                if (device == null)
                    continue;
                _devices.Add(device);
                if (device is HidConsoleDevice console)
                    console.ConsoleReportReceived += (d, data) => ConsoleReportReceived?.Invoke(d, data);
                connected.Add(device);
            }
        }

        // Raise events outside the lock to avoid potential deadlocks
        foreach (BaseHidDevice device in disconnected)
        {
            if (device is HidConsoleDevice c)
                c.Dispose();
            HidDeviceDisconnected?.Invoke(device);
        }
        foreach (BaseHidDevice device in connected)
            HidDeviceConnected?.Invoke(device);
    }

    private static BaseHidDevice? CreateDevice(DeviceInfo d) =>
        HidConsoleDevice.TryCreate(d);

    public void Dispose()
    {
        Stop();
        lock (_deviceLock)
        {
            foreach (BaseHidDevice device in _devices)
                (device as IDisposable)?.Dispose();
            _devices.Clear();
        }
        HidApi.Hid.Exit();
    }
}
