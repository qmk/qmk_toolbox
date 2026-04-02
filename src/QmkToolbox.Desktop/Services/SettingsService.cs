using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QmkToolbox.Desktop.Services;

[JsonSerializable(typeof(AppSettings))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class AppSettingsJsonContext : JsonSerializerContext
{
}

public class AppSettings
{
    public bool FirstStart { get; set; } = true;
    public bool ShowAllDevices { get; set; }
    public bool AutoFlashEnabled { get; set; }
    public string FirmwareFilePath { get; set; } = "";
    public List<string> FirmwareFileHistory { get; set; } = [];
    public string SelectedMcu { get; set; } = "atmega32u4";
    public double? WindowX { get; set; }
    public double? WindowY { get; set; }
    public double? WindowWidth { get; set; }
    public double? WindowHeight { get; set; }
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "QMK", "Toolbox", "settings.json");

    public AppSettings Current { get; private set; } = new AppSettings();

    public SettingsService()
    {
        Load();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                string json = File.ReadAllText(_settingsPath);
                Current = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings) ?? new AppSettings();
                return;
            }
        }
        catch (Exception ex) { Debug.WriteLine($"Failed to load settings: {ex.Message}"); }
        Current = new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            string json = JsonSerializer.Serialize(Current, AppSettingsJsonContext.Default.AppSettings);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex) { Debug.WriteLine($"Failed to save settings: {ex.Message}"); }
    }
}
