namespace QMK_Toolbox.HidConsole
{
    partial class HidConsoleWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HidConsoleWindow));
            this.consoleList = new QMK_Toolbox.ComboBoxPlaceholder();
            this.logTextBox = new QMK_Toolbox.LogTextBox();
            this.logContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logContextMenuSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logContextMenuSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // consoleList
            // 
            this.consoleList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.consoleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.consoleList.FormattingEnabled = true;
            this.consoleList.Location = new System.Drawing.Point(12, 12);
            this.consoleList.Name = "consoleList";
            this.consoleList.PlaceholderText = "No HID console devices connected";
            this.consoleList.Size = new System.Drawing.Size(600, 21);
            this.consoleList.TabIndex = 0;
            // 
            // logTextBox
            // 
            this.logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logTextBox.ContextMenuStrip = this.logContextMenu;
            this.logTextBox.DataBindings.Add(new System.Windows.Forms.Binding("ZoomFactor", global::QMK_Toolbox.Properties.Settings.Default, "outputZoom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.logTextBox.DetectUrls = false;
            this.logTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.ForeColor = System.Drawing.Color.White;
            this.logTextBox.HideSelection = false;
            this.logTextBox.Location = new System.Drawing.Point(12, 39);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(600, 390);
            this.logTextBox.TabIndex = 1;
            this.logTextBox.Text = "";
            this.logTextBox.WordWrap = false;
            this.logTextBox.ZoomFactor = global::QMK_Toolbox.Properties.Settings.Default.outputZoom;
            // 
            // logContextMenu
            // 
            this.logContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.logContextMenuSep1,
            this.selectAllToolStripMenuItem,
            this.logContextMenuSep2,
            this.clearToolStripMenuItem});
            this.logContextMenu.Name = "contextMenuStrip2";
            this.logContextMenu.ShowImageMargin = false;
            this.logContextMenu.Size = new System.Drawing.Size(140, 126);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Enabled = false;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Enabled = false;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            // 
            // logContextMenuSep1
            // 
            this.logContextMenuSep1.Name = "logContextMenuSep1";
            this.logContextMenuSep1.Size = new System.Drawing.Size(136, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Enabled = false;
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            // 
            // logContextMenuSep2
            // 
            this.logContextMenuSep2.Name = "logContextMenuSep2";
            this.logContextMenuSep2.Size = new System.Drawing.Size(136, 6);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Enabled = false;
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.clearToolStripMenuItem.Text = "Clea&r";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
            // 
            // HidConsoleWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.consoleList);
            this.Controls.Add(this.logTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 960);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "HidConsoleWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "HID Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HidConsoleWindow_FormClosing);
            this.Load += new System.EventHandler(this.HidConsoleWindow_Load);
            this.logContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private LogTextBox logTextBox;
        private ComboBoxPlaceholder consoleList;
        private System.Windows.Forms.ContextMenuStrip logContextMenu;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator logContextMenuSep1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator logContextMenuSep2;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
    }
}