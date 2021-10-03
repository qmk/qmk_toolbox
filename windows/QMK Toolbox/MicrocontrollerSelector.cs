using QMK_Toolbox.Helpers;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class MicrocontrollerSelector : ComboBox
    {
        public MicrocontrollerSelector()
        {
            List<KeyValuePair<string, string>> microcontrollerDataSource = new List<KeyValuePair<string, string>>();
            string[] microcontrollers = EmbeddedResourceHelper.GetResourceContent("mcu-list.txt").Split('\n');

            foreach (string microcontroller in microcontrollers)
            {
                if (microcontroller.Length > 0)
                {
                    string[] parts = microcontroller.Split(':');
                    microcontrollerDataSource.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
                }
            }

            DataSource = microcontrollerDataSource;
        }
    }
}
