namespace QMK_Toolbox.Hid
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
            components = new System.ComponentModel.Container();
            Properties.Settings settings1 = new Properties.Settings();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HidConsoleWindow));
            consoleList = new ComboBoxPlaceholder();
            logTextBox = new LogTextBox();
            logContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logContextMenuSep1 = new System.Windows.Forms.ToolStripSeparator();
            selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logContextMenuSep2 = new System.Windows.Forms.ToolStripSeparator();
            clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // consoleList
            // 
            consoleList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            consoleList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            consoleList.FormattingEnabled = true;
            consoleList.Location = new System.Drawing.Point(14, 14);
            consoleList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            consoleList.Name = "consoleList";
            consoleList.PlaceholderText = "No HID console devices connected";
            consoleList.Size = new System.Drawing.Size(699, 23);
            consoleList.TabIndex = 0;
            // 
            // logTextBox
            // 
            logTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            logTextBox.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            logTextBox.ContextMenuStrip = logContextMenu;
            settings1.autoSetting = false;
            settings1.driversInstalled = false;
            settings1.firstStart = true;
            settings1.hexFileCollection = null;
            settings1.hexFileSetting = "";
            settings1.keyboard = "";
            settings1.keymap = "";
            settings1.outputZoom = 1F;
            settings1.SettingsKey = "";
            settings1.showAllDevices = false;
            settings1.targetSetting = "atmega32u4";
            logTextBox.DataBindings.Add(new System.Windows.Forms.Binding("ZoomFactor", settings1, "outputZoom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            logTextBox.DetectUrls = false;
            logTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            logTextBox.ForeColor = System.Drawing.Color.White;
            logTextBox.HideSelection = false;
            logTextBox.Location = new System.Drawing.Point(14, 45);
            logTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            logTextBox.Name = "logTextBox";
            logTextBox.ReadOnly = true;
            logTextBox.Size = new System.Drawing.Size(700, 450);
            logTextBox.TabIndex = 1;
            logTextBox.Text = "";
            logTextBox.WordWrap = false;
            // 
            // logContextMenu
            // 
            logContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, logContextMenuSep1, selectAllToolStripMenuItem, logContextMenuSep2, clearToolStripMenuItem });
            logContextMenu.Name = "contextMenuStrip2";
            logContextMenu.ShowImageMargin = false;
            logContextMenu.Size = new System.Drawing.Size(140, 126);
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Enabled = false;
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X;
            cutToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            cutToolStripMenuItem.Text = "Cut";
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            copyToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            copyToolStripMenuItem.Text = "&Copy";
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Enabled = false;
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            pasteToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            pasteToolStripMenuItem.Text = "Paste";
            // 
            // logContextMenuSep1
            // 
            logContextMenuSep1.Name = "logContextMenuSep1";
            logContextMenuSep1.Size = new System.Drawing.Size(136, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            selectAllToolStripMenuItem.Enabled = false;
            selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            selectAllToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A;
            selectAllToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            selectAllToolStripMenuItem.Text = "Select &All";
            // 
            // logContextMenuSep2
            // 
            logContextMenuSep2.Name = "logContextMenuSep2";
            logContextMenuSep2.Size = new System.Drawing.Size(136, 6);
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Enabled = false;
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            clearToolStripMenuItem.Text = "Clea&r";
            clearToolStripMenuItem.Click += ClearToolStripMenuItem_Click;
            // 
            // HidConsoleWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(728, 509);
            Controls.Add(consoleList);
            Controls.Add(logTextBox);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(1192, 1102);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(744, 548);
            Name = "HidConsoleWindow";
            ShowInTaskbar = false;
            Text = "HID Console";
            FormClosing += HidConsoleWindow_FormClosing;
            Load += HidConsoleWindow_Load;
            logContextMenu.ResumeLayout(false);
            ResumeLayout(false);
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