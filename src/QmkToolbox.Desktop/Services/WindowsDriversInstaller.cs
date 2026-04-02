using System.Diagnostics;
using System.Runtime.InteropServices;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Windows-only driver installer using qmk_driver_installer.exe (bundled resource).
/// All methods are no-ops on non-Windows platforms.
/// </summary>
public static class WindowsDriversInstaller
{
    private const string DriversListFilename = "drivers.txt";
    private const string InstallerFilename = "qmk_driver_installer.exe";

    public static void Install(IFlashToolProvider toolProvider, Action<string> logError)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        string toolboxData = toolProvider.GetResourceFolder();
        string installerPath = Path.Combine(toolboxData, InstallerFilename);
        string driversPath = Path.Combine(toolboxData, DriversListFilename);

        if (!File.Exists(installerPath) || !File.Exists(driversPath))
        {
            logError("Driver installer not found. Please clear and re-extract resources via Tools → Clear Resources.");
            return;
        }

        try
        {
            var psi = new ProcessStartInfo(installerPath) { Verb = "runas", UseShellExecute = true };
            psi.ArgumentList.Add("--all");
            psi.ArgumentList.Add("--force");
            psi.ArgumentList.Add(driversPath);
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            logError($"Driver installation failed: {ex.Message}");
        }
    }
}
