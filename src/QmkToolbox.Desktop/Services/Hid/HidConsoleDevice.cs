using System.Diagnostics;
using System.Text;
using HidApi;

namespace QmkToolbox.Desktop.Services.Hid;

/// <summary>
/// Represents a QMK HID Console device (usage page 0xFF31, usage 0x0074).
/// Opens the device on construction and continuously reads console output on a background
/// thread, raising <see cref="ConsoleReportReceived"/> with the decoded text of each report.
/// The console is a raw byte stream chunked into null-padded USB reports; the consumer's
/// terminal buffer interprets '\r'/'\n'.
/// </summary>
public sealed class HidConsoleDevice : BaseHidDevice, IDisposable
{
    public const ushort TargetUsagePage = 0xFF31;
    public const ushort TargetUsage = 0x0074;

    /// <inheritdoc />
    public override bool IsConsoleDevice => true;

    public static bool Match(DeviceInfo d) =>
        d.UsagePage == TargetUsagePage && d.Usage == TargetUsage;

    public static BaseHidDevice? TryCreate(DeviceInfo d) =>
        Match(d) ? new HidConsoleDevice(d) : null;

    public event Action<HidConsoleDevice, string>? ConsoleReportReceived;

    private CancellationTokenSource? _cts;
    private readonly Task? _readTask;
    // UTF-8 decoder preserves state across HID reports so multi-byte characters
    // that span a report boundary are decoded correctly.
    private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();

    public HidConsoleDevice(DeviceInfo deviceInfo) : base(deviceInfo)
    {
        _cts = new CancellationTokenSource();
        // ReadLoop uses blocking synchronous HID reads (ReadTimeout); Task.Run offloads
        // it to a thread pool thread so the constructor doesn't block the UI thread.
        _readTask = Task.Run(() => ReadLoop(_cts.Token), _cts.Token);
    }

    private void ReadLoop(CancellationToken token)
    {
        try
        {
            using var device = new Device(DevicePath);
            byte[] buffer = new byte[65];
            char[] charBuffer = new char[Encoding.UTF8.GetMaxCharCount(65)];
            while (!token.IsCancellationRequested)
            {
                int bytesRead = device.ReadTimeout(buffer, 100);
                if (bytesRead <= 0)
                    continue;

                // HID reports are null-padded — truncate at first null byte.
                int validBytes = bytesRead;
                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == 0)
                    { validBytes = i; break; }
                }
                if (validBytes == 0)
                    continue;

                int charCount = _decoder.GetChars(buffer, 0, validBytes, charBuffer, 0);
                if (charCount > 0)
                    ConsoleReportReceived?.Invoke(this, new string(charBuffer, 0, charCount));
            }
        }
        catch (Exception ex) when (ex is HidException or IOException or ObjectDisposedException)
        {
            // Device disconnected or read error — stop gracefully
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HidConsoleDevice.ReadLoop unexpected exception: {ex}");
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();

        try
        {
            _readTask?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // ReadLoop faulted/cancelled — nothing left to wait on.
        }
        _cts?.Dispose();
        _cts = null;
    }
}
