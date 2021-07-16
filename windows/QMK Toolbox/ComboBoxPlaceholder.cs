using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class ComboBoxPlaceholder : ComboBox
    {
        private const uint CB_SETCUEBANNER = 0x1703;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(HandleRef hWnd, uint msg, IntPtr wParam, String lParam);

        private string _placeholderText = String.Empty;

        /// <summary>
        /// Gets or sets the text the <see cref="ComboBox"/> will display as a cue to the user.
        /// </summary>
        [Description("The text value to be displayed as a cue to the user.")]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                if (value == null)
                {
                    value = String.Empty;
                }

                if (!_placeholderText.Equals(value, StringComparison.CurrentCulture))
                {
                    _placeholderText = value;
                    UpdatePlaceholderText();
                    OnPlaceholderTextChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="PlaceholderText"/> property value changes.
        /// </summary>
        public event EventHandler PlaceholderTextChanged;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnPlaceholderTextChanged(EventArgs e)
        {
            EventHandler handler = PlaceholderTextChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            UpdatePlaceholderText();
            base.OnHandleCreated(e);
        }

        private void UpdatePlaceholderText()
        {
            if (this.IsHandleCreated)
            {
                SendMessage(new HandleRef(this, this.Handle), CB_SETCUEBANNER, IntPtr.Zero, _placeholderText);
            }
        }
    }
}
