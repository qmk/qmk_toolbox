using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class ComboBoxPlaceholder : ComboBox
    {
        private const uint CB_SETCUEBANNER = 0x1703;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern IntPtr SendMessage(HandleRef hWnd, uint msg, IntPtr wParam, string lParam);

        private string _placeholderText = string.Empty;

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
                value ??= string.Empty;

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
            PlaceholderTextChanged?.Invoke(this, e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            UpdatePlaceholderText();
            base.OnHandleCreated(e);
        }

        private void UpdatePlaceholderText()
        {
            if (IsHandleCreated)
            {
                SendMessage(new HandleRef(this, Handle), CB_SETCUEBANNER, IntPtr.Zero, _placeholderText);
            }
        }
    }
}
