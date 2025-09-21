using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace QMK_Toolbox.Helpers
{
    public static class EmbeddedResourceHelper
    {
        public static readonly string[] Resources =
        {
            "avrdude.conf",
            "reset.eep",
            "reset_left.eep",
            "reset_right.eep",
            "avrdude.exe",
            "bootloadHID.exe",
            "dfu-programmer.exe",
            "dfu-util.exe",
            "hid_bootloader_cli.exe",
            "mdloader.exe",
            "teensy_loader_cli.exe",
            "wb32-dfu-updater_cli.exe",
            "flashutils_release_windowsX64"
        };

        public static string GetResourceFolder()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "QMK", "Toolbox");
        }

        public static void InitResourceFolder()
        {
            string toolboxData = GetResourceFolder();
            if (Directory.Exists(toolboxData))
            {
                Directory.Delete(toolboxData, true);
            }
            Directory.CreateDirectory(toolboxData);
            ExtractResources(Resources);
        }

        public static void ExtractResource(string file)
        {
            string destPath = Path.Combine(GetResourceFolder(), file);

            if (!File.Exists(destPath))
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"QMK_Toolbox.Resources.{file}");
                using var filestream = new FileStream(destPath, FileMode.Create);
                stream?.CopyTo(filestream);
            }
        }

        public static void ExtractResources(params string[] files) => ExtractResources(files as IEnumerable<string>);
        public static void ExtractResources(IEnumerable<string> files)
        {
            foreach (var s in files)
            {
                ExtractResource(s);
            }
        }

        public static string GetResourceContent(string file)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"QMK_Toolbox.Resources.{file}");
            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }
    }
}
