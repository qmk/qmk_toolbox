namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Provides UI operations that require access to the application window.
/// </summary>
public interface IWindowService
{
    /// <summary>Opens a file picker and returns the selected firmware file path, or <see langword="null"/> if cancelled.</summary>
    Task<string?> PickFirmwareFileAsync();

    /// <summary>Places <paramref name="text"/> on the system clipboard.</summary>
    Task SetClipboardTextAsync(string text);

    /// <summary>Opens the Key Tester window.</summary>
    void ShowKeyTester();

    /// <summary>Opens the HID Console window.</summary>
    void ShowHidConsole();

    /// <summary>Opens the About window.</summary>
    void ShowAbout();

    /// <summary>Opens the Debug Log window.</summary>
    void ShowDebugLog();

    /// <summary>Appends a diagnostic trace line to the Debug Log window. No-op when the window is not open.</summary>
    void TraceDebug(string message);
}
