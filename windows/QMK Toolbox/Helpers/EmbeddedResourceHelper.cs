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
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"QMK_Toolbox.{file}"))
            using (var filestream = new FileStream(Path.Combine(Application.LocalUserAppDataPath, file), FileMode.Create))
            {
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
    }
}
