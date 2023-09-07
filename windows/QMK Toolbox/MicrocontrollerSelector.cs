using QMK_Toolbox.Helpers;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class MicrocontrollerSelector : ComboBox
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (!DesignMode)
            {
                List<KeyValuePair<string, string>> microcontrollerDataSource = new();
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
}
