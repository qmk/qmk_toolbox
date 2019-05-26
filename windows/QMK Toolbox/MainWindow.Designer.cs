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
            this.listHidDevicesButton = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.jumpToBootloaderButton = new System.Windows.Forms.Button();
            this.sayHelloButton = new System.Windows.Forms.Button();
            this.mcuLabel = new System.Windows.Forms.Label();
            this.qmkGroupBox = new System.Windows.Forms.GroupBox();
            this.keymapLabel = new System.Windows.Forms.Label();
            this.keymapBox = new System.Windows.Forms.ComboBox();
            this.keyboardBox = new System.Windows.Forms.ComboBox();
            this.loadKeymap = new System.Windows.Forms.Button();
            this.fileGroupBox = new System.Windows.Forms.GroupBox();
            this.filepathBox = new BetterComboBox();
            this.mcuBox = new System.Windows.Forms.ComboBox();
            this.dfuCheckbox = new System.Windows.Forms.CheckBox();
            this.caterinaCheckbox = new System.Windows.Forms.CheckBox();
            this.halfkayCheckbox = new System.Windows.Forms.CheckBox();
            this.stm32Checkbox = new System.Windows.Forms.CheckBox();
            this.enabledFlasherGroupBox = new System.Windows.Forms.GroupBox();
            this.eepromResetButton = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hidList = new System.Windows.Forms.ComboBox();
            this.flashWhenReadyCheckbox = new System.Windows.Forms.CheckBox();
            this.statusStrip.SuspendLayout();
            this.qmkGroupBox.SuspendLayout();
            this.fileGroupBox.SuspendLayout();
            this.enabledFlasherGroupBox.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // flashButton
            // 
            this.flashButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flashButton.Location = new System.Drawing.Point(667, 54);
            this.flashButton.Name = "flashButton";
            this.flashButton.Size = new System.Drawing.Size(57, 21);
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
            this.autoflashCheckbox.Location = new System.Drawing.Point(711, 79);
            this.autoflashCheckbox.Name = "autoflashCheckbox";
            this.autoflashCheckbox.Size = new System.Drawing.Size(75, 16);
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
            this.openFileDialog.Filter = "Intel Hex|*.hex|Binary|*.bin";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // openFileButton
            // 
            this.openFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openFileButton.Location = new System.Drawing.Point(476, 16);
            this.openFileButton.Name = "openFileButton";
            this.openFileButton.Size = new System.Drawing.Size(64, 21);
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
            this.resetButton.Location = new System.Drawing.Point(730, 54);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(57, 21);
            this.resetButton.TabIndex = 7;
            this.resetButton.Tag = "Reset the MCU back into application mode";
            this.resetButton.Text = "Reset";
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            this.resetButton.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.resetButton.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // listHidDevicesButton
            // 
            this.listHidDevicesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.listHidDevicesButton.Location = new System.Drawing.Point(688, 566);
            this.listHidDevicesButton.Name = "listHidDevicesButton";
            this.listHidDevicesButton.Size = new System.Drawing.Size(99, 21);
            this.listHidDevicesButton.TabIndex = 19;
            this.listHidDevicesButton.Tag = "List all HID devices that are compatible with HID listen (must use a certain usag" +
    "e page)";
            this.listHidDevicesButton.Text = "List HID Devices";
            this.listHidDevicesButton.UseVisualStyleBackColor = true;
            this.listHidDevicesButton.Click += new System.EventHandler(this.listHidDevicesButton_Click);
            this.listHidDevicesButton.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.listHidDevicesButton.MouseLeave += new System.EventHandler(this.btn_MouseLeave);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 588);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(799, 22);
            this.statusStrip.TabIndex = 15;
            this.statusStrip.Text = "statusStrip";
            this.statusStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip_ItemClicked);
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // jumpToBootloaderButton
            // 
            this.jumpToBootloaderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.jumpToBootloaderButton.Enabled = false;
            this.jumpToBootloaderButton.Location = new System.Drawing.Point(97, 566);
            this.jumpToBootloaderButton.Name = "jumpToBootloaderButton";
            this.jumpToBootloaderButton.Size = new System.Drawing.Size(119, 21);
            this.jumpToBootloaderButton.TabIndex = 20;
            this.jumpToBootloaderButton.Tag = "Experimental feature for QMK boards using the hid_api branch";
            this.jumpToBootloaderButton.Text = "Jump to Bootloader";
            this.jumpToBootloaderButton.UseVisualStyleBackColor = true;
            this.jumpToBootloaderButton.Click += new System.EventHandler(this.jumpToBootloaderButton_Click);
            // 
            // sayHelloButton
            // 
            this.sayHelloButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.sayHelloButton.Location = new System.Drawing.Point(12, 566);
            this.sayHelloButton.Name = "sayHelloButton";
            this.sayHelloButton.Size = new System.Drawing.Size(78, 21);
            this.sayHelloButton.TabIndex = 21;
            this.sayHelloButton.Tag = "Experimental feature for QMK boards using the hid_api branch";
            this.sayHelloButton.Text = "Say Hello";
            this.sayHelloButton.UseVisualStyleBackColor = true;
            this.sayHelloButton.Click += new System.EventHandler(this.sayHelloButton_Click);
            // 
            // mcuLabel
            // 
            this.mcuLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mcuLabel.AutoSize = true;
            this.mcuLabel.Location = new System.Drawing.Point(546, 0);
            this.mcuLabel.Name = "mcuLabel";
            this.mcuLabel.Size = new System.Drawing.Size(78, 12);
            this.mcuLabel.TabIndex = 22;
            this.mcuLabel.Text = "Microcontroller";
            // 
            // qmkGroupBox
            // 
            this.qmkGroupBox.Controls.Add(this.keymapLabel);
            this.qmkGroupBox.Controls.Add(this.keymapBox);
            this.qmkGroupBox.Controls.Add(this.keyboardBox);
            this.qmkGroupBox.Controls.Add(this.loadKeymap);
            this.qmkGroupBox.Location = new System.Drawing.Point(6, 54);
            this.qmkGroupBox.Name = "qmkGroupBox";
            this.qmkGroupBox.Size = new System.Drawing.Size(444, 44);
            this.qmkGroupBox.TabIndex = 23;
            this.qmkGroupBox.TabStop = false;
            this.qmkGroupBox.Text = "Keyboard from qmk.fm";
            // 
            // keymapLabel
            // 
            this.keymapLabel.AutoSize = true;
            this.keymapLabel.Location = new System.Drawing.Point(213, 0);
            this.keymapLabel.Name = "keymapLabel";
            this.keymapLabel.Size = new System.Drawing.Size(44, 12);
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
            this.keymapBox.Location = new System.Drawing.Point(214, 18);
            this.keymapBox.Name = "keymapBox";
            this.keymapBox.Size = new System.Drawing.Size(152, 20);
            this.keymapBox.TabIndex = 4;
            this.keymapBox.Tag = "The target (MCU) of the flashing";
            this.keymapBox.Text = global::QMK_Toolbox.Properties.Settings.Default.keymap;
            this.keymapBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.keymapBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // keyboardBox
            // 
            this.keyboardBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "keyboard", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keyboardBox.Enabled = false;
            this.keyboardBox.FormattingEnabled = true;
            this.keyboardBox.Items.AddRange(new object[] {
            "this feature coming in"});
            this.keyboardBox.Location = new System.Drawing.Point(6, 18);
            this.keyboardBox.Name = "keyboardBox";
            this.keyboardBox.Size = new System.Drawing.Size(202, 20);
            this.keyboardBox.TabIndex = 4;
            this.keyboardBox.Tag = "The target (MCU) of the flashing";
            this.keyboardBox.Text = global::QMK_Toolbox.Properties.Settings.Default.keyboard;
            this.keyboardBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.keyboardBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // loadKeymap
            // 
            this.loadKeymap.Enabled = false;
            this.loadKeymap.Location = new System.Drawing.Point(372, 17);
            this.loadKeymap.Name = "loadKeymap";
            this.loadKeymap.Size = new System.Drawing.Size(64, 21);
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
            this.fileGroupBox.Size = new System.Drawing.Size(786, 44);
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
            this.filepathBox.Location = new System.Drawing.Point(6, 18);
            this.filepathBox.Name = "filepathBox";
            this.filepathBox.Size = new System.Drawing.Size(464, 20);
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
            this.mcuBox.FormattingEnabled = true;
            this.mcuBox.Location = new System.Drawing.Point(546, 16);
            this.mcuBox.Name = "mcuBox";
            this.mcuBox.Size = new System.Drawing.Size(214, 20);
            this.mcuBox.TabIndex = 4;
            this.mcuBox.Tag = "The target (MCU) of the flashing";
            this.mcuBox.Text = global::QMK_Toolbox.Properties.Settings.Default.targetSetting;
            this.mcuBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.mcuBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // dfuCheckbox
            // 
            this.dfuCheckbox.AutoSize = true;
            this.dfuCheckbox.BackColor = System.Drawing.Color.Transparent;
            this.dfuCheckbox.Checked = true;
            this.dfuCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dfuCheckbox.Enabled = false;
            this.dfuCheckbox.Location = new System.Drawing.Point(6, 12);
            this.dfuCheckbox.Name = "dfuCheckbox";
            this.dfuCheckbox.Size = new System.Drawing.Size(46, 16);
            this.dfuCheckbox.TabIndex = 5;
            this.dfuCheckbox.Tag = "Atmel AVR, Lufa";
            this.dfuCheckbox.Text = "DFU";
            this.dfuCheckbox.UseVisualStyleBackColor = false;
            this.dfuCheckbox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.dfuCheckbox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // caterinaCheckbox
            // 
            this.caterinaCheckbox.AutoSize = true;
            this.caterinaCheckbox.BackColor = System.Drawing.Color.Transparent;
            this.caterinaCheckbox.Checked = true;
            this.caterinaCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.caterinaCheckbox.Enabled = false;
            this.caterinaCheckbox.Location = new System.Drawing.Point(66, 27);
            this.caterinaCheckbox.Name = "caterinaCheckbox";
            this.caterinaCheckbox.Size = new System.Drawing.Size(63, 16);
            this.caterinaCheckbox.TabIndex = 5;
            this.caterinaCheckbox.Tag = "Arduino, Pro micros";
            this.caterinaCheckbox.Text = "Caterina";
            this.caterinaCheckbox.UseVisualStyleBackColor = false;
            this.caterinaCheckbox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.caterinaCheckbox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // halfkayCheckbox
            // 
            this.halfkayCheckbox.AutoSize = true;
            this.halfkayCheckbox.BackColor = System.Drawing.Color.Transparent;
            this.halfkayCheckbox.Checked = true;
            this.halfkayCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.halfkayCheckbox.Enabled = false;
            this.halfkayCheckbox.Location = new System.Drawing.Point(66, 12);
            this.halfkayCheckbox.Name = "halfkayCheckbox";
            this.halfkayCheckbox.Size = new System.Drawing.Size(61, 16);
            this.halfkayCheckbox.TabIndex = 5;
            this.halfkayCheckbox.Tag = "Teensy";
            this.halfkayCheckbox.Text = "Halfkay";
            this.halfkayCheckbox.UseVisualStyleBackColor = false;
            this.halfkayCheckbox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.halfkayCheckbox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // stm32Checkbox
            // 
            this.stm32Checkbox.AutoSize = true;
            this.stm32Checkbox.BackColor = System.Drawing.Color.Transparent;
            this.stm32Checkbox.Checked = true;
            this.stm32Checkbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.stm32Checkbox.Enabled = false;
            this.stm32Checkbox.Location = new System.Drawing.Point(6, 27);
            this.stm32Checkbox.Name = "stm32Checkbox";
            this.stm32Checkbox.Size = new System.Drawing.Size(59, 16);
            this.stm32Checkbox.TabIndex = 5;
            this.stm32Checkbox.Tag = "ARM Boards";
            this.stm32Checkbox.Text = "STM32";
            this.stm32Checkbox.UseVisualStyleBackColor = false;
            this.stm32Checkbox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.stm32Checkbox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // enabledFlasherGroupBox
            // 
            this.enabledFlasherGroupBox.Controls.Add(this.dfuCheckbox);
            this.enabledFlasherGroupBox.Controls.Add(this.halfkayCheckbox);
            this.enabledFlasherGroupBox.Controls.Add(this.caterinaCheckbox);
            this.enabledFlasherGroupBox.Controls.Add(this.stm32Checkbox);
            this.enabledFlasherGroupBox.Location = new System.Drawing.Point(456, 53);
            this.enabledFlasherGroupBox.Name = "enabledFlasherGroupBox";
            this.enabledFlasherGroupBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.enabledFlasherGroupBox.Size = new System.Drawing.Size(130, 44);
            this.enabledFlasherGroupBox.TabIndex = 26;
            this.enabledFlasherGroupBox.TabStop = false;
            this.enabledFlasherGroupBox.Text = "Flashers enabled";
            // 
            // eepromResetButton
            // 
            this.eepromResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.eepromResetButton.Location = new System.Drawing.Point(572, 566);
            this.eepromResetButton.Name = "eepromResetButton";
            this.eepromResetButton.Size = new System.Drawing.Size(110, 21);
            this.eepromResetButton.TabIndex = 27;
            this.eepromResetButton.Text = "Reset EEPROM";
            this.eepromResetButton.UseVisualStyleBackColor = true;
            this.eepromResetButton.Click += new System.EventHandler(this.eepromResetButton_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(110, 26);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.aboutToolStripMenuItem.Text = "About";
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
            this.logTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.ForeColor = System.Drawing.Color.White;
            this.logTextBox.HideSelection = false;
            this.logTextBox.Location = new System.Drawing.Point(12, 103);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(775, 457);
            this.logTextBox.TabIndex = 1;
            this.logTextBox.Text = "";
            this.logTextBox.WordWrap = false;
            this.logTextBox.ZoomFactor = global::QMK_Toolbox.Properties.Settings.Default.outputZoom;
            this.logTextBox.TextChanged += new System.EventHandler(this.logTextBox_TextChanged);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(181, 48);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // hidList
            // 
            this.hidList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hidList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.hidList.FormattingEnabled = true;
            this.hidList.Location = new System.Drawing.Point(222, 568);
            this.hidList.Name = "hidList";
            this.hidList.Size = new System.Drawing.Size(329, 20);
            this.hidList.TabIndex = 29;
            // 
            // flashWhenReadyCheckbox
            // 
            this.flashWhenReadyCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flashWhenReadyCheckbox.AutoSize = true;
            this.flashWhenReadyCheckbox.Location = new System.Drawing.Point(597, 79);
            this.flashWhenReadyCheckbox.Name = "flashWhenReadyCheckbox";
            this.flashWhenReadyCheckbox.Size = new System.Drawing.Size(105, 16);
            this.flashWhenReadyCheckbox.TabIndex = 30;
            this.flashWhenReadyCheckbox.Text = "Flash when ready";
            this.flashWhenReadyCheckbox.UseVisualStyleBackColor = true;
            this.flashWhenReadyCheckbox.CheckedChanged += new System.EventHandler(this.flashWhenReadyCheckbox_CheckedChanged);
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 610);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.flashWhenReadyCheckbox);
            this.Controls.Add(this.hidList);
            this.Controls.Add(this.eepromResetButton);
            this.Controls.Add(this.enabledFlasherGroupBox);
            this.Controls.Add(this.fileGroupBox);
            this.Controls.Add(this.qmkGroupBox);
            this.Controls.Add(this.listHidDevicesButton);
            this.Controls.Add(this.jumpToBootloaderButton);
            this.Controls.Add(this.flashButton);
            this.Controls.Add(this.sayHelloButton);
            this.Controls.Add(this.autoflashCheckbox);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.resetButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(815, 649);
            this.Name = "MainWindow";
            this.Text = "QMK Toolbox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainWindow_DragEnter);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.qmkGroupBox.ResumeLayout(false);
            this.qmkGroupBox.PerformLayout();
            this.fileGroupBox.ResumeLayout(false);
            this.fileGroupBox.PerformLayout();
            this.enabledFlasherGroupBox.ResumeLayout(false);
            this.enabledFlasherGroupBox.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox filepathBox;
        private System.Windows.Forms.ComboBox mcuBox;
        private System.Windows.Forms.Button flashButton;
        private System.Windows.Forms.CheckBox autoflashCheckbox;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.RichTextBox logTextBox;
        private System.Windows.Forms.Button openFileButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Button listHidDevicesButton;
        private System.Windows.Forms.Button jumpToBootloaderButton;
        private System.Windows.Forms.Button sayHelloButton;
        private System.Windows.Forms.Label mcuLabel;
        private System.Windows.Forms.GroupBox qmkGroupBox;
        private System.Windows.Forms.ComboBox keymapBox;
        private System.Windows.Forms.ComboBox keyboardBox;
        private System.Windows.Forms.Button loadKeymap;
        private System.Windows.Forms.Label keymapLabel;
        private System.Windows.Forms.GroupBox fileGroupBox;
        private System.Windows.Forms.CheckBox dfuCheckbox;
        private System.Windows.Forms.CheckBox caterinaCheckbox;
        private System.Windows.Forms.CheckBox halfkayCheckbox;
        private System.Windows.Forms.CheckBox stm32Checkbox;
        private System.Windows.Forms.GroupBox enabledFlasherGroupBox;
        private System.Windows.Forms.Button eepromResetButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ComboBox hidList;
        private System.Windows.Forms.CheckBox flashWhenReadyCheckbox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
    }
}

