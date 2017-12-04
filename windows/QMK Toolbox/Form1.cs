//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using QMK_Toolbox.Properties;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace QMK_Toolbox {
    using HidLibrary;
    using Newtonsoft.Json;
    using Syroot.Windows.IO;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management;
    using System.Net;
    using System.Text.RegularExpressions;

    public partial class Form1 : Form {
        BackgroundWorker backgroundWorker1;

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVNODES_CHANGED = 0x0007; //device changed
        private const int deviceIDOffset = 70;

        string filePassedIn = string.Empty;

        Printing printer;
        Flashing flasher;
        USB usb;

        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 ABOUT = 1000;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        public Form1() {
            InitializeComponent();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_DEVICECHANGE && m.WParam.ToInt32() == DBT_DEVNODES_CHANGED) {
                //Print("*** USB change\n");
            }
            if (m.Msg == NativeMethods.WM_SHOWME) {
                ShowMe();
                if (File.Exists(Path.Combine(Path.GetTempPath(), "qmk_toolbox/file_passed_in.txt"))) {
                    using (StreamReader sr = new StreamReader(Path.Combine(Path.GetTempPath(), "qmk_toolbox/file_passed_in.txt"))) {
                        setFilePath(sr.ReadLine());
                    }
                    File.Delete(Path.Combine(Path.GetTempPath(), "qmk_toolbox/file_passed_in.txt"));
                }
            }
            if (m.Msg == WM_SYSCOMMAND) {
                switch (m.WParam.ToInt32()) {
                    case ABOUT:
                        AboutBox1 aboutBox = new AboutBox1();
                        aboutBox.Show();
                        return;
                    default:
                        break;
                }
            }

            base.WndProc(ref m);
        }

        private void ShowMe() {
            if (WindowState == FormWindowState.Minimized) {
                WindowState = FormWindowState.Normal;
            }
            // get our current "TopMost" value (ours will always be false though)
            bool top = TopMost;
            // make our form jump to the top of everything
            TopMost = true;
            // set it back to whatever it was
            TopMost = top;
        }


        List<HidDevice> _devices = new List<HidDevice>();

        public Form1(string path) {
            InitializeComponent();

            if (path != string.Empty) {
                if ((Path.GetExtension(path).ToLower() == ".qmk" ||
                (Path.GetExtension(path).ToLower() == ".hex") ||
                (Path.GetExtension(path).ToLower() == ".bin"))) {
                    filePassedIn = path;
                } else {
                    MessageBox.Show("QMK Toolbox doesn't support this kind of file", "File Type Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            printer = new Printing(richTextBox1);
            flasher = new Flashing(printer);
            usb = new USB(flasher, printer);
            flasher.usb = usb;

            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.WorkerReportsProgress = true;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) {
            // This shouldn't be needed anymore
            // set the current caret position to the end
            // richTextBox1.SelectionStart = richTextBox1.Text.Length;
            // scroll it automatically
            // richTextBox1.ScrollToCaret();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            ArrayList arraylist = new ArrayList(this.filepathBox.Items);
            Settings.Default.hexFileCollection = arraylist;
            Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e) {

            IntPtr MenuHandle = GetSystemMenu(this.Handle, false);
            InsertMenu(MenuHandle, 0, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty); // <-- Add a menu seperator
            InsertMenu(MenuHandle, 0, MF_BYPOSITION, ABOUT, "About");


            backgroundWorker1.RunWorkerAsync();

            foreach (var mcu in flasher.getMCUList()) {
                mcuBox.Items.Add(mcu);
            }
            if (mcuBox.SelectedIndex == -1)
                mcuBox.SelectedIndex = 0;

            if (Settings.Default.hexFileCollection != null)
                this.filepathBox.Items.AddRange(Settings.Default.hexFileCollection.ToArray());

            richTextBox1.Font = new Font(FontFamily.GenericMonospace, 8);

            printer.print("QMK Toolbox (http://qmk.fm/toolbox)", MessageType.Info);
            printer.printResponse("Supporting following bootloaders:\n", MessageType.Info);
            printer.printResponse(" - DFU (Atmel, LUFA) via dfu-programmer (http://dfu-programmer.github.io/)\n", MessageType.Info);
            printer.printResponse(" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)\n", MessageType.Info);
            printer.printResponse(" - Halfkay (Teensy, Ergodox EZ) via teensy_loader_cli (https://pjrc.com/teensy/loader_cli.html)\n", MessageType.Info);
            printer.printResponse(" - STM32 (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
            printer.printResponse(" - Kiibohd (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
            printer.printResponse("And the following ISP flasher protocols:\n", MessageType.Info);
            printer.printResponse(" - USBTiny (AVR Pocket)\n", MessageType.Info);
            printer.printResponse(" - AVRISP (Arduino ISP)\n", MessageType.Info);

            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                collection = searcher.Get();

            usb.DetectBootloaderFromCollection(collection);

            UpdateHIDDevices();

            if (filePassedIn != string.Empty)
                setFilePath(filePassedIn);

            LoadKeyboardList();
            LoadKeymapList();

        }

        private void LoadKeyboardList() {
            using (WebClient wc = new WebClient()) {
                var json = wc.DownloadString("http://compile.qmk.fm/v1/keyboards");
                List<String> keyboards = JsonConvert.DeserializeObject<List<String>>(json);
                keyboardBox.Items.Clear();
                foreach (var keyboard in keyboards) {
                    keyboardBox.Items.Add(keyboard);
                }
                if (keyboardBox.SelectedIndex == -1)
                    keyboardBox.SelectedIndex = 0;
                keyboardBox.Enabled = true;
            }
        }

        private void LoadKeymapList() {
            keymapBox.Items.Clear();
            keymapBox.Items.Add("default");
            keymapBox.SelectedIndex = 0;
            // keymapBox.Enabled = true;
            loadKeymap.Enabled = true;
        }

        private void loadKeymap_Click(object sender, EventArgs e) {
            setFilePath("qmk:http://qmk.fm/compiled/" + keyboardBox.SelectedItem.ToString().Replace("/", "_") + "_default.hex");
        }

        private void flashButton_Click(object sender, EventArgs e) {
            if (!InvokeRequired) {
                flashButton.Enabled = false;
                resetButton.Enabled = false;

                if (usb.areDevicesAvailable()) {
                    int error = 0;
                    if (mcuBox.Text == "") {
                        printer.print("Please select a microcontroller", MessageType.Error);
                        error++;
                    }
                    if (filepathBox.Text == "") {
                        printer.print("Please select a file", MessageType.Error);
                        error++;
                    }
                    if (error == 0) {
                        printer.print("Attempting to flash, please don't remove device", MessageType.Bootloader);
                        flasher.flash(mcuBox.Text, filepathBox.Text);
                    }
                } else {
                    printer.print("There are no devices available", MessageType.Error);
                }

                flashButton.Enabled = true;
                resetButton.Enabled = true;
            } else {
                this.Invoke(new Action<object, EventArgs>(flashButton_Click), new object[] { sender, e });
            }
        }

        private void resetButton_Click(object sender, EventArgs e) {
            if (!InvokeRequired) {
                flashButton.Enabled = false;
                resetButton.Enabled = false;

                if (usb.areDevicesAvailable()) {
                    int error = 0;
                    if (mcuBox.Text == "") {
                        printer.print("Please select a microcontroller", MessageType.Error);
                        error++;
                    }
                    if (error == 0) {
                        flasher.reset(mcuBox.Text);
                    }
                } else {
                    printer.print("There are no devices available", MessageType.Error);
                }

                flashButton.Enabled = true;
                resetButton.Enabled = true;
            } else {
                this.Invoke(new Action<object, EventArgs>(resetButton_Click), new object[] { sender, e });
            }
        }

        private void eepromResetButton_Click(object sender, EventArgs e) {
            if (!InvokeRequired) {
                eepromResetButton.Enabled = false;

                if (usb.areDevicesAvailable()) {
                    int error = 0;
                    if (mcuBox.Text == "") {
                        printer.print("Please select a microcontroller", MessageType.Error);
                        error++;
                    }
                    if (error == 0) {
                        flasher.eepromReset(mcuBox.Text);
                    }
                } else {
                    printer.print("There are no devices available", MessageType.Error);
                }
                
                eepromResetButton.Enabled = true;
            } else {
                this.Invoke(new Action<object, EventArgs>(eepromResetButton_Click), new object[] { sender, e });
            }
        }

        private void UpdateHIDDevices() {
            List<HidDevice> devices = new List<HidDevice>(GetListableDevices());

            foreach (HidDevice device in devices) {
                bool device_exists = false;
                foreach (HidDevice existing_device in _devices) {
                    device_exists |= existing_device.DevicePath.Equals(device.DevicePath);
                }
                if (device != null && !device_exists) {
                    _devices.Add(device);
                    device.OpenDevice();
                    //device.Inserted += DeviceAttachedHandler;
                    //device.Removed += DeviceRemovedHandler;

                    device.MonitorDeviceEvents = true;

                    printer.print(GetManufacturerString(device) + ": " + GetProductString(device) + " connected " + 
                        " -- " + device.Attributes.VendorId.ToString("X4") + ":" + device.Attributes.ProductId.ToString("X4") + ":" + device.Attributes.Version.ToString("X4") + " (" + GetParentIDPrefix(device) + ")", MessageType.HID);

                    device.ReadReport(OnReport);
                    device.CloseDevice();
                }
            }
            foreach (HidDevice existing_device in _devices) {
                bool device_exists = false;
                foreach (HidDevice device in devices) {
                    device_exists |= existing_device.DevicePath.Equals(device.DevicePath);
                }
                if (!device_exists) {
                    printer.print("HID device disconnected" + 
                    " -- " + existing_device.Attributes.VendorId.ToString("X4") + ":" + existing_device.Attributes.ProductId.ToString("X4") + ":" + existing_device.Attributes.Version.ToString("X4") + " (" + GetParentIDPrefix(existing_device) + ")", MessageType.HID);
                }
            }
            _devices = devices;
        }

        private string GetParentIDPrefix(HidDevice d) {
            Regex regex = new Regex("#([&0-9a-fA-F]+)#");
            var vp = regex.Match(d.DevicePath);
            return vp.Groups[1].ToString();
        }

        private IEnumerable<HidDevice> GetListableDevices() {
            var devices = HidDevices.Enumerate();
            List<HidDevice> listenable_devices = new List<HidDevice>();
            foreach (HidDevice device in devices) {
                if ((ushort)device.Capabilities.UsagePage == Flashing.UsagePage)
                    listenable_devices.Add(device);
            }
            return listenable_devices;
        }

        private string GetProductString(HidDevice d) {
            if (d == null)
                return "";
            byte[] bs;
            d.ReadProduct(out bs);
            string ps = "";
            foreach (byte b in bs) {
                if (b > 0)
                    ps += ((char)b).ToString();
            }
            return ps;
        }

        private string GetManufacturerString(HidDevice d) {
            if (d == null)
                return "";
            byte[] bs;
            d.ReadManufacturer(out bs);
            string ps = "";
            foreach (byte b in bs) {
                if (b > 0)
                    ps += ((char)b).ToString();
            }
            return ps;
        }

        private void DeviceAttachedHandler() {
            // not sure if this will be useful
        }

        private void DeviceRemovedHandler() {
            // not sure if this will be useful
        }

        private void OnReport(HidReport report) {

            var data = report.Data;
            //Print(string.Format("* recv {0} bytes:", data.Length));

            string outputString = string.Empty;
            for (int i = 0; i < data.Length; i++) {
                outputString += (char)data[i];
                if (i % 16 == 15 && i < data.Length) {
                    printer.printResponse(outputString, MessageType.HID);
                    outputString = string.Empty;
                }
            }

            foreach (HidDevice device in _devices) {
                device.ReadReport(OnReport);
            }
        }

        private void filepathBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 13) {
                setFilePath(filepathBox.Text);
            }
        }

        private void setFilePath(string filepath) {
            Uri uri = new Uri(filepath);
            if (uri.Scheme == "qmk") {
                string url;
                if (filepath.Contains("qmk://"))
                    url = filepath.Replace("qmk://", "");
                else
                    url = filepath.Replace("qmk:", "");
                printer.print("Downloading the file: " + url, MessageType.Info);
                WebClient wb = new WebClient();
                if (!Directory.Exists(Path.Combine(Application.LocalUserAppDataPath, "downloads"))) {
                    Directory.CreateDirectory(Path.Combine(Application.LocalUserAppDataPath, "downloads"));
                }
                wb.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.33 Safari/537.36");
                filepath = Path.Combine(KnownFolders.Downloads.Path, filepath.Substring(filepath.LastIndexOf("/") + 1).Replace(".", "_" + Guid.NewGuid().ToString().Substring(0, 8) + "."));
                wb.DownloadFile(url, filepath);
                printer.printResponse("File saved to: " + filepath, MessageType.Info);
            }
            if (filepath.EndsWith(".qmk", true, null)) {
                printer.print("Found .qmk file", MessageType.Info);
                string qmk_filepath = Path.GetTempPath() + "qmk_toolbox" + filepath.Substring(filepath.LastIndexOf("\\")) + "\\";
                printer.printResponse("Extracting to " + qmk_filepath + "\n", MessageType.Info);
                if (Directory.Exists(qmk_filepath))
                    Directory.Delete(qmk_filepath, true);
                ZipFile.ExtractToDirectory(filepath, qmk_filepath);
                string[] files = Directory.GetFiles(qmk_filepath);
                string readme = "";
                Info info = new Info();
                foreach (string file in files) {
                    printer.printResponse(" - " + file.Substring(file.LastIndexOf("\\") + 1) + "\n", MessageType.Info);
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("firmware.hex", StringComparison.OrdinalIgnoreCase) || 
                        file.Substring(file.LastIndexOf("\\") + 1).Equals("firmware.bin", StringComparison.OrdinalIgnoreCase))
                        setFilePath(file);
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("readme.md", StringComparison.OrdinalIgnoreCase))
                        readme = System.IO.File.ReadAllText(file);
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("info.json", StringComparison.OrdinalIgnoreCase))
                        info = JsonConvert.DeserializeObject<Info>(System.IO.File.ReadAllText(file));
                }
                if (!string.IsNullOrEmpty(info.keyboard)) {
                    printer.print("Keymap for keyboard \"" + info.keyboard + "\" - " + info.vendor_id + ":" + info.product_id, MessageType.Info);
                }
                if (!readme.Equals("")) {
                    printer.print("Notes for this keymap:", MessageType.Info);
                    printer.printResponse(readme, MessageType.Info);
                }

            } else {
                if (filepath != "") {
                    filepathBox.Text = filepath;
                    if (!filepathBox.Items.Contains(filepath))
                        filepathBox.Items.Add(filepath);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                setFilePath(openFileDialog1.FileName);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            Settings.Default.Save();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e) {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            if (usb.DetectBootloader(instance) && checkBox1.Checked) {
                flashButton_Click(sender, e);
            }
            UpdateHIDDevices();
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e) {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            usb.DetectBootloader(instance, false);
            UpdateHIDDevices();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();

            // Do something while waiting for events
            System.Threading.Thread.Sleep(2000000);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                printer.print("Auto-flash enabled", MessageType.Info);
                flashButton.Enabled = false;
                resetButton.Enabled = false;
            } else {
                printer.print("Auto-flash disabled", MessageType.Info);
                flashButton.Enabled = true;
                resetButton.Enabled = true;
            }
        }

        // Set the button's status tip.
        private void btn_MouseEnter(object sender, EventArgs e) {
            Control obj = sender as Control;
            toolStripStatusLabel1.Text = obj.Tag.ToString();
        }

        // Remove the button's status tip.
        private void btn_MouseLeave(object sender, EventArgs e) {
            //if (toolStripStatusLabel1.Text.Equals((sender as Control).Tag)) {
            //    toolStripStatusLabel1.Text = "";
            //}
        }

        private void button4_Click(object sender, EventArgs e) {
            ((Button)sender).Enabled = false;
            foreach (HidDevice device in _devices) {
                device.CloseDevice();
            }
            printer.print("Listing compatible HID devices: (must have Usage Page "+ string.Format("0x{0:X4}", Flashing.UsagePage) + ")", MessageType.HID);
            foreach (HidDevice device in _devices) {
                if (device != null) {
                    device.OpenDevice();
                    printer.printResponse((" - " + GetManufacturerString(device) + " - " + GetProductString(device) + " ").PadRight(deviceIDOffset, '-') + 
                    " | " + device.Attributes.VendorHexId + ":" + device.Attributes.ProductHexId + "\n", MessageType.Info);
                    printer.printResponse("   Parent ID Prefix: " + GetParentIDPrefix(device) + "\n", MessageType.Info);
                }
                device.CloseDevice();
            }
            ((Button)sender).Enabled = true;
        }

        private void ReportWritten(bool success) {
            if (!InvokeRequired) {
                //button5.Enabled = true;
                //button6.Enabled = true;
                if (success) {
                    printer.printResponse("Report sent sucessfully\n", MessageType.Info);
                } else {
                    printer.printResponse("Report errored\n", MessageType.Error);
                }
            } else {
                this.Invoke(new Action<bool>(ReportWritten), new object[] { success });
            }
        }

        private void button5_Click(object sender, EventArgs e) {
            button5.Enabled = false;
            foreach (HidDevice device in _devices) {
                device.CloseDevice();
            }
            foreach (HidDevice device in _devices) {
                device.OpenDevice();
                //device.Write(Encoding.ASCII.GetBytes("BOOTLOADER"), 0);
                byte[] data = new byte[2];
                data[0] = 0;
                data[1] = 0xFE;
                HidReport report = new HidReport(2, new HidDeviceData(data, HidDeviceData.ReadStatus.Success));
                device.WriteReport(report, ReportWritten);
                device.CloseDevice();
                printer.print("Sending report", MessageType.HID);
            }
        }

        private void button6_Click(object sender, EventArgs e) {
            button6.Enabled = false;
            foreach (HidDevice device in _devices) {
                device.CloseDevice();
            }
            foreach (HidDevice device in _devices) {
                device.OpenDevice();
                //device.Write(Encoding.ASCII.GetBytes("BOOTLOADER"), 0);
                byte[] data = new byte[2];
                data[0] = 0;
                data[1] = 0x01;
                HidReport report = new HidReport(2, new HidDeviceData(data, HidDeviceData.ReadStatus.Success));
                device.WriteReport(report, ReportWritten);
                device.CloseDevice();
                printer.print("Sending report", MessageType.HID);
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e) {
            setFilePath(((string[])e.Data.GetData(DataFormats.FileDrop, false)).First());
        }

        private void Form1_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

    }

    class USBDeviceInfo {
        public USBDeviceInfo(string deviceID, string pnpDeviceID, string description) {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
    }

}
