using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace QMK_Toolbox.Helpers
{
    public static class EmbeddedResourceHelper
    {
        public static void ExtractResource(string file)
        {
            var destPath = Path.Combine(Application.LocalUserAppDataPath, file);

            if (!File.Exists(destPath))
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"QMK_Toolbox.Resources.{file}"))
                {
                    using (var filestream = new FileStream(destPath, FileMode.Create))
                    {
                        stream?.CopyTo(filestream);
                    }
                }
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
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"QMK_Toolbox.Resources.{file}"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
