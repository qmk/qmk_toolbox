using System.Diagnostics;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            versionLabel.Text = $"Version {Application.ProductVersion}";
        }

        private void GithubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(githubLink.Text) { UseShellExecute = true });
        }
    }
}
