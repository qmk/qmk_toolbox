namespace QMK_Toolbox {
    partial class Form1 {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.flashButton = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.keymapBox = new System.Windows.Forms.ComboBox();
            this.loadKeymap = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.eepromResetButton = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filepathBox = new System.Windows.Forms.ComboBox();
            this.mcuBox = new System.Windows.Forms.ComboBox();
            this.keyboardBox = new System.Windows.Forms.ComboBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flashButton
            // 
            this.flashButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.flashButton.Location = new System.Drawing.Point(652, 59);
            this.flashButton.Name = "flashButton";
            this.flashButton.Size = new System.Drawing.Size(57, 23);
            this.flashButton.TabIndex = 6;
            this.flashButton.Tag = "Erase, flash, and reset the MCU with the provided .hex file";
            this.flashButton.Text = "Flash";
            this.flashButton.Click += new System.EventHandler(this.flashButton_Click);
            this.flashButton.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.flashButton.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.Color.Transparent;
            this.checkBox1.Location = new System.Drawing.Point(654, 88);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(76, 17);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Tag = "Automatically flash when a device is detected in DFU mode";
            this.checkBox1.Text = "Auto-Flash";
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            this.checkBox1.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.checkBox1.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Intel Hex|*.hex|Binary|*.bin";
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(481, 17);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(64, 23);
            this.button2.TabIndex = 3;
            this.button2.Tag = "Select a file from explorer";
            this.button2.Text = "Open";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            this.button2.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.button2.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.Location = new System.Drawing.Point(715, 59);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(57, 23);
            this.resetButton.TabIndex = 7;
            this.resetButton.Tag = "Reset the MCU back into application mode";
            this.resetButton.Text = "Reset";
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            this.resetButton.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.resetButton.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(673, 613);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(99, 23);
            this.button4.TabIndex = 19;
            this.button4.Tag = "List all HID devices that are compatible with HID listen (must use a certain usag" +
    "e page)";
            this.button4.Text = "List HID Devices";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            this.button4.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.button4.MouseLeave += new System.EventHandler(this.btn_MouseLeave);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 639);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 22);
            this.statusStrip1.TabIndex = 15;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Enabled = false;
            this.button5.Location = new System.Drawing.Point(96, 613);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(119, 23);
            this.button5.TabIndex = 20;
            this.button5.Tag = "Experimental feature for QMK boards using the hid_api branch";
            this.button5.Text = "Jump to Bootloader";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button6.Enabled = false;
            this.button6.Location = new System.Drawing.Point(12, 613);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(78, 23);
            this.button6.TabIndex = 21;
            this.button6.Tag = "Experimental feature for QMK boards using the hid_api branch";
            this.button6.Text = "Say Hello";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(551, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Microcontroller";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.keymapBox);
            this.groupBox1.Controls.Add(this.keyboardBox);
            this.groupBox1.Controls.Add(this.loadKeymap);
            this.groupBox1.Location = new System.Drawing.Point(6, 58);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(444, 48);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Keyboard from qmk.fm";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(213, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Keymap";
            // 
            // keymapBox
            // 
            this.keymapBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "keymap", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keymapBox.Enabled = false;
            this.keymapBox.FormattingEnabled = true;
            this.keymapBox.Items.AddRange(new object[] {
            "later version!"});
            this.keymapBox.Location = new System.Drawing.Point(214, 19);
            this.keymapBox.Name = "keymapBox";
            this.keymapBox.Size = new System.Drawing.Size(152, 21);
            this.keymapBox.TabIndex = 4;
            this.keymapBox.Tag = "The target (MCU) of the flashing";
            this.keymapBox.Text = global::QMK_Toolbox.Properties.Settings.Default.keymap;
            this.keymapBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.keymapBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // loadKeymap
            // 
            this.loadKeymap.Enabled = false;
            this.loadKeymap.Location = new System.Drawing.Point(372, 18);
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
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.filepathBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.mcuBox);
            this.groupBox2.Location = new System.Drawing.Point(6, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(771, 48);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Local file";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.BackColor = System.Drawing.Color.Transparent;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Enabled = false;
            this.checkBox2.Location = new System.Drawing.Point(6, 13);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(48, 17);
            this.checkBox2.TabIndex = 5;
            this.checkBox2.Tag = "Atmel AVR, Lufa";
            this.checkBox2.Text = "DFU";
            this.checkBox2.UseVisualStyleBackColor = false;
            this.checkBox2.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.checkBox2.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.BackColor = System.Drawing.Color.Transparent;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Enabled = false;
            this.checkBox3.Location = new System.Drawing.Point(66, 29);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(65, 17);
            this.checkBox3.TabIndex = 5;
            this.checkBox3.Tag = "Arduino, Pro micros";
            this.checkBox3.Text = "Caterina";
            this.checkBox3.UseVisualStyleBackColor = false;
            this.checkBox3.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.checkBox3.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.BackColor = System.Drawing.Color.Transparent;
            this.checkBox4.Checked = true;
            this.checkBox4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox4.Enabled = false;
            this.checkBox4.Location = new System.Drawing.Point(66, 13);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(62, 17);
            this.checkBox4.TabIndex = 5;
            this.checkBox4.Tag = "Teensy";
            this.checkBox4.Text = "Halfkay";
            this.checkBox4.UseVisualStyleBackColor = false;
            this.checkBox4.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.checkBox4.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.BackColor = System.Drawing.Color.Transparent;
            this.checkBox5.Checked = true;
            this.checkBox5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox5.Enabled = false;
            this.checkBox5.Location = new System.Drawing.Point(6, 29);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(61, 17);
            this.checkBox5.TabIndex = 5;
            this.checkBox5.Tag = "ARM Boards";
            this.checkBox5.Text = "STM32";
            this.checkBox5.UseVisualStyleBackColor = false;
            this.checkBox5.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.checkBox5.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.checkBox2);
            this.groupBox3.Controls.Add(this.checkBox4);
            this.groupBox3.Controls.Add(this.checkBox3);
            this.groupBox3.Controls.Add(this.checkBox5);
            this.groupBox3.Location = new System.Drawing.Point(456, 57);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox3.Size = new System.Drawing.Size(130, 48);
            this.groupBox3.TabIndex = 26;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Flashers enabled";
            // 
            // eepromResetButton
            // 
            this.eepromResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.eepromResetButton.Location = new System.Drawing.Point(557, 613);
            this.eepromResetButton.Name = "eepromResetButton";
            this.eepromResetButton.Size = new System.Drawing.Size(110, 23);
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
            this.contextMenuStrip1.Size = new System.Drawing.Size(108, 26);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // filepathBox
            // 
            this.filepathBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filepathBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "hexFileSetting", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.filepathBox.FormattingEnabled = true;
            this.filepathBox.Location = new System.Drawing.Point(6, 19);
            this.filepathBox.Name = "filepathBox";
            this.filepathBox.Size = new System.Drawing.Size(469, 21);
            this.filepathBox.TabIndex = 2;
            this.filepathBox.Tag = "The path for your firmware file";
            this.filepathBox.Text = global::QMK_Toolbox.Properties.Settings.Default.hexFileSetting;
            this.filepathBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.filepathBox_KeyPress);
            this.filepathBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.filepathBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // mcuBox
            // 
            this.mcuBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mcuBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "targetSetting", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.mcuBox.FormattingEnabled = true;
            this.mcuBox.Location = new System.Drawing.Point(551, 17);
            this.mcuBox.Name = "mcuBox";
            this.mcuBox.Size = new System.Drawing.Size(214, 21);
            this.mcuBox.TabIndex = 4;
            this.mcuBox.Tag = "The target (MCU) of the flashing";
            this.mcuBox.Text = global::QMK_Toolbox.Properties.Settings.Default.targetSetting;
            this.mcuBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.mcuBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // keyboardBox
            // 
            this.keyboardBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::QMK_Toolbox.Properties.Settings.Default, "keyboard", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.keyboardBox.Enabled = false;
            this.keyboardBox.FormattingEnabled = true;
            this.keyboardBox.Items.AddRange(new object[] {
            "this feature coming in"});
            this.keyboardBox.Location = new System.Drawing.Point(6, 19);
            this.keyboardBox.Name = "keyboardBox";
            this.keyboardBox.Size = new System.Drawing.Size(202, 21);
            this.keyboardBox.TabIndex = 4;
            this.keyboardBox.Tag = "The target (MCU) of the flashing";
            this.keyboardBox.Text = global::QMK_Toolbox.Properties.Settings.Default.keyboard;
            this.keyboardBox.MouseEnter += new System.EventHandler(this.btn_MouseEnter);
            this.keyboardBox.MouseHover += new System.EventHandler(this.btn_MouseLeave);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.DataBindings.Add(new System.Windows.Forms.Binding("ZoomFactor", global::QMK_Toolbox.Properties.Settings.Default, "outputZoom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.Font = new System.Drawing.Font("Courier Prime", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.ForeColor = System.Drawing.Color.White;
            this.richTextBox1.HideSelection = false;
            this.richTextBox1.Location = new System.Drawing.Point(12, 112);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(760, 495);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            this.richTextBox1.ZoomFactor = global::QMK_Toolbox.Properties.Settings.Default.outputZoom;
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 661);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.eepromResetButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.flashButton);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.resetButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "QMK Toolbox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox filepathBox;
        private System.Windows.Forms.ComboBox mcuBox;
        private System.Windows.Forms.Button flashButton;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox keymapBox;
        private System.Windows.Forms.ComboBox keyboardBox;
        private System.Windows.Forms.Button loadKeymap;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button eepromResetButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

