namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Provides access to persistent application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>The current application settings.</summary>
    AppSettings Current { get; }

    /// <summary>Persists the current settings to disk.</summary>
    void Save();
}
