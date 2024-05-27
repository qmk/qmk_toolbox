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
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Properties.Settings settings1 = new Properties.Settings();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            flashButton = new System.Windows.Forms.Button();
            windowStateBindingSource = new System.Windows.Forms.BindingSource(components);
            autoflashCheckbox = new System.Windows.Forms.CheckBox();
            openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileButton = new System.Windows.Forms.Button();
            resetButton = new System.Windows.Forms.Button();
            mcuLabel = new System.Windows.Forms.Label();
            fileGroupBox = new System.Windows.Forms.GroupBox();
            filepathBox = new BetterComboBox();
            mcuBox = new MicrocontrollerSelector();
            clearEepromButton = new System.Windows.Forms.Button();
            logTextBox = new LogTextBox();
            logContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logContextMenuSep1 = new System.Windows.Forms.ToolStripSeparator();
            selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logContextMenuSep2 = new System.Windows.Forms.ToolStripSeparator();
            clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mainMenu = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileToolStripMenuSep = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            flashToolStripMenuItem = new BindableToolStripMenuItem();
            eepromToolStripMenuItem = new BindableToolStripMenuItem();
            eepromClearToolStripMenuItem = new BindableToolStripMenuItem();
            eepromToolStripMenuSep = new System.Windows.Forms.ToolStripSeparator();
            eepromLeftToolStripMenuItem = new BindableToolStripMenuItem();
            eepromRightToolStripMenuItem = new BindableToolStripMenuItem();
            exitDFUToolStripMenuItem = new BindableToolStripMenuItem();
            toolsToolStripMenuSep1 = new System.Windows.Forms.ToolStripSeparator();
            autoFlashToolStripMenuItem = new BindableToolStripMenuItem();
            showAllDevicesToolStripMenuItem = new BindableToolStripMenuItem();
            toolsToolStripMenuSep2 = new System.Windows.Forms.ToolStripSeparator();
            keyTesterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            hidConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            installDriversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolsToolStripMenuSep3 = new System.Windows.Forms.ToolStripSeparator();
            optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuSep = new System.Windows.Forms.ToolStripSeparator();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            clearResourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)windowStateBindingSource).BeginInit();
            fileGroupBox.SuspendLayout();
            logContextMenu.SuspendLayout();
            mainMenu.SuspendLayout();
            SuspendLayout();
            // 
            // flashButton
            // 
            flashButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            flashButton.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanFlash", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            flashButton.Enabled = false;
            flashButton.Location = new System.Drawing.Point(626, 99);
            flashButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            flashButton.Name = "flashButton";
            flashButton.Size = new System.Drawing.Size(72, 27);
            flashButton.TabIndex = 6;
            flashButton.Text = "Flash";
            flashButton.Click += FlashButton_Click;
            // 
            // windowStateBindingSource
            // 
            windowStateBindingSource.DataSource = typeof(WindowState);
            // 
            // autoflashCheckbox
            // 
            autoflashCheckbox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            autoflashCheckbox.AutoSize = true;
            autoflashCheckbox.BackColor = System.Drawing.Color.Transparent;
            autoflashCheckbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", windowStateBindingSource, "AutoFlashEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            autoflashCheckbox.Location = new System.Drawing.Point(536, 104);
            autoflashCheckbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            autoflashCheckbox.Name = "autoflashCheckbox";
            autoflashCheckbox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            autoflashCheckbox.Size = new System.Drawing.Size(84, 19);
            autoflashCheckbox.TabIndex = 5;
            autoflashCheckbox.Text = "Auto-Flash";
            autoflashCheckbox.UseVisualStyleBackColor = false;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "Intel Hex and Binary (*.hex;*.bin)|*.hex;*.bin|Intel Hex (*.hex)|*.hex|Binary (*.bin)|*.bin";
            // 
            // openFileButton
            // 
            openFileButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            openFileButton.Location = new System.Drawing.Point(666, 21);
            openFileButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            openFileButton.Name = "openFileButton";
            openFileButton.Size = new System.Drawing.Size(75, 27);
            openFileButton.TabIndex = 2;
            openFileButton.Text = "Open";
            openFileButton.UseVisualStyleBackColor = true;
            openFileButton.Click += OpenFileButton_Click;
            // 
            // resetButton
            // 
            resetButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            resetButton.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanReset", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resetButton.Enabled = false;
            resetButton.Location = new System.Drawing.Point(841, 99);
            resetButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            resetButton.Name = "resetButton";
            resetButton.Size = new System.Drawing.Size(78, 27);
            resetButton.TabIndex = 8;
            resetButton.Text = "Exit DFU";
            resetButton.Click += ResetButton_Click;
            // 
            // mcuLabel
            // 
            mcuLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            mcuLabel.AutoSize = true;
            mcuLabel.Location = new System.Drawing.Point(744, 0);
            mcuLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            mcuLabel.Name = "mcuLabel";
            mcuLabel.Size = new System.Drawing.Size(92, 15);
            mcuLabel.TabIndex = 3;
            mcuLabel.Text = "MCU (AVR only)";
            // 
            // fileGroupBox
            // 
            fileGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            fileGroupBox.Controls.Add(openFileButton);
            fileGroupBox.Controls.Add(filepathBox);
            fileGroupBox.Controls.Add(mcuLabel);
            fileGroupBox.Controls.Add(mcuBox);
            fileGroupBox.Location = new System.Drawing.Point(14, 37);
            fileGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            fileGroupBox.Name = "fileGroupBox";
            fileGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            fileGroupBox.Size = new System.Drawing.Size(905, 55);
            fileGroupBox.TabIndex = 0;
            fileGroupBox.TabStop = false;
            fileGroupBox.Text = "Local file";
            // 
            // filepathBox
            // 
            filepathBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
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
            filepathBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", settings1, "hexFileSetting", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            filepathBox.FormattingEnabled = true;
            filepathBox.Location = new System.Drawing.Point(7, 22);
            filepathBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            filepathBox.Name = "filepathBox";
            filepathBox.PlaceholderText = "Click Open or drag to window to select file";
            filepathBox.Size = new System.Drawing.Size(650, 23);
            filepathBox.TabIndex = 1;
            filepathBox.KeyDown += FilepathBox_KeyDown;
            // 
            // mcuBox
            // 
            mcuBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            mcuBox.DisplayMember = "Value";
            mcuBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            mcuBox.FormattingEnabled = true;
            mcuBox.Location = new System.Drawing.Point(748, 22);
            mcuBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            mcuBox.Name = "mcuBox";
            mcuBox.Size = new System.Drawing.Size(150, 23);
            mcuBox.TabIndex = 4;
            mcuBox.ValueMember = "Key";
            // 
            // clearEepromButton
            // 
            clearEepromButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            clearEepromButton.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanClearEeprom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            clearEepromButton.Enabled = false;
            clearEepromButton.Location = new System.Drawing.Point(706, 99);
            clearEepromButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            clearEepromButton.Name = "clearEepromButton";
            clearEepromButton.Size = new System.Drawing.Size(128, 27);
            clearEepromButton.TabIndex = 7;
            clearEepromButton.Text = "Clear EEPROM";
            clearEepromButton.UseVisualStyleBackColor = true;
            clearEepromButton.Click += ClearEepromButton_Click;
            // 
            // logTextBox
            // 
            logTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            logTextBox.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            logTextBox.ContextMenuStrip = logContextMenu;
            logTextBox.DataBindings.Add(new System.Windows.Forms.Binding("ZoomFactor", settings1, "outputZoom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            logTextBox.DetectUrls = false;
            logTextBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            logTextBox.ForeColor = System.Drawing.Color.White;
            logTextBox.HideSelection = false;
            logTextBox.Location = new System.Drawing.Point(14, 133);
            logTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            logTextBox.Name = "logTextBox";
            logTextBox.ReadOnly = true;
            logTextBox.Size = new System.Drawing.Size(905, 620);
            logTextBox.TabIndex = 9;
            logTextBox.Text = "";
            logTextBox.WordWrap = false;
            // 
            // logContextMenu
            // 
            logContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, logContextMenuSep1, selectAllToolStripMenuItem, logContextMenuSep2, clearToolStripMenuItem });
            logContextMenu.Name = "contextMenuStrip2";
            logContextMenu.ShowImageMargin = false;
            logContextMenu.Size = new System.Drawing.Size(140, 126);
            logContextMenu.Opening += LogContextMenuStrip_Opening;
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
            copyToolStripMenuItem.Click += CopyToolStripMenuItem_Click;
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
            selectAllToolStripMenuItem.Click += SelectAllToolStripMenuItem_Click;
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
            // mainMenu
            // 
            mainMenu.BackColor = System.Drawing.Color.Transparent;
            mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            mainMenu.Location = new System.Drawing.Point(0, 0);
            mainMenu.Name = "mainMenu";
            mainMenu.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            mainMenu.Size = new System.Drawing.Size(933, 24);
            mainMenu.TabIndex = 0;
            mainMenu.Text = "mainMenu";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, fileToolStripMenuSep, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            openToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            openToolStripMenuItem.Text = "&Open...";
            openToolStripMenuItem.Click += OpenFileButton_Click;
            // 
            // fileToolStripMenuSep
            // 
            fileToolStripMenuSep.Name = "fileToolStripMenuSep";
            fileToolStripMenuSep.Size = new System.Drawing.Size(152, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4;
            exitToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += ExitMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { flashToolStripMenuItem, eepromToolStripMenuItem, exitDFUToolStripMenuItem, toolsToolStripMenuSep1, autoFlashToolStripMenuItem, showAllDevicesToolStripMenuItem, toolsToolStripMenuSep2, keyTesterToolStripMenuItem, hidConsoleToolStripMenuItem, installDriversToolStripMenuItem, clearResourcesToolStripMenuItem, toolsToolStripMenuSep3, optionsToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            toolsToolStripMenuItem.Text = "&Tools";
            // 
            // flashToolStripMenuItem
            // 
            flashToolStripMenuItem.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanFlash", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            flashToolStripMenuItem.Name = "flashToolStripMenuItem";
            flashToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F;
            flashToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            flashToolStripMenuItem.Text = "Flash";
            flashToolStripMenuItem.Click += FlashButton_Click;
            // 
            // eepromToolStripMenuItem
            // 
            eepromToolStripMenuItem.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanClearEeprom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            eepromToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { eepromClearToolStripMenuItem, eepromToolStripMenuSep, eepromLeftToolStripMenuItem, eepromRightToolStripMenuItem });
            eepromToolStripMenuItem.Name = "eepromToolStripMenuItem";
            eepromToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            eepromToolStripMenuItem.Text = "EEPROM";
            // 
            // eepromClearToolStripMenuItem
            // 
            eepromClearToolStripMenuItem.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanClearEeprom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            eepromClearToolStripMenuItem.Name = "eepromClearToolStripMenuItem";
            eepromClearToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.E;
            eepromClearToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            eepromClearToolStripMenuItem.Text = "Clear";
            eepromClearToolStripMenuItem.Click += ClearEepromButton_Click;
            // 
            // eepromToolStripMenuSep
            // 
            eepromToolStripMenuSep.Name = "eepromToolStripMenuSep";
            eepromToolStripMenuSep.Size = new System.Drawing.Size(244, 6);
            // 
            // eepromLeftToolStripMenuItem
            // 
            eepromLeftToolStripMenuItem.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanClearEeprom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            eepromLeftToolStripMenuItem.Name = "eepromLeftToolStripMenuItem";
            eepromLeftToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Left;
            eepromLeftToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            eepromLeftToolStripMenuItem.Tag = "left";
            eepromLeftToolStripMenuItem.Text = "Set Left Hand";
            eepromLeftToolStripMenuItem.Click += SetHandednessButton_Click;
            // 
            // eepromRightToolStripMenuItem
            // 
            eepromRightToolStripMenuItem.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanClearEeprom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            eepromRightToolStripMenuItem.Name = "eepromRightToolStripMenuItem";
            eepromRightToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Right;
            eepromRightToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            eepromRightToolStripMenuItem.Tag = "right";
            eepromRightToolStripMenuItem.Text = "Set Right Hand";
            eepromRightToolStripMenuItem.Click += SetHandednessButton_Click;
            // 
            // exitDFUToolStripMenuItem
            // 
            exitDFUToolStripMenuItem.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", windowStateBindingSource, "CanReset", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            exitDFUToolStripMenuItem.Name = "exitDFUToolStripMenuItem";
            exitDFUToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.X;
            exitDFUToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            exitDFUToolStripMenuItem.Text = "Exit DFU";
            exitDFUToolStripMenuItem.Click += ResetButton_Click;
            // 
            // toolsToolStripMenuSep1
            // 
            toolsToolStripMenuSep1.Name = "toolsToolStripMenuSep1";
            toolsToolStripMenuSep1.Size = new System.Drawing.Size(193, 6);
            // 
            // autoFlashToolStripMenuItem
            // 
            autoFlashToolStripMenuItem.CheckOnClick = true;
            autoFlashToolStripMenuItem.DataBindings.Add(new System.Windows.Forms.Binding("Checked", windowStateBindingSource, "AutoFlashEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            autoFlashToolStripMenuItem.Name = "autoFlashToolStripMenuItem";
            autoFlashToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            autoFlashToolStripMenuItem.Text = "Auto-Flash";
            // 
            // showAllDevicesToolStripMenuItem
            // 
            showAllDevicesToolStripMenuItem.CheckOnClick = true;
            showAllDevicesToolStripMenuItem.DataBindings.Add(new System.Windows.Forms.Binding("Checked", windowStateBindingSource, "ShowAllDevices", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            showAllDevicesToolStripMenuItem.Name = "showAllDevicesToolStripMenuItem";
            showAllDevicesToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            showAllDevicesToolStripMenuItem.Text = "Show All Devices";
            // 
            // toolsToolStripMenuSep2
            // 
            toolsToolStripMenuSep2.Name = "toolsToolStripMenuSep2";
            toolsToolStripMenuSep2.Size = new System.Drawing.Size(193, 6);
            // 
            // keyTesterToolStripMenuItem
            // 
            keyTesterToolStripMenuItem.Name = "keyTesterToolStripMenuItem";
            keyTesterToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            keyTesterToolStripMenuItem.Text = "Key Tester";
            keyTesterToolStripMenuItem.Click += KeyTesterToolStripMenuItem_Click;
            // 
            // hidConsoleToolStripMenuItem
            // 
            hidConsoleToolStripMenuItem.Name = "hidConsoleToolStripMenuItem";
            hidConsoleToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            hidConsoleToolStripMenuItem.Text = "HID Console";
            hidConsoleToolStripMenuItem.Click += HidConsoleToolStripMenuItem_Click;
            // 
            // installDriversToolStripMenuItem
            // 
            installDriversToolStripMenuItem.Name = "installDriversToolStripMenuItem";
            installDriversToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
            installDriversToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            installDriversToolStripMenuItem.Text = "I&nstall Drivers...";
            installDriversToolStripMenuItem.Click += InstallDriversMenuItem_Click;
            // 
            // toolsToolStripMenuSep3
            // 
            toolsToolStripMenuSep3.Name = "toolsToolStripMenuSep3";
            toolsToolStripMenuSep3.Size = new System.Drawing.Size(193, 6);
            // 
            // optionsToolStripMenuItem
            // 
            optionsToolStripMenuItem.Enabled = false;
            optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            optionsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P;
            optionsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            optionsToolStripMenuItem.Text = "O&ptions...";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { checkForUpdatesToolStripMenuItem, helpToolStripMenuSep, aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            checkForUpdatesToolStripMenuItem.Enabled = false;
            checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            checkForUpdatesToolStripMenuItem.Text = "Check for Updates...";
            // 
            // helpToolStripMenuSep
            // 
            helpToolStripMenuSep.Name = "helpToolStripMenuSep";
            helpToolStripMenuSep.Size = new System.Drawing.Size(177, 6);
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            aboutToolStripMenuItem.Text = "&About";
            aboutToolStripMenuItem.Click += AboutMenuItem_Click;
            // 
            // clearResourcesToolStripMenuItem
            // 
            clearResourcesToolStripMenuItem.Name = "clearResourcesToolStripMenuItem";
            clearResourcesToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            clearResourcesToolStripMenuItem.Text = "Clear Resources";
            clearResourcesToolStripMenuItem.Click += ClearResourcesMenuItem_Click;
            // 
            // MainWindow
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(933, 766);
            Controls.Add(mainMenu);
            Controls.Add(clearEepromButton);
            Controls.Add(fileGroupBox);
            Controls.Add(flashButton);
            Controls.Add(autoflashCheckbox);
            Controls.Add(logTextBox);
            Controls.Add(resetButton);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = mainMenu;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(949, 805);
            Name = "MainWindow";
            Text = "QMK Toolbox";
            FormClosing += MainWindow_FormClosing;
            FormClosed += MainWindow_FormClosed;
            Load += MainWindow_Load;
            Shown += MainWindow_Shown;
            DragDrop += MainWindow_DragDrop;
            DragEnter += MainWindow_DragEnter;
            ((System.ComponentModel.ISupportInitialize)windowStateBindingSource).EndInit();
            fileGroupBox.ResumeLayout(false);
            fileGroupBox.PerformLayout();
            logContextMenu.ResumeLayout(false);
            mainMenu.ResumeLayout(false);
            mainMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private QMK_Toolbox.MicrocontrollerSelector mcuBox;
        private System.Windows.Forms.Button flashButton;
        private System.Windows.Forms.CheckBox autoflashCheckbox;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private QMK_Toolbox.LogTextBox logTextBox;
        private System.Windows.Forms.Button openFileButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Label mcuLabel;
        private System.Windows.Forms.GroupBox fileGroupBox;
        private System.Windows.Forms.Button clearEepromButton;
        private System.Windows.Forms.ContextMenuStrip logContextMenu;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private BetterComboBox filepathBox;
        private System.Windows.Forms.ToolStripSeparator logContextMenuSep1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator logContextMenuSep2;
        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator fileToolStripMenuSep;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installDriversToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolsToolStripMenuSep1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator helpToolStripMenuSep;
        private QMK_Toolbox.BindableToolStripMenuItem flashToolStripMenuItem;
        private QMK_Toolbox.BindableToolStripMenuItem eepromToolStripMenuItem;
        private QMK_Toolbox.BindableToolStripMenuItem eepromClearToolStripMenuItem;
        private QMK_Toolbox.BindableToolStripMenuItem eepromLeftToolStripMenuItem;
        private QMK_Toolbox.BindableToolStripMenuItem eepromRightToolStripMenuItem;
        private QMK_Toolbox.BindableToolStripMenuItem exitDFUToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolsToolStripMenuSep2;
        private QMK_Toolbox.BindableToolStripMenuItem autoFlashToolStripMenuItem;
        private System.Windows.Forms.BindingSource windowStateBindingSource;
        private System.Windows.Forms.ToolStripSeparator eepromToolStripMenuSep;
        private System.Windows.Forms.ToolStripMenuItem keyTesterToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolsToolStripMenuSep3;
        private QMK_Toolbox.BindableToolStripMenuItem showAllDevicesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hidConsoleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearResourcesToolStripMenuItem;
    }
}
