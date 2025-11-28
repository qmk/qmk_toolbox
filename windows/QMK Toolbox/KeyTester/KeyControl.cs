using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace QMK_Toolbox.KeyTester
{
    public partial class KeyControl : UserControl
    {
        private bool pressed = false;

        private bool tested = false;

        [Description("Whether the key is currently pressed."), Category("Appearance")]
        public bool Pressed {
            get => pressed;
            set {
                pressed = value;
                SetKeyColor();
            }
        }

        public bool ShouldSerializePressed() => false;

        [Description("Whether the key has been tested."), Category("Appearance")]
        public bool Tested {
            get => tested;
            set
            {
                tested = value;
                SetKeyColor();
            }
        }

        public bool ShouldSerializeTested() => false;

        [Description("The legend to be displayed on the key."), Category("Appearance"), Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Legend {
            get => lblLegend.Text;
            set => lblLegend.Text = value;
        }

        public bool ShouldSerializeLegend() => false;

        private void SetKeyColor()
        {
            lblLegend.BackColor = Pressed ? Color.LightYellow : (Tested ? Color.LightGreen : SystemColors.ControlLight);
        }

        public KeyControl()
        {
            InitializeComponent();
        }
    }
}
