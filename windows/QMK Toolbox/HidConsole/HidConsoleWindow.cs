using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace QMK_Toolbox.HidConsole
{
    public partial class HidConsoleWindow : Form
    {
        private static HidConsoleWindow instance = null;

        #region Window Events
        public HidConsoleWindow()
        {
            InitializeComponent();
        }

        public static HidConsoleWindow GetInstance()
        {
            if (instance == null)
            {
                instance = new HidConsoleWindow();
                instance.FormClosed += delegate { instance = null; };
            }

            return instance;
        }

        private void HidConsoleWindow_Load(object sender, EventArgs e)
        {
            CenterToParent();

            consoleListener.consoleDeviceConnected += ConsoleDeviceConnected;
            consoleListener.consoleDeviceDisconnected += ConsoleDeviceDisconnected;
            consoleListener.consoleReportReceived += ConsoleReportReceived;
            consoleListener.Start();
        }

        private void HidConsoleWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            consoleListener.consoleDeviceConnected -= ConsoleDeviceConnected;
            consoleListener.consoleDeviceDisconnected -= ConsoleDeviceDisconnected;
            consoleListener.consoleReportReceived -= ConsoleReportReceived;
            consoleListener.Dispose();
        }
        #endregion

        #region HID Console
        private readonly HidConsoleListener consoleListener = new();

        private HidConsoleDevice lastReportedDevice;

        private void ConsoleDeviceConnected(HidConsoleDevice device)
        {
            Invoke(new Action(() =>
            {
                lastReportedDevice = device;
                UpdateConsoleList();
                logTextBox.LogHid($"HID console connected: {device}");
            }));
        }

        private void ConsoleDeviceDisconnected(HidConsoleDevice device)
        {
            Invoke(new Action(() =>
            {
                lastReportedDevice = null;
                UpdateConsoleList();
                logTextBox.LogHid($"HID console disconnected: {device}");
            }));
        }

        private void ConsoleReportReceived(HidConsoleDevice device, string report)
        {
            Invoke(new Action(() =>
            {
                int selectedDevice = consoleList.SelectedIndex;
                if (selectedDevice == 0 || consoleListener.Devices[selectedDevice - 1] == device)
                {
                    if (lastReportedDevice != device)
                    {
                        logTextBox.LogHid($"{device.ManufacturerString} {device.ProductString}:");
                        lastReportedDevice = device;
                    }
                    logTextBox.LogHidOutput(report);
                }
            }));
        }

        private void UpdateConsoleList()
        {
            var selected = consoleList.SelectedIndex != -1 ? consoleList.SelectedIndex : 0;
            consoleList.Items.Clear();

            foreach (var device in consoleListener.Devices)
            {
                if (device != null)
                {
                    consoleList.Items.Add(device.ToString());
                }
            }

            if (consoleList.Items.Count > 0)
            {
                consoleList.Items.Insert(0, "(All connected devices)");
                consoleList.SelectedIndex = consoleList.Items.Count > selected ? selected : 0;
            }
        }
        #endregion

        #region Log Box
        private void LogContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            copyToolStripMenuItem.Enabled = logTextBox.SelectedText.Length > 0;
            selectAllToolStripMenuItem.Enabled = logTextBox.Text.Length > 0;
            clearToolStripMenuItem.Enabled = logTextBox.Text.Length > 0;
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logTextBox.Copy();
        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logTextBox.SelectAll();
        }
        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logTextBox.Clear();
        }
        #endregion
    }
}
