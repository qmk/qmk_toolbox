using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class BetterComboBox : ComboBoxPlaceholder
    {
        public override string Text
        {
            get
            {
                if (Items.Count == 0 && SelectedIndex > -1)
                {
                    SelectedIndex = -1;
                }
                return base.Text;
            }
            set => base.Text = value;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                if (SelectedIndex < 0)
                {
                    return;
                }

                int hoverIndex = SelectedIndex;

                Items.RemoveAt(hoverIndex);

                if (hoverIndex == Items.Count)
                {
                    SelectedIndex = hoverIndex - 1;
                }
                else if (Items.Count > 0)
                {
                    SelectedIndex = hoverIndex;
                }
                else
                {
                    SelectedIndex = -1;
                }
            }
        }
    }
}
