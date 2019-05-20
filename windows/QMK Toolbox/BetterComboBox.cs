using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class BetterComboBox : ComboBox
    {
        public override string Text
        {
            get
            {
                if (Items.Count == 0 && SelectedIndex > -1)
                    SelectedIndex = -1;
                return base.Text;
            }
            set => base.Text = value;
        }
    }
}
