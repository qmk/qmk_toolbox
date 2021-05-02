namespace QMK_Toolbox {
    partial class MainWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.flashButton = new System.Windows.Forms.Button();
            this.autoflashCheckbox = new System.Windows.Forms.CheckBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.openFileButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.mcuLabel = new System.Windows.Forms.Label();
            this.qmkfmGroupBox = new System.Windows.Forms.GroupBox();
            this.keymapLabel = new System.Windows.Forms.Label();
            this.keymapBox = new System.Windows.Forms.ComboBox();
            this.keyboardBox = new System.Windows.Forms.ComboBox();
            this.loadKeymap = new System.Windows.Forms.Button();
            this.fileGroupBox = new System.Windows.Forms.GroupBox();
            this.filepathBox = new QMK_Toolbox.BetterComboBox();
            this.mcuBox = new System.Windows.Forms.ComboBox();
            this.clearEepromButton = new System.Windows.Forms.Button();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.logContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hidList = new System.Windows.Forms.ComboBox();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenuItem = new System.Windows.Forms.MenuItem();
            this.openMenuItem = new System.Windows.Forms.MenuItem();
            this.fileMenuSep = new System.Windows.Forms.MenuItem();
            this.exitMenuItem = new System.Windows.Forms.MenuItem();
            this.toolsMenuItem = new System.Windows.Forms.MenuItem();
            this.installDriversMenuItem = new System.Windows.Forms.MenuItem();
            this.toolsMenuSep = new System.Windows.Forms.MenuItem();
            this.optionsMenuItem = new System.Windows.Forms.MenuItem();
            this.HelpMenuItem = new System.Windows.Forms.MenuItem();
            this.checkForUpdatesMenuItem = new System.Windows.Forms.MenuItem();
            this.helpMenuSep = new System.Windows.Forms.MenuItem();
            this.aboutMenuItem = new System.Windows.Forms.MenuItem();
            this.qmkfmGroupBox.SuspendLayout();
            this.fileGroupBox.SuspendLayout();
            this.logContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // flashButton
            // 
            this.flashButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flashButton.Enabled = false;
            this.flashButton.Location = new System.Drawing.Point(653, 66);
            this.flashButton.Name = "flashButton";
            this.flashButton.Size = new System.Drawing.Size(62, 23);
            this.flashButton.TabIndex = 6;
            this.flashButton.Text = "Flash";
            this.flashButton.Click += new System.EventHandler(this.flashButton_Click);
            // 
            // autoflashCheckbox
            // 
            this.autoflashCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.autoflashCheckbox.AutoSize = true;
            this.autoflashCheckbox.BackColor = System.Drawing.Color.Transparent;
            this.autoflashCheckbox.Location = new System.Drawing.Point(653, 95);
            this.autoflashCheckbox.Name = "autoflashCheckbox";
            this.autoflashCheckbox.Size = new System.Drawing.Size(76, 17);
            this.autoflashCheckbox.TabIndex = 5;
            this.autoflashCheckbox.Text = "Auto-Flash";
            this.autoflashCheckbox.UseVisualStyleBackColor = false;
            this.autoflashCheckbox.CheckedChanged += new System.EventHandler(this.autoflashCheckbox_CheckedChanged);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Intel Hex and Binary (*.hex;*.bin)|*.hex;*.bin|Intel Hex (*.hex)|*.hex|Binary (*." +
    "bin)|*.bin";
            // 
            // openFileButton
            // 
            this.openFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openFileButton.Location = new System.Drawing.Point(571, 18);
            this.openFileButton.Name = "openFileButton";
            this.openFileButton.Size = new System.Drawing.Size(64, 23);
            this.openFileButton.TabIndex = 3;
            this.openFileButton.Text = "Open";
            this.openFileButton.UseVisualStyleBackColor = true;
            this.openFileButton.Click += new System.EventHandler(this.openFileButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Enabled = false;
            this.resetButton.Location = new System.Drawing.Point(721, 66);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(67, 23);
            this.resetButton.TabIndex = 7;
            this.resetButton.Text = "Exit DFU";
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // mcuLabel
            // 
            this.mcuLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mcuLabel.AutoSize = true;
            this.mcuLabel.Location = new System.Drawing.Point(638, 0);
            this.mcuLabel.Name = "mcuLabel";
            this.mcuLabel.Size = new System.Drawing.Size(84, 13);
            this.mcuLabel.TabIndex = 22;
            this.mcuLabel.Text = "MCU (AVR only)";
            // 
            // qmkfmGroupBox
            // 
            this.qmkfmGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.qmkfmGroupBox.Controls.Add(this.keymapLabel);
            this.qmkfmGroupBox.Controls.Add(this.keymapBox);
            this.qmkfmGroupBox.Controls.Add(this.keyboardBox);
            this.qmkfmGroupBox.Controls.Add(this.loadKeymap);
            this.qmkfmGroupBox.Location = new System.Drawing.Point(12, 66);
            this.qmkfmGroupBox.Name = "qmkfmGroupBox";
            this.qmkfmGroupBox.Size = new System.Drawing.Size(635, 48);
            this.qmkfmGroupBox.TabIndex = 23;
            this.qmkfmGroupBox.TabStop = false;
            this.qmkfmGroupBox.Text = "Keyboard from qmk.fm";
            // 
            // keymapLabel
            // 
            this.keymapLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.keymapLabel.AutoSize = true;
            this.keymapLabel.Location = new System.Drawing.Point(447, 0);
            this.keymapLabel.Name = "keymapLabel";
            this.keymapLabel.Size = new System.Drawing.Size(45, 13);
            this.keymapLabel.TabIndex = 24;
            this.keymapLabel.Text = "Keymap";
            // 
            // keymapBox
            // 
            this.keymapBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.keymapBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "keymap", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keymapBox.Enabled = false;
            this.keymapBox.FormattingEnabled = true;
            this.keymapBox.Items.AddRange(new object[] {
            "later version!"});
            this.keymapBox.Location = new System.Drawing.Point(450, 19);
            this.keymapBox.Name = "keymapBox";
            this.keymapBox.Size = new System.Drawing.Size(109, 21);
            this.keymapBox.TabIndex = 4;
            this.keymapBox.Text = global::QMK_Toolbox.Properties.Settings.Default.keymap;
            // 
            // keyboardBox
            // 
            this.keyboardBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.keyboardBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.keyboardBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.keyboardBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "keyboard", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keyboardBox.Enabled = false;
            this.keyboardBox.FormattingEnabled = true;
            this.keyboardBox.Items.AddRange(new object[] {
            "this feature coming in"});
            this.keyboardBox.Location = new System.Drawing.Point(6, 19);
            this.keyboardBox.Name = "keyboardBox";
            this.keyboardBox.Size = new System.Drawing.Size(438, 21);
            this.keyboardBox.TabIndex = 4;
            this.keyboardBox.Text = global::QMK_Toolbox.Properties.Settings.Default.keyboard;
            this.keyboardBox.TextChanged += new System.EventHandler(this.keyboardBox_TextChanged);
            // 
            // loadKeymap
            // 
            this.loadKeymap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.loadKeymap.Enabled = false;
            this.loadKeymap.Location = new System.Drawing.Point(565, 18);
            this.loadKeymap.Name = "loadKeymap";
            this.loadKeymap.Size = new System.Drawing.Size(64, 23);
            this.loadKeymap.TabIndex = 3;
            this.loadKeymap.Text = "Load";
            this.loadKeymap.UseVisualStyleBackColor = true;
            this.loadKeymap.Click += new System.EventHandler(this.loadKeymap_Click);
            // 
            // fileGroupBox
            // 
            this.fileGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileGroupBox.Controls.Add(this.openFileButton);
            this.fileGroupBox.Controls.Add(this.filepathBox);
            this.fileGroupBox.Controls.Add(this.mcuLabel);
            this.fileGroupBox.Controls.Add(this.mcuBox);
            this.fileGroupBox.Location = new System.Drawing.Point(12, 12);
            this.fileGroupBox.Name = "fileGroupBox";
            this.fileGroupBox.Size = new System.Drawing.Size(776, 48);
            this.fileGroupBox.TabIndex = 25;
            this.fileGroupBox.TabStop = false;
            this.fileGroupBox.Text = "Local file";
            // 
            // filepathBox
            // 
            this.filepathBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filepathBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "hexFileSetting", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.filepathBox.FormattingEnabled = true;
            this.filepathBox.Location = new System.Drawing.Point(6, 19);
            this.filepathBox.Name = "filepathBox";
            this.filepathBox.Size = new System.Drawing.Size(558, 21);
            this.filepathBox.TabIndex = 2;
            this.filepathBox.Text = global::QMK_Toolbox.Properties.Settings.Default.hexFileSetting;
            this.filepathBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.filepathBox_KeyDown);
            // 
            // mcuBox
            // 
            this.mcuBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mcuBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "targetSetting", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.mcuBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mcuBox.FormattingEnabled = true;
            this.mcuBox.Location = new System.Drawing.Point(641, 19);
            this.mcuBox.Name = "mcuBox";
            this.mcuBox.Size = new System.Drawing.Size(129, 21);
            this.mcuBox.TabIndex = 4;
            this.mcuBox.Text = global::QMK_Toolbox.Properties.Settings.Default.targetSetting;
            // 
            // clearEepromButton
            // 
            this.clearEepromButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.clearEepromButton.Enabled = false;
            this.clearEepromButton.Location = new System.Drawing.Point(12, 606);
            this.clearEepromButton.Name = "clearEepromButton";
            this.clearEepromButton.Size = new System.Drawing.Size(110, 23);
            this.clearEepromButton.TabIndex = 27;
            this.clearEepromButton.Text = "Clear EEPROM";
            this.clearEepromButton.UseVisualStyleBackColor = true;
            this.clearEepromButton.Click += new System.EventHandler(this.clearEepromButton_Click);
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
            this.logTextBox.Location = new System.Drawing.Point(12, 120);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(776, 480);
            this.logTextBox.TabIndex = 1;
            this.logTextBox.Text = "";
            this.logTextBox.WordWrap = false;
            this.logTextBox.ZoomFactor = global::QMK_Toolbox.Properties.Settings.Default.outputZoom;
            this.logTextBox.TextChanged += new System.EventHandler(this.logTextBox_TextChanged);
            // 
            // logContextMenu
            // 
            this.logContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripMenuItem1,
            this.selectAllToolStripMenuItem,
            this.toolStripMenuItem2,
            this.clearToolStripMenuItem});
            this.logContextMenu.Name = "contextMenuStrip2";
            this.logContextMenu.ShowImageMargin = false;
            this.logContextMenu.Size = new System.Drawing.Size(140, 126);
            this.logContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
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
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Enabled = false;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(136, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Enabled = false;
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(136, 6);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Enabled = false;
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.clearToolStripMenuItem.Text = "Clea&r";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // hidList
            // 
            this.hidList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hidList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.hidList.FormattingEnabled = true;
            this.hidList.Location = new System.Drawing.Point(128, 607);
            this.hidList.Name = "hidList";
            this.hidList.Size = new System.Drawing.Size(660, 21);
            this.hidList.TabIndex = 29;
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenuItem,
            this.toolsMenuItem,
            this.HelpMenuItem});
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.Index = 0;
            this.fileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.openMenuItem,
            this.fileMenuSep,
            this.exitMenuItem});
            this.fileMenuItem.Text = "&File";
            // 
            // openMenuItem
            // 
            this.openMenuItem.Index = 0;
            this.openMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.openMenuItem.Text = "&Open...";
            this.openMenuItem.Click += new System.EventHandler(this.openFileButton_Click);
            // 
            // fileMenuSep
            // 
            this.fileMenuSep.Index = 1;
            this.fileMenuSep.Text = "-";
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Index = 2;
            this.exitMenuItem.Shortcut = System.Windows.Forms.Shortcut.AltF4;
            this.exitMenuItem.Text = "E&xit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // toolsMenuItem
            // 
            this.toolsMenuItem.Index = 1;
            this.toolsMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.installDriversMenuItem,
            this.toolsMenuSep,
            this.optionsMenuItem});
            this.toolsMenuItem.Text = "&Tools";
            // 
            // installDriversMenuItem
            // 
            this.installDriversMenuItem.Index = 0;
            this.installDriversMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.installDriversMenuItem.Text = "I&nstall Drivers...";
            this.installDriversMenuItem.Click += new System.EventHandler(this.installDriversMenuItem_Click);
            // 
            // toolsMenuSep
            // 
            this.toolsMenuSep.Index = 1;
            this.toolsMenuSep.Text = "-";
            // 
            // optionsMenuItem
            // 
            this.optionsMenuItem.Enabled = false;
            this.optionsMenuItem.Index = 2;
            this.optionsMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlP;
            this.optionsMenuItem.Text = "O&ptions...";
            // 
            // HelpMenuItem
            // 
            this.HelpMenuItem.Index = 2;
            this.HelpMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.checkForUpdatesMenuItem,
            this.helpMenuSep,
            this.aboutMenuItem});
            this.HelpMenuItem.Text = "&Help";
            // 
            // checkForUpdatesMenuItem
            // 
            this.checkForUpdatesMenuItem.Enabled = false;
            this.checkForUpdatesMenuItem.Index = 0;
            this.checkForUpdatesMenuItem.Text = "Check for Updates...";
            // 
            // helpMenuSep
            // 
            this.helpMenuSep.Index = 1;
            this.helpMenuSep.Text = "-";
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Index = 2;
            this.aboutMenuItem.Text = "&About";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 640);
            this.Controls.Add(this.hidList);
            this.Controls.Add(this.clearEepromButton);
            this.Controls.Add(this.fileGroupBox);
            this.Controls.Add(this.qmkfmGroupBox);
            this.Controls.Add(this.flashButton);
            this.Controls.Add(this.autoflashCheckbox);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.resetButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu;
            this.MinimumSize = new System.Drawing.Size(816, 699);
            this.Name = "MainWindow";
            this.Text = "QMK Toolbox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.Shown += new System.EventHandler(this.MainWindow_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragEnter);
            this.qmkfmGroupBox.ResumeLayout(false);
            this.qmkfmGroupBox.PerformLayout();
            this.fileGroupBox.ResumeLayout(false);
            this.fileGroupBox.PerformLayout();
            this.logContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox mcuBox;
        private System.Windows.Forms.Button flashButton;
        private System.Windows.Forms.CheckBox autoflashCheckbox;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.RichTextBox logTextBox;
        private System.Windows.Forms.Button openFileButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Label mcuLabel;
        private System.Windows.Forms.GroupBox qmkfmGroupBox;
        private System.Windows.Forms.ComboBox keymapBox;
        private System.Windows.Forms.ComboBox keyboardBox;
        private System.Windows.Forms.Button loadKeymap;
        private System.Windows.Forms.Label keymapLabel;
        private System.Windows.Forms.GroupBox fileGroupBox;
        private System.Windows.Forms.Button clearEepromButton;
        private System.Windows.Forms.ComboBox hidList;
        private System.Windows.Forms.ContextMenuStrip logContextMenu;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private BetterComboBox filepathBox;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem fileMenuItem;
        private System.Windows.Forms.MenuItem openMenuItem;
        private System.Windows.Forms.MenuItem fileMenuSep;
        private System.Windows.Forms.MenuItem exitMenuItem;
        private System.Windows.Forms.MenuItem toolsMenuItem;
        private System.Windows.Forms.MenuItem installDriversMenuItem;
        private System.Windows.Forms.MenuItem HelpMenuItem;
        private System.Windows.Forms.MenuItem aboutMenuItem;
        private System.Windows.Forms.MenuItem toolsMenuSep;
        private System.Windows.Forms.MenuItem optionsMenuItem;
        private System.Windows.Forms.MenuItem checkForUpdatesMenuItem;
        private System.Windows.Forms.MenuItem helpMenuSep;
    }
}

