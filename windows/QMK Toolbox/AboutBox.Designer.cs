namespace QMK_Toolbox {
    partial class AboutBox {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.toolboxLabel = new System.Windows.Forms.Label();
            this.githubLink = new System.Windows.Forms.LinkLabel();
            this.qmkLogo = new System.Windows.Forms.PictureBox();
            this.versionLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.qmkLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // toolboxLabel
            // 
            this.toolboxLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolboxLabel.Location = new System.Drawing.Point(12, 108);
            this.toolboxLabel.Name = "toolboxLabel";
            this.toolboxLabel.Size = new System.Drawing.Size(232, 13);
            this.toolboxLabel.TabIndex = 0;
            this.toolboxLabel.Text = "QMK Toolbox";
            this.toolboxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // githubLink
            // 
            this.githubLink.ActiveLinkColor = System.Drawing.SystemColors.Highlight;
            this.githubLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.githubLink.LinkColor = System.Drawing.SystemColors.Highlight;
            this.githubLink.Location = new System.Drawing.Point(12, 138);
            this.githubLink.Name = "githubLink";
            this.githubLink.Size = new System.Drawing.Size(232, 13);
            this.githubLink.TabIndex = 1;
            this.githubLink.TabStop = true;
            this.githubLink.Text = "https://github.com/qmk/qmk_toolbox";
            this.githubLink.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.githubLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GithubLink_LinkClicked);
            // 
            // qmkLogo
            // 
            this.qmkLogo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.qmkLogo.Image = ((System.Drawing.Image)(resources.GetObject("qmkLogo.Image")));
            this.qmkLogo.Location = new System.Drawing.Point(92, 12);
            this.qmkLogo.Name = "qmkLogo";
            this.qmkLogo.Size = new System.Drawing.Size(72, 72);
            this.qmkLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.qmkLogo.TabIndex = 2;
            this.qmkLogo.TabStop = false;
            // 
            // versionLabel
            // 
            this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.versionLabel.Location = new System.Drawing.Point(12, 123);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(232, 13);
            this.versionLabel.TabIndex = 3;
            this.versionLabel.Text = "Version 0.0.0";
            this.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(256, 160);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.qmkLogo);
            this.Controls.Add(this.githubLink);
            this.Controls.Add(this.toolboxLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About QMK Toolbox";
            ((System.ComponentModel.ISupportInitialize)(this.qmkLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label toolboxLabel;
        private System.Windows.Forms.LinkLabel githubLink;
        private System.Windows.Forms.PictureBox qmkLogo;
        private System.Windows.Forms.Label versionLabel;
    }
}
