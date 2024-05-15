using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace QMK_Toolbox.Hid
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

            hidListener.hidDeviceConnected += HidDeviceConnected;
            hidListener.hidDeviceDisconnected += HidDeviceDisconnected;
            hidListener.consoleReportReceived += ConsoleReportReceived;
            hidListener.Start();
        }

        private void HidConsoleWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            hidListener.hidDeviceConnected -= HidDeviceConnected;
            hidListener.hidDeviceDisconnected -= HidDeviceDisconnected;
            hidListener.consoleReportReceived -= ConsoleReportReceived;
            hidListener.Dispose();
        }
        #endregion

        #region HID Console
        private readonly HidListener hidListener = new();

        private HidConsoleDevice lastReportedDevice;

        private void HidDeviceConnected(BaseHidDevice device)
        {
            Invoke(new Action(() =>
            {
                if (device is HidConsoleDevice)
                {
                    lastReportedDevice = device as HidConsoleDevice;
                    UpdateConsoleList();
                    logTextBox.LogHid($"HID console connected: {device}");
                }
                else
                {
                    logTextBox.LogHid($"Raw HID device connected: {device}");
                }
            }));
        }

        private void HidDeviceDisconnected(BaseHidDevice device)
        {
            Invoke(new Action(() =>
            {
                if (device is HidConsoleDevice)
                {
                    lastReportedDevice = null;
                    UpdateConsoleList();
                    logTextBox.LogHid($"HID console disconnected: {device}");
                }
                else
                {
                    logTextBox.LogHid($"Raw HID device disconnected: {device}");
                }
            }));
        }

        private void ConsoleReportReceived(HidConsoleDevice device, string report)
        {
            Invoke(new Action(() =>
            {
                int selectedDevice = consoleList.SelectedIndex;
                var consoleDevices = hidListener.Devices.Where(d => d is HidConsoleDevice).ToList();
                if (selectedDevice == 0 || consoleDevices[selectedDevice - 1] == device)
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

            foreach (var device in hidListener.Devices.Where(d => d is HidConsoleDevice))
            {
                consoleList.Items.Add(device.ToString());
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
