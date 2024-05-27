using QMK_Toolbox.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace QMK_Toolbox.Helpers
{
    public class DriverInstaller
    {
        private const string DriversListFilename = "drivers.txt";
        private const string InstallerFilename = "qmk_driver_installer.exe";

        public static bool DisplayPrompt()
        {
            var driverPromptResult = MessageBox.Show("Would you like to install drivers for your devices?", "Driver installation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (driverPromptResult == DialogResult.No || !InstallDrivers())
            {
                return false;
            }
            
            Settings.Default.driversInstalled = true;
            Settings.Default.Save();
            return true;
        }

        private static bool InstallDrivers()
        {
            string toolboxData = EmbeddedResourceHelper.GetResourceFolder();
            string driversPath = Path.Combine(toolboxData, DriversListFilename);
            string installerPath = Path.Combine(toolboxData, InstallerFilename);

            if (!File.Exists(driversPath))
            {
                EmbeddedResourceHelper.ExtractResources(DriversListFilename);
            }

            if (!File.Exists(installerPath))
            {
                EmbeddedResourceHelper.ExtractResources(InstallerFilename);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(installerPath, $"--all --force \"{driversPath}\"")
                {
                    Verb = "runas"
                }
            };

            try
            {
                process.Start();
                return true;
            }
            catch (Win32Exception)
            {
                var tryAgainResult = MessageBox.Show("This action requires administrator rights. Do you want to try again?", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (tryAgainResult == DialogResult.Retry)
                {
                    return InstallDrivers();
                }
            }

            return false;
        }
    }
}
