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

namespace QMK_Flasher {

    using System.Collections.Generic;
    using System.Management;

    public partial class Form1 : Form {

        private System.Diagnostics.Process process;
        private System.Diagnostics.ProcessStartInfo startInfo;
        BackgroundWorker backgroundWorker1;

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

            richTextBox1.VisibleChanged += (sender1, e1) =>
            {
                if (richTextBox1.Visible) {
                    richTextBox1.SelectionStart = richTextBox1.TextLength;
                    richTextBox1.ScrollToCaret();
                }
            };

            string dfuPath = Application.LocalUserAppDataPath + "\\dfu-programmer.exe";
            ExtractResource("QMK_Toolbox.dfu-programmer.exe", dfuPath);

            string mcuListPath = Application.LocalUserAppDataPath + "\\mcu-list.txt";
            ExtractResource("QMK_Toolbox.mcu-list.txt", mcuListPath);

            var lines = File.ReadLines(mcuListPath);
            foreach (var line in lines) {
                string[] tokens = line.Split(',');
                targetBox.Items.Add(tokens[0]);
            }

            process = new System.Diagnostics.Process();
            startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = dfuPath;
            startInfo.CreateNoWindow = true;

            Print("> dfu-programmer --version\n", true);
            startInfo.Arguments = "--version";
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            output += process.StandardError.ReadToEnd();
            process.WaitForExit();
            Print(output, false, Color.LightGray);

            //File.Delete(exePath);

        }

        void ExtractResource(string resource, string path) {
            Stream stream = GetType().Assembly.GetManifestResourceStream(resource);
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            File.WriteAllBytes(path, bytes);
        }

        private void FlashButton_Click(object sender, EventArgs e) {
            string hexFile = hexFileBox.Text;
            if (hexFile == "") {
                Print("*** Please select a file ***\n", true);
            } else {
                if (mcuIsAvailable()) {
                    RunDFU("erase --force");
                    RunDFU("flash " + hexFile);
                    RunDFU("reset");
                }
            }
        }
        private void Print(string str, bool bold = false, Color? color = null) {
            if (!InvokeRequired) {
                if (bold)
                    richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.SelectionColor = color ?? Color.White;
                richTextBox1.AppendText(str);
            } else {
                this.Invoke(new Action<string, bool, Color?>(Print), new object[] { str, bold, color });
            }
        }

        private void RunDFU(string dfuArgs) {
            Print("> dfu-programmer " + targetBox.Text + " " + dfuArgs + "\n", true);
            startInfo.Arguments = targetBox.Text + " " + dfuArgs;
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            output += process.StandardError.ReadToEnd();
            process.WaitForExit();
            Print(output, false, Color.LightGray);
        }

        private bool mcuIsAvailable() {
            startInfo.Arguments = targetBox.Text + " read";
            process.StartInfo = startInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            output += process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (output.Contains("no device present")) {
                Print("*** No device present ***\n", true);
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
            Print("Thing inserted:", true);
            foreach (var property in instance.Properties) {
                Print(property.Name + " = " + property.Value + "\n");
            }
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e) {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            Print("Thing removed:", true);
            foreach (var property in instance.Properties) {
                Print(property.Name + " = " + property.Value + "\n");
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();

            // Do something while waiting for events
            System.Threading.Thread.Sleep(2000000);
        }

    }
}
