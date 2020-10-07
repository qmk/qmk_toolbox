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
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mcuLabel = new System.Windows.Forms.Label();
            this.qmkGroupBox = new System.Windows.Forms.GroupBox();
            this.keymapLabel = new System.Windows.Forms.Label();
            this.keymapBox = new System.Windows.Forms.ComboBox();
            this.keyboardBox = new System.Windows.Forms.ComboBox();
            this.loadKeymap = new System.Windows.Forms.Button();
            this.fileGroupBox = new System.Windows.Forms.GroupBox();
            this.filepathBox = new QMK_Toolbox.BetterComboBox();
            this.mcuBox = new System.Windows.Forms.ComboBox();
            this.clearEepromButton = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.installDriversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hidList = new System.Windows.Forms.ComboBox();
            this.flashWhenReadyCheckbox = new System.Windows.Forms.CheckBox();
            this.statusStrip.SuspendLayout();
            this.qmkGroupBox.SuspendLayout();
            this.fileGroupBox.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // flashButton
            // 
            this.flashButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flashButton.Location = new System.Drawing.Point(657, 59);
            this.flashButton.Name = "flashButton";
            this.flashButton.Size = new System.Drawing.Size(57, 23);
            this.flashButton.TabIndex = 6;
            this.flashButton.Tag = "Erase, flash, and reset the MCU with the provided .hex file";
            this.flashButton.Text = "Flash";
            this.flashButton.Click += new System.EventHandler(this.flashButton_Click);
            this.flashButton.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.flashButton.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // autoflashCheckbox
            // 
            this.autoflashCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.autoflashCheckbox.AutoSize = true;
            this.autoflashCheckbox.BackColor = System.Drawing.Color.Transparent;
            this.autoflashCheckbox.Location = new System.Drawing.Point(710, 86);
            this.autoflashCheckbox.Name = "autoflashCheckbox";
            this.autoflashCheckbox.Size = new System.Drawing.Size(76, 17);
            this.autoflashCheckbox.TabIndex = 5;
            this.autoflashCheckbox.Tag = "Automatically flash when a device is detected in DFU mode";
            this.autoflashCheckbox.Text = "Auto-Flash";
            this.autoflashCheckbox.UseVisualStyleBackColor = false;
            this.autoflashCheckbox.CheckedChanged += new System.EventHandler(this.autoflashCheckbox_CheckedChanged);
            this.autoflashCheckbox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.autoflashCheckbox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Intel Hex and Binary (*.hex;*.bin)|*.hex;*.bin|Intel Hex (*.hex)|*.hex|Binary (*." +
    "bin)|*.bin";
            // 
            // openFileButton
            // 
            this.openFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openFileButton.Location = new System.Drawing.Point(476, 19);
            this.openFileButton.Name = "openFileButton";
            this.openFileButton.Size = new System.Drawing.Size(64, 23);
            this.openFileButton.TabIndex = 3;
            this.openFileButton.Tag = "Select a file from explorer";
            this.openFileButton.Text = "Open";
            this.openFileButton.UseVisualStyleBackColor = true;
            this.openFileButton.Click += new System.EventHandler(this.openFileButton_Click);
            this.openFileButton.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.openFileButton.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Location = new System.Drawing.Point(720, 59);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(67, 23);
            this.resetButton.TabIndex = 7;
            this.resetButton.Tag = "Reset the MCU back into application mode";
            this.resetButton.Text = "Exit DFU";
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            this.resetButton.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.resetButton.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 639);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(799, 22);
            this.statusStrip.TabIndex = 15;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // mcuLabel
            // 
            this.mcuLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mcuLabel.AutoSize = true;
            this.mcuLabel.Location = new System.Drawing.Point(546, 0);
            this.mcuLabel.Name = "mcuLabel";
            this.mcuLabel.Size = new System.Drawing.Size(84, 13);
            this.mcuLabel.TabIndex = 22;
            this.mcuLabel.Text = "MCU (AVR only)";
            // 
            // qmkGroupBox
            // 
            this.qmkGroupBox.Controls.Add(this.keymapLabel);
            this.qmkGroupBox.Controls.Add(this.keymapBox);
            this.qmkGroupBox.Controls.Add(this.keyboardBox);
            this.qmkGroupBox.Controls.Add(this.loadKeymap);
            this.qmkGroupBox.Location = new System.Drawing.Point(6, 59);
            this.qmkGroupBox.Name = "qmkGroupBox";
            this.qmkGroupBox.Size = new System.Drawing.Size(581, 48);
            this.qmkGroupBox.TabIndex = 23;
            this.qmkGroupBox.TabStop = false;
            this.qmkGroupBox.Text = "Keyboard from qmk.fm";
            // 
            // keymapLabel
            // 
            this.keymapLabel.AutoSize = true;
            this.keymapLabel.Location = new System.Drawing.Point(355, 0);
            this.keymapLabel.Name = "keymapLabel";
            this.keymapLabel.Size = new System.Drawing.Size(45, 13);
            this.keymapLabel.TabIndex = 24;
            this.keymapLabel.Text = "Keymap";
            // 
            // keymapBox
            // 
            this.keymapBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "keymap", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keymapBox.Enabled = false;
            this.keymapBox.FormattingEnabled = true;
            this.keymapBox.Items.AddRange(new object[] {
            "later version!"});
            this.keymapBox.Location = new System.Drawing.Point(353, 20);
            this.keymapBox.Name = "keymapBox";
            this.keymapBox.Size = new System.Drawing.Size(152, 21);
            this.keymapBox.TabIndex = 4;
            this.keymapBox.Tag = "The target (MCU) of the flashing";
            this.keymapBox.Text = global::QMK_Toolbox.Properties.Settings.Default.keymap;
            this.keymapBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.keymapBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // keyboardBox
            // 
            this.keyboardBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.keyboardBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.keyboardBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "keyboard", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keyboardBox.Enabled = false;
            this.keyboardBox.FormattingEnabled = true;
            this.keyboardBox.Items.AddRange(new object[] {
            "this feature coming in"});
            this.keyboardBox.Location = new System.Drawing.Point(6, 20);
            this.keyboardBox.Name = "keyboardBox";
            this.keyboardBox.Size = new System.Drawing.Size(341, 21);
            this.keyboardBox.TabIndex = 4;
            this.keyboardBox.Tag = "The target (MCU) of the flashing";
            this.keyboardBox.Text = global::QMK_Toolbox.Properties.Settings.Default.keyboard;
            this.keyboardBox.TextChanged += new System.EventHandler(this.KeyboardBox_TextChanged);
            this.keyboardBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.keyboardBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // loadKeymap
            // 
            this.loadKeymap.Enabled = false;
            this.loadKeymap.Location = new System.Drawing.Point(511, 19);
            this.loadKeymap.Name = "loadKeymap";
            this.loadKeymap.Size = new System.Drawing.Size(64, 23);
            this.loadKeymap.TabIndex = 3;
            this.loadKeymap.Tag = "Load firmware from qmk.fm for this keyboard and keymap";
            this.loadKeymap.Text = "Load";
            this.loadKeymap.UseVisualStyleBackColor = true;
            this.loadKeymap.Click += new System.EventHandler(this.loadKeymap_Click);
            this.loadKeymap.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.loadKeymap.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // fileGroupBox
            // 
            this.fileGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileGroupBox.Controls.Add(this.openFileButton);
            this.fileGroupBox.Controls.Add(this.filepathBox);
            this.fileGroupBox.Controls.Add(this.mcuLabel);
            this.fileGroupBox.Controls.Add(this.mcuBox);
            this.fileGroupBox.Location = new System.Drawing.Point(6, 5);
            this.fileGroupBox.Name = "fileGroupBox";
            this.fileGroupBox.Size = new System.Drawing.Size(786, 48);
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
            this.filepathBox.Location = new System.Drawing.Point(6, 20);
            this.filepathBox.Name = "filepathBox";
            this.filepathBox.Size = new System.Drawing.Size(464, 21);
            this.filepathBox.TabIndex = 2;
            this.filepathBox.Tag = "The path for your firmware file";
            this.filepathBox.Text = global::QMK_Toolbox.Properties.Settings.Default.hexFileSetting;
            this.filepathBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.filepathBox_KeyDown);
            this.filepathBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.filepathBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // mcuBox
            // 
            this.mcuBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mcuBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "targetSetting", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.mcuBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mcuBox.FormattingEnabled = true;
            this.mcuBox.Location = new System.Drawing.Point(546, 20);
            this.mcuBox.Name = "mcuBox";
            this.mcuBox.Size = new System.Drawing.Size(214, 21);
            this.mcuBox.TabIndex = 4;
            this.mcuBox.Tag = "The target (MCU) of the flashing";
            this.mcuBox.Text = global::QMK_Toolbox.Properties.Settings.Default.targetSetting;
            this.mcuBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.mcuBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // clearEepromButton
            // 
            this.clearEepromButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.clearEepromButton.Location = new System.Drawing.Point(12, 613);
            this.clearEepromButton.Name = "clearEepromButton";
            this.clearEepromButton.Size = new System.Drawing.Size(110, 23);
            this.clearEepromButton.TabIndex = 27;
            this.clearEepromButton.Text = "Clear EEPROM";
            this.clearEepromButton.UseVisualStyleBackColor = true;
            this.clearEepromButton.Click += new System.EventHandler(this.clearEepromButton_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installDriversToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(129, 48);
            // 
            // installDriversToolStripMenuItem
            // 
            this.installDriversToolStripMenuItem.Name = "installDriversToolStripMenuItem";
            this.installDriversToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.installDriversToolStripMenuItem.Text = "Install Drivers...";
            this.installDriversToolStripMenuItem.Click += new System.EventHandler(this.installDriversToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // logTextBox
            // 
            this.logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logTextBox.ContextMenuStrip = this.contextMenuStrip2;
            this.logTextBox.DataBindings.Add(new System.Windows.Forms.Binding("ZoomFactor", global::QMK_Toolbox.Properties.Settings.Default, "outputZoom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.logTextBox.DetectUrls = false;
            this.logTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.ForeColor = System.Drawing.Color.White;
            this.logTextBox.HideSelection = false;
            this.logTextBox.Location = new System.Drawing.Point(12, 112);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(775, 495);
            this.logTextBox.TabIndex = 1;
            this.logTextBox.Text = "";
            this.logTextBox.WordWrap = false;
            this.logTextBox.ZoomFactor = global::QMK_Toolbox.Properties.Settings.Default.outputZoom;
            this.logTextBox.TextChanged += new System.EventHandler(this.logTextBox_TextChanged);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripMenuItem1,
            this.selectAllToolStripMenuItem,
            this.toolStripMenuItem2,
            this.clearToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.ShowImageMargin = false;
            this.contextMenuStrip2.Size = new System.Drawing.Size(140, 126);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
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
            this.hidList.Location = new System.Drawing.Point(128, 615);
            this.hidList.Name = "hidList";
            this.hidList.Size = new System.Drawing.Size(658, 21);
            this.hidList.TabIndex = 29;
            // 
            // flashWhenReadyCheckbox
            // 
            this.flashWhenReadyCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flashWhenReadyCheckbox.AutoSize = true;
            this.flashWhenReadyCheckbox.Location = new System.Drawing.Point(593, 86);
            this.flashWhenReadyCheckbox.Name = "flashWhenReadyCheckbox";
            this.flashWhenReadyCheckbox.Size = new System.Drawing.Size(109, 17);
            this.flashWhenReadyCheckbox.TabIndex = 30;
            this.flashWhenReadyCheckbox.Text = "Flash when ready";
            this.flashWhenReadyCheckbox.UseVisualStyleBackColor = true;
            this.flashWhenReadyCheckbox.CheckedChanged += new System.EventHandler(this.flashWhenReadyCheckbox_CheckedChanged);
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 661);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.flashWhenReadyCheckbox);
            this.Controls.Add(this.hidList);
            this.Controls.Add(this.clearEepromButton);
            this.Controls.Add(this.fileGroupBox);
            this.Controls.Add(this.qmkGroupBox);
            this.Controls.Add(this.flashButton);
            this.Controls.Add(this.autoflashCheckbox);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.resetButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(815, 700);
            this.Name = "MainWindow";
            this.Text = "QMK Toolbox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.Shown += new System.EventHandler(this.MainWindow_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragEnter);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.qmkGroupBox.ResumeLayout(false);
            this.qmkGroupBox.PerformLayout();
            this.fileGroupBox.ResumeLayout(false);
            this.fileGroupBox.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
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
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Label mcuLabel;
        private System.Windows.Forms.GroupBox qmkGroupBox;
        private System.Windows.Forms.ComboBox keymapBox;
        private System.Windows.Forms.ComboBox keyboardBox;
        private System.Windows.Forms.Button loadKeymap;
        private System.Windows.Forms.Label keymapLabel;
        private System.Windows.Forms.GroupBox fileGroupBox;
        private System.Windows.Forms.Button clearEepromButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ComboBox hidList;
        private System.Windows.Forms.CheckBox flashWhenReadyCheckbox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private BetterComboBox filepathBox;
        private System.Windows.Forms.ToolStripMenuItem installDriversToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    }
}

