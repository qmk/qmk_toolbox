using QmkToolbox.Desktop.Services;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Drives <see cref="SettingsService"/> through its entry points — construct (loads),
/// mutate <see cref="SettingsService.Current"/>, <see cref="SettingsService.Save"/>,
/// reconstruct (reloads) — against a temp settings path owned by the fixture.
/// </summary>
public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("qmk-settings-tests-").FullName;

    private string SettingsPath => Path.Combine(_dir, "QMK", "Toolbox", "settings.json");

    private SettingsService NewService() => new(SettingsPath);

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    [Fact]
    public void FirstRun_NoFile_LoadsDefaults()
    {
        SettingsService service = NewService();

        Assert.True(service.Current.FirstStart);
        Assert.False(service.Current.ShowAllDevices);
        Assert.False(service.Current.AutoFlashEnabled);
        Assert.Equal("", service.Current.FirmwareFilePath);
        Assert.Empty(service.Current.FirmwareFileHistory);
        Assert.Equal("atmega32u4", service.Current.SelectedMcu);
        Assert.Equal("Default", service.Current.ThemeVariant);
        Assert.Null(service.Current.WindowX);
        Assert.Null(service.Current.WindowWidth);
    }

    [Fact]
    public void SaveThenReload_RoundTripsEveryField()
    {
        SettingsService service = NewService();
        service.Current.FirstStart = false;
        service.Current.ShowAllDevices = true;
        service.Current.AutoFlashEnabled = true;
        service.Current.FirmwareFilePath = "/home/user/firmware.hex";
        service.Current.FirmwareFileHistory = ["/home/user/firmware.hex", "/old/one.bin"];
        service.Current.SelectedMcu = "at90usb1286";
        service.Current.ThemeVariant = "Light";
        service.Current.WindowX = 100.5;
        service.Current.WindowY = 200.0;
        service.Current.WindowWidth = 800.0;
        service.Current.WindowHeight = 600.0;

        service.Save();
        SettingsService reloaded = NewService();

        Assert.False(reloaded.Current.FirstStart);
        Assert.True(reloaded.Current.ShowAllDevices);
        Assert.True(reloaded.Current.AutoFlashEnabled);
        Assert.Equal("/home/user/firmware.hex", reloaded.Current.FirmwareFilePath);
        Assert.Equal(["/home/user/firmware.hex", "/old/one.bin"], reloaded.Current.FirmwareFileHistory);
        Assert.Equal("at90usb1286", reloaded.Current.SelectedMcu);
        Assert.Equal("Light", reloaded.Current.ThemeVariant);
        Assert.Equal(100.5, reloaded.Current.WindowX);
        Assert.Equal(200.0, reloaded.Current.WindowY);
        Assert.Equal(800.0, reloaded.Current.WindowWidth);
        Assert.Equal(600.0, reloaded.Current.WindowHeight);
    }

    [Fact]
    public void Save_MissingDirectory_CreatesIt()
    {
        // SettingsPath nests two directories that don't exist on a fresh install.
        SettingsService service = NewService();

        service.Save();

        Assert.True(File.Exists(SettingsPath));
    }

    [Fact]
    public void Load_FileFromNewerVersion_UnknownFieldsIgnored()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, /*lang=json,strict*/ """
            {
                "ShowAllDevices": true,
                "UnknownFutureField": "some value",
                "AnotherUnknown": 42
            }
            """);

        SettingsService service = NewService();

        Assert.True(service.Current.ShowAllDevices);
        Assert.False(service.Current.AutoFlashEnabled);
        Assert.Equal("atmega32u4", service.Current.SelectedMcu);
    }

    [Fact]
    public void Load_FileFromOlderVersion_MissingFieldsFallBackToDefaults()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, /*lang=json,strict*/ """{"FirmwareFilePath": "/my/firmware.hex"}""");

        SettingsService service = NewService();

        Assert.Equal("/my/firmware.hex", service.Current.FirmwareFilePath);
        Assert.False(service.Current.ShowAllDevices);
        Assert.Equal("atmega32u4", service.Current.SelectedMcu);
        Assert.Equal("Default", service.Current.ThemeVariant);
        Assert.Null(service.Current.WindowX);
    }

    [Fact]
    public void Save_Failure_ReportsThroughErrorLogger()
    {
        // The settings path nests under a *file*, so directory creation must fail.
        string blocker = Path.Combine(_dir, "blocker");
        File.WriteAllText(blocker, "");
        var service = new SettingsService(Path.Combine(blocker, "sub", "settings.json"));
        var errors = new List<string>();
        service.ErrorLogger = errors.Add;

        service.Save();

        Assert.Contains(errors, e => e.StartsWith("Failed to save settings:"));
    }

    [Fact]
    public void Load_CorruptedFile_FallsBackToDefaults()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, "{ this is not valid json !!!");

        SettingsService service = NewService();

        Assert.True(service.Current.FirstStart);
        Assert.Equal("atmega32u4", service.Current.SelectedMcu);
    }
}
