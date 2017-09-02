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
using QMK_Toolbox.Properties;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace QMK_Toolbox {
    using HidLibrary;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management;
    using System.Text.RegularExpressions;

    public partial class Form1 : Form {

        private System.Diagnostics.Process process_dfu;
        private System.Diagnostics.ProcessStartInfo startInfo_dfu;
        private System.Diagnostics.Process process_avrdude;
        private System.Diagnostics.ProcessStartInfo startInfo_avrdude;
        BackgroundWorker backgroundWorker1;

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVNODES_CHANGED = 0x0007; //device changed
        private int VendorId;
        private int ProductId;
        private const ushort UsagePage = 0xFF31;
        private const int Usage = 0x0074;
        private string _COM = "";

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_DEVICECHANGE && m.WParam.ToInt32() == DBT_DEVNODES_CHANGED) {
                //Print("*** USB change\n");
            }
            base.WndProc(ref m);
        }

        HidDevice _device;
        bool _isAttached;

        public Form1() {
            InitializeComponent();

            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.WorkerReportsProgress = true;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) {
            // set the current caret position to the end
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            // scroll it automatically
            richTextBox1.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e) {
            backgroundWorker1.RunWorkerAsync();


            //richTextBox1.VisibleChanged += (sender1, e1) =>
            //{
            //    if (richTextBox1.Visible) {
            //        richTextBox1.SelectionStart = richTextBox1.TextLength;
            //        richTextBox1.ScrollToCaret();
            //    }
            //};

            string dfuPath = Application.LocalUserAppDataPath + "\\dfu-programmer.exe";
            ExtractResource("QMK_Toolbox.dfu-programmer.exe", dfuPath);

            string avrdudePath = Application.LocalUserAppDataPath + "\\avrdude.exe";
            ExtractResource("QMK_Toolbox.avrdude.exe", avrdudePath);
            string avrdudeConfPath = Application.LocalUserAppDataPath + "\\avrdude.conf";
            ExtractResource("QMK_Toolbox.avrdude.conf", avrdudeConfPath);

            string mcuListPath = Application.LocalUserAppDataPath + "\\mcu-list.txt";
            ExtractResource("QMK_Toolbox.mcu-list.txt", mcuListPath);

            // rawhid.rawhid_open(0, 0, 0, 0, 0);

            var lines = File.ReadLines(mcuListPath);
            foreach (var line in lines) {
                string[] tokens = line.Split(',');
                targetBox.Items.Add(tokens[0]);
            }

            process_dfu = new System.Diagnostics.Process();
            startInfo_dfu = new System.Diagnostics.ProcessStartInfo();
            startInfo_dfu.UseShellExecute = false;
            startInfo_dfu.RedirectStandardError = true;
            startInfo_dfu.RedirectStandardOutput = true;
            startInfo_dfu.FileName = dfuPath;
            startInfo_dfu.CreateNoWindow = true;


            Print("> dfu-programmer --version\n", true);
            startInfo_dfu.Arguments = "--version";
            process_dfu.StartInfo = startInfo_dfu;
            process_dfu.Start();
            string output = process_dfu.StandardOutput.ReadToEnd();
            output += process_dfu.StandardError.ReadToEnd();
            process_dfu.WaitForExit();
            Print(output, false, Color.LightGray);


            process_avrdude = new System.Diagnostics.Process();
            startInfo_avrdude = new System.Diagnostics.ProcessStartInfo();
            startInfo_avrdude.UseShellExecute = false;
            startInfo_avrdude.RedirectStandardError = true;
            startInfo_avrdude.RedirectStandardOutput = true;
            startInfo_avrdude.FileName = avrdudePath;
            startInfo_avrdude.CreateNoWindow = true;


            Print("> avrdude -v\n", true);
            startInfo_avrdude.Arguments = "-v";
            process_avrdude.StartInfo = startInfo_avrdude;
            process_avrdude.Start();
            output = process_avrdude.StandardOutput.ReadToEnd();
            output += process_avrdude.StandardError.ReadToEnd();
            output = string.Join("\n", output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Take(4));
            process_avrdude.WaitForExit();
            Print(output.Substring(1) + "\n", false, Color.LightGray);

            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                collection = searcher.Get();

            foreach (var device in collection) {
                var match = Regex.Match(device.GetPropertyValue("DeviceID").ToString(), @".*VID_03EB.*");
                if (match.Success) {
                    Print("*** DFU Device connected: " + device.GetPropertyValue("Name") + "\n", true, Color.Yellow);
                } else {
                    //Print("*** Device connected: " + device.GetPropertyValue("Name") + "\n", true);
                }
            }

            Connect();

            //File.Delete(exePath);

        }

        private bool Connect() {
            if (comboBox3.Text != "") {
                VendorId = int.Parse(comboBox3.Text, NumberStyles.HexNumber);
            }
            if (comboBox2.Text != "") {
                ProductId = int.Parse(comboBox2.Text, NumberStyles.HexNumber);
            }
            if (_device != null && _device.IsOpen)
                _device.CloseDevice();
            _device = HidDevices.Enumerate(VendorId, ProductId, UsagePage).FirstOrDefault();
            if (_device != null) {
                _device.OpenDevice();
                _device.Inserted += DeviceAttachedHandler;
                _device.Removed += DeviceRemovedHandler;

                _device.MonitorDeviceEvents = true;
                _isAttached = true;
                Print("*** HID device available: " + GetManufacturerString(_device) + " - " + GetProductString(_device), false, Color.DeepSkyBlue);
                Print(" / " + string.Format("0x{0:X4}", VendorId) + ":" + string.Format("0x{0:X4}", ProductId) + "\n");
            } else {
                Print("*** No HID device available\n", true, Color.DeepSkyBlue);
                _isAttached = false;
            }
            return _isAttached;
        }

        private string GetProductString(HidDevice d) {
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
            Print("*** HID device attached:  " + GetManufacturerString(_device) + " - " + GetProductString(_device), false, Color.LightSkyBlue);
            Print(" / " + _device.Attributes.VendorHexId + ":" + _device.Attributes.ProductHexId + "\n");
            if (_device != null) {
                _isAttached = true;
                _device.ReadReport(OnReport);
            }
        }

        private void DeviceRemovedHandler() {
            Print("*** HID device dettached: " + GetManufacturerString(_device) + " - " + GetProductString(_device), false, Color.LightSkyBlue);
            Print(" / " + _device.Attributes.VendorHexId + ":" + _device.Attributes.ProductHexId + "\n");
            _isAttached = false;
        }

        private void OnReport(HidReport report) {
            if (!_isAttached) {
                return;
            }

            var data = report.Data;
            //Print(string.Format("* recv {0} bytes:", data.Length));

            string outputString = string.Empty;
            for (int i = 0; i < data.Length; i++) {
                outputString += (char)data[i];
                if (i % 16 == 15 && i < data.Length) {
                    Print(outputString, false, Color.SkyBlue);
                    outputString = string.Empty;
                }
            }
            //Print("\n");
            if (_device != null)
                _device.ReadReport(OnReport);
        }

        void ExtractResource(string resource, string path) {
            Stream stream = GetType().Assembly.GetManifestResourceStream(resource);
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            File.WriteAllBytes(path, bytes);
        }

        private void FlashButton_Click(object sender, EventArgs e) {
            if (!InvokeRequired) {
                string hexFile = hexFileBox.Text;
                if (_COM != "") {
                    if (targetBox.Text == "") {
                        Print("*** Please select an MCU\n", true, Color.Red);
                    } else if (hexFile == "") {
                        Print("*** Please select a .hex file\n", true, Color.Red);
                    } else {
                        RunAvrdude(hexFile);
                    }
                    _COM = "";
                } else {
                    if (targetBox.Text == "") {
                        Print("*** Please select an MCU\n", true, Color.Red);
                    } else if (hexFile == "") {
                        Print("*** Please select a .hex file\n", true, Color.Red);
                        RunDFU("reset");
                    } else {
                        if (mcuIsAvailable()) {
                            RunDFU("erase --force");
                            RunDFU("flash " + hexFile);
                            RunDFU("reset");
                        }
                    }
                }
            } else {
                this.Invoke(new Action<object, EventArgs>(FlashButton_Click), new object[] { sender, e });
            }
        }
        private void Print(string str, bool bold = false, Color? color = null) {
            if (!InvokeRequired) {
                if (bold)
                    richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.SelectionColor = color ?? Color.White;
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectedText = str;
            } else {
                this.Invoke(new Action<string, bool, Color?>(Print), new object[] { str, bold, color });
            }
        }

        private void RunDFU(string dfuArgs) {
            Print("> dfu-programmer " + targetBox.Text + " " + dfuArgs + "\n", true);
            startInfo_dfu.Arguments = targetBox.Text + " " + dfuArgs;
            process_dfu.StartInfo = startInfo_dfu;
            process_dfu.Start();
            string output = process_dfu.StandardOutput.ReadToEnd();
            output += process_dfu.StandardError.ReadToEnd();
            process_dfu.WaitForExit();
            Print(output, false, Color.LightGray);
        }

        private void RunAvrdude(string file) {
            Print("> avrdude -p " + targetBox.Text + " -c avr109 -U flash:w:\"" + file + "\" -P " + _COM + "\n", true);
            startInfo_avrdude.Arguments = "-p " + targetBox.Text + " -c avr109 -U flash:w:\"" + file + "\":i -P " + _COM;
            process_avrdude.StartInfo = startInfo_avrdude;
            process_avrdude.Start();
            string output = process_avrdude.StandardOutput.ReadToEnd();
            output += process_avrdude.StandardError.ReadToEnd();
            process_avrdude.WaitForExit();
            Print(output, false, Color.LightGray);
        }

        private bool mcuIsAvailable() {
            startInfo_dfu.Arguments = targetBox.Text + " read";
            process_dfu.StartInfo = startInfo_dfu;
            process_dfu.Start();
            string output = process_dfu.StandardOutput.ReadToEnd();
            output += process_dfu.StandardError.ReadToEnd();
            process_dfu.WaitForExit();
            if (output.Contains("no device present")) {
                Print("*** No \""+ targetBox.Text + "\" DFU device connected\n", true, Color.Red);
                return false;
            } else {
                return true;
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                hexFileBox.Text = openFileDialog1.FileName;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            Settings.Default.Save();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e) {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            //Print("*** Device inserted: " + instance.GetPropertyValue("DeviceID") + "\n", true);
            //Print("*** Device inserted:\n", true);
            //foreach (var property in instance.Properties) {
            //    Print("  " + property.Name + ": " + property.Value + "\n", false);
            //}

            // Detects Atmel Vendor ID
            var dfu = Regex.Match(instance.GetPropertyValue("DeviceID").ToString(), @".*VID_03EB.*");
            var caterina = Regex.Match(instance.GetPropertyValue("DeviceID").ToString(), @".*VID_2341.*");
            if (dfu.Success) {
                Print("*** DFU device connected:\n    " + instance.GetPropertyValue("Name") + "\n    " + 
                    instance.GetPropertyValue("DeviceID")  + "\n", true, Color.Yellow);
                if (checkBox1.Checked) {
                    FlashButton_Click(sender, e);
                }
            } else if (caterina.Success) {
                Print("*** Caterina device connected:\n    " + instance.GetPropertyValue("Name") + "\n    " + 
                    instance.GetPropertyValue("DeviceID") + "\n", true, Color.Yellow);
                Regex regex = new Regex("(COM[0-9]+)");
                var v = regex.Match(instance.GetPropertyValue("Name").ToString());
                _COM = v.Groups[1].ToString();
                //Print("COM: " +_COM + "\n");
                if (checkBox1.Checked) {
                    FlashButton_Click(sender, e);
                }
            }
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e) {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            //Print("*** Device removed: "+ instance.GetPropertyValue("DeviceID") + "\n", true);
            //Print("*** Device removed:\n", true);
            //foreach (var property in instance.Properties) {
            //    Print("  " + property.Name + ": " + property.Value + "\n", false);
            //}

            // Detects Atmel Vendor ID
            var match = Regex.Match(instance.GetPropertyValue("DeviceID").ToString(), @".*VID_03EB.*");
            // Detects Arduino Vendor ID
            var caterina = Regex.Match(instance.GetPropertyValue("DeviceID").ToString(), @".*VID_2341.*");
            if (match.Success) {
                Print("*** DFU Device disconnected: " + instance.GetPropertyValue("Name") + "\n", true, Color.Yellow);
            } else if (caterina.Success) {
                Print("*** Caterina device disconnected: " + instance.GetPropertyValue("Name") + "\n", true, Color.Yellow);
            }
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

        private void button3_Click(object sender, EventArgs e) {
            if (targetBox.Text == "") {
                Print("*** Please select an MCU\n", true, Color.Red);
            } else if (mcuIsAvailable()) {
                RunDFU("reset");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                Print("*** Auto-flash enabled\n", true);
                button1.Enabled = false;
                button3.Enabled = false;
            } else {
                Print("*** Auto-flash disabled\n", true);
                button1.Enabled = true;
                button3.Enabled = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            if (radioButton1.Checked) {
                comboBox1.Enabled = true;
                comboBox3.Enabled = false;
                comboBox2.Enabled = false;
            } else {
                comboBox1.Enabled = false;
                comboBox3.Enabled = true;
                comboBox2.Enabled = true;
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) {
            Connect();
        }

        private void comboBox3_SelectedIndexChanged(object sender, KeyPressEventArgs e) {
            Connect();
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
            _device.CloseDevice();
            var hiddevices = HidDevices.Enumerate();

            Print("*** Listing (" + hiddevices.Count() + ") PID/VIDs of current HID devices:\n", false, Color.LightSkyBlue);
            foreach (HidDevice hiddevice in hiddevices) {
                if (hiddevice != null) {
                    hiddevice.OpenDevice();
                    Print("  * " + GetManufacturerString(hiddevice) + " - " + GetProductString(hiddevice) + "", false, Color.LightSkyBlue);
                    Print(" / " + hiddevice.Attributes.VendorHexId + ":" + hiddevice.Attributes.ProductHexId + "\n", false, Color.White);
                }
                hiddevice.CloseDevice();
            }
            _device.OpenDevice();
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
