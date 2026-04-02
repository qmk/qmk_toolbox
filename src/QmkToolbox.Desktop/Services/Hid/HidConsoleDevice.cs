using System.Diagnostics;
using System.Text;
using HidApi;

namespace QmkToolbox.Desktop.Services.Hid;

/// <summary>
/// Represents a QMK HID Console device (usage page 0xFF31, usage 0x0074).
/// Opens the device on construction and continuously reads console output
/// on a background thread, raising <see cref="ConsoleReportReceived"/> for each line.
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
    // UTF-8 decoder preserves state across HID reports so multi-byte characters
    // that span a report boundary are decoded correctly.
    private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();
    private readonly StringBuilder _lineBuffer = new();

    public HidConsoleDevice(DeviceInfo deviceInfo) : base(deviceInfo)
    {
        _cts = new CancellationTokenSource();
        // ReadLoop uses blocking synchronous HID reads (ReadTimeout); Task.Run offloads
        // it to a thread pool thread so the constructor doesn't block the UI thread.
        Task.Run(() => ReadLoop(_cts.Token), _cts.Token);
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
                _lineBuffer.Append(charBuffer, 0, charCount);

                int lineEnd;
                while ((lineEnd = IndexOfNewline(_lineBuffer)) >= 0)
                {
                    string line = _lineBuffer.ToString(0, lineEnd);
                    _lineBuffer.Remove(0, lineEnd + 1);
                    ConsoleReportReceived?.Invoke(this, line);
                }
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

    private static int IndexOfNewline(StringBuilder sb)
    {
        for (int i = 0; i < sb.Length; i++)
        {
            if (sb[i] == '\n')
                return i;
        }

        return -1;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
