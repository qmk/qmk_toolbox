using System.Text.Json;
using QmkToolbox.Desktop.Services;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Serialization round-trip tests for <see cref="AppSettings"/> using the source-generated
/// <see cref="AppSettingsJsonContext"/>. These tests verify that every field survives a
/// serialize → deserialize cycle and that resilience defaults (missing fields, extra fields)
/// work correctly.
/// </summary>
public class SettingsServiceTests
{
    private static AppSettings? Roundtrip(AppSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings);
        return JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);
    }

    // ── Default values ─────────────────────────────────────────────────────────

    [Fact]
    public void DefaultSettings_RoundTrip_PreservesDefaults()
    {
        AppSettings? result = Roundtrip(new AppSettings());

        Assert.NotNull(result);
        Assert.True(result.FirstStart);
        Assert.False(result.ShowAllDevices);
        Assert.False(result.AutoFlashEnabled);
        Assert.Equal("", result.FirmwareFilePath);
        Assert.Empty(result.FirmwareFileHistory);
        Assert.Equal("atmega32u4", result.SelectedMcu);
        Assert.Null(result.WindowX);
        Assert.Null(result.WindowY);
        Assert.Null(result.WindowWidth);
        Assert.Null(result.WindowHeight);
    }

    // ── Non-default scalar fields ──────────────────────────────────────────────

    [Fact]
    public void ModifiedScalars_RoundTrip_PreservesValues()
    {
        var original = new AppSettings
        {
            FirstStart = false,
            ShowAllDevices = true,
            AutoFlashEnabled = true,
            FirmwareFilePath = "/home/user/firmware.hex",
            SelectedMcu = "at90usb1286",
        };

        AppSettings? result = Roundtrip(original);

        Assert.NotNull(result);
        Assert.False(result.FirstStart);
        Assert.True(result.ShowAllDevices);
        Assert.True(result.AutoFlashEnabled);
        Assert.Equal("/home/user/firmware.hex", result.FirmwareFilePath);
        Assert.Equal("at90usb1286", result.SelectedMcu);
    }

    // ── Nullable window geometry ───────────────────────────────────────────────

    [Fact]
    public void WindowGeometry_Set_RoundTrip_PreservesValues()
    {
        var original = new AppSettings
        {
            WindowX = 100.5,
            WindowY = 200.0,
            WindowWidth = 800.0,
            WindowHeight = 600.0,
        };

        AppSettings? result = Roundtrip(original);

        Assert.NotNull(result);
        Assert.Equal(100.5, result.WindowX);
        Assert.Equal(200.0, result.WindowY);
        Assert.Equal(800.0, result.WindowWidth);
        Assert.Equal(600.0, result.WindowHeight);
    }

    [Fact]
    public void WindowGeometry_Null_RoundTrip_RemainsNull()
    {
        AppSettings? result = Roundtrip(new AppSettings { WindowX = null, WindowY = null, WindowWidth = null, WindowHeight = null });

        Assert.NotNull(result);
        Assert.Null(result.WindowX);
        Assert.Null(result.WindowY);
        Assert.Null(result.WindowWidth);
        Assert.Null(result.WindowHeight);
    }

    // ── FirmwareFileHistory list ───────────────────────────────────────────────

    [Fact]
    public void FirmwareFileHistory_MultipleEntries_RoundTrip_PreservesOrderAndContent()
    {
        var original = new AppSettings
        {
            FirmwareFileHistory =
            [
                "/path/to/first.hex",
                "/path/to/second.bin",
                "/path/to/third.uf2",
            ],
        };

        AppSettings? result = Roundtrip(original);

        Assert.NotNull(result);
        Assert.Equal(["/path/to/first.hex", "/path/to/second.bin", "/path/to/third.uf2"],
            result.FirmwareFileHistory);
    }

    // ── Resilience: unknown JSON fields ignored ─────────────────────────────────

    [Fact]
    public void Deserialize_UnknownFields_AreIgnored()
    {
        const string json = """
            {
                "ShowAllDevices": true,
                "UnknownFutureField": "some value",
                "AnotherUnknown": 42
            }
            """;

        AppSettings? result = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);

        Assert.NotNull(result);
        Assert.True(result.ShowAllDevices);
        // Other fields should have their defaults
        Assert.False(result.AutoFlashEnabled);
        Assert.Equal("atmega32u4", result.SelectedMcu);
    }

    // ── Resilience: missing JSON fields fall back to defaults ──────────────────

    [Fact]
    public void Deserialize_MissingFields_FallBackToDefaults()
    {
        // JSON with only one field — all others must default correctly.
        const string json = """{"FirmwareFilePath": "/my/firmware.hex"}""";

        AppSettings? result = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);

        Assert.NotNull(result);
        Assert.Equal("/my/firmware.hex", result.FirmwareFilePath);
        Assert.False(result.ShowAllDevices);
        Assert.Equal("atmega32u4", result.SelectedMcu);
        Assert.Null(result.WindowX);
    }

    // ── Serialized JSON is human-readable (WriteIndented) ─────────────────────

    [Fact]
    public void Serialize_ProducesIndentedJson()
    {
        string json = JsonSerializer.Serialize(new AppSettings(), AppSettingsJsonContext.Default.AppSettings);

        // WriteIndented = true means there will be newlines in the output.
        Assert.Contains('\n', json);
    }

    // ── Empty JSON object produces default AppSettings ─────────────────────────

    [Fact]
    public void Deserialize_EmptyObject_ReturnsDefaults()
    {
        AppSettings? result = JsonSerializer.Deserialize("{}", AppSettingsJsonContext.Default.AppSettings);

        Assert.NotNull(result);
        Assert.False(result.ShowAllDevices);
        Assert.Equal("atmega32u4", result.SelectedMcu);
    }
}
