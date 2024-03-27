using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace QMK_Toolbox.Helpers
{
    internal static class EmbeddedResourceHelper
    {
        public static readonly string[] Resources =
        {
            "avrdude.conf",
            "reset.eep",
            "reset_left.eep",
            "reset_right.eep",
            "avrdude",
            "bootloadHID",
            "dfu-programmer",
            "dfu-util",
            "hid_bootloader_cli",
            "mdloader",
            "teensy_loader_cli",
            "wb32-dfu-updater_cli",
            "libftdi1.so",
            "libusb.so",
            "libhidapi-libusb.so",
            "libusb-1.0.dll",
            "wb32-dfu-updater_cli",
        };

        internal static void ExtractResource(string file)
        {
            var destPath = Path.Combine("/tmp", file);

            if (!File.Exists(destPath))
            {
                using var stream = 
                    Assembly.GetExecutingAssembly().GetManifestResourceStream($"QMK_Toolbox.Resources.{file}");
                using var filestream = new FileStream(destPath, FileMode.Create);
                stream?.CopyTo(filestream);
                LinuxPermissions.MakeExecutable(destPath);
            }
        }

        internal static void ExtractResources(params string[] files) => ExtractResources(files as IEnumerable<string>);

        private static void ExtractResources(IEnumerable<string> files)
        {
            foreach (var s in files)
            {
                ExtractResource(s);
            }
        }

        internal static string GetResourceContent(string file)
        {
            using var stream = 
                Assembly.GetExecutingAssembly().GetManifestResourceStream($"QMK_Toolbox.Resources.{file}");
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
