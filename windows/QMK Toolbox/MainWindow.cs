﻿//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using QMK_Toolbox.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    using HidLibrary;
    using Newtonsoft.Json;
    using Syroot.Windows.IO;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management;
    using System.Net;
    using System.Text.RegularExpressions;

    public partial class MainWindow : Form
    {
        private const int WmDevicechange = 0x0219;
        private const int DbtDevnodesChanged = 0x0007; //device changed
        private const int DeviceIdOffset = 55;

        private readonly string _filePassedIn = string.Empty;
        private readonly BackgroundWorker _backgroundWorker;
        private readonly Printing _printer;
        private readonly Flashing _flasher;
        private readonly Usb _usb;

        public const int MfSeparator = 0x800;
        public const int WmSyscommand = 0x112;
        public const int MfByposition = 0x400;
        public const int About = 1000;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, int wPosition, int wFlags, int wIdNewItem, string lpNewItem);

        public MainWindow()
        {
            InitializeComponent();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmDevicechange && m.WParam.ToInt32() == DbtDevnodesChanged)
            {
                //Print("*** USB change\n");
            }
            if (m.Msg == NativeMethods.WmShowme)
            {
                ShowMe();
                if (File.Exists(Path.Combine(Path.GetTempPath(), "qmk_toolbox/file_passed_in.txt")))
                {
                    using (var sr = new StreamReader(Path.Combine(Path.GetTempPath(), "qmk_toolbox/file_passed_in.txt")))
                    {
                        SetFilePath(sr.ReadLine());
                    }
                    File.Delete(Path.Combine(Path.GetTempPath(), "qmk_toolbox/file_passed_in.txt"));
                }
            }
            if (m.Msg == WmSyscommand)
            {
                if (m.WParam.ToInt32() == About)
                {
                    var aboutBox = new AboutBox();
                    aboutBox.Show();
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void ShowMe()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            // get our current "TopMost" value (ours will always be false though)
            var top = TopMost;
            // make our form jump to the top of everything
            TopMost = true;
            // set it back to whatever it was
            TopMost = top;
        }

        private List<HidDevice> _devices = new List<HidDevice>();

        public MainWindow(string path)
        {
            InitializeComponent();

            if (path != string.Empty)
            {
                if (Path.GetExtension(path)?.ToLower() == ".qmk" ||
                    Path.GetExtension(path)?.ToLower() == ".hex" ||
                    Path.GetExtension(path)?.ToLower() == ".bin")
                {
                    _filePassedIn = path;
                }
                else
                {
                    MessageBox.Show("QMK Toolbox doesn't support this kind of file", "File Type Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            _printer = new Printing(logTextBox);
            _flasher = new Flashing(_printer);
            _usb = new Usb(_flasher, _printer);
            _flasher.Usb = _usb;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += backgroundWorker1_DoWork;
            _backgroundWorker.WorkerReportsProgress = true;
        }

        private void logTextBox_TextChanged(object sender, EventArgs e)
        {
            // This shouldn't be needed anymore
            // set the current caret position to the end
            // logTextBox.SelectionStart = logTextBox.Text.Length;
            // scroll it automatically
            // logTextBox.ScrollToCaret();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            var arraylist = new ArrayList(filepathBox.Items);
            Settings.Default.hexFileCollection = arraylist;
            Settings.Default.Save();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            var menuHandle = GetSystemMenu(Handle, false);
            InsertMenu(menuHandle, 0, MfByposition | MfSeparator, 0, string.Empty); // <-- Add a menu seperator
            InsertMenu(menuHandle, 0, MfByposition, About, "About");

            _backgroundWorker.RunWorkerAsync();

            foreach (var mcu in _flasher.GetMcuList())
            {
                mcuBox.Items.Add(mcu);
            }
            if (mcuBox.SelectedIndex == -1)
                mcuBox.SelectedIndex = 0;

            if (Settings.Default.hexFileCollection != null)
                filepathBox.Items.AddRange(Settings.Default.hexFileCollection.ToArray());

            logTextBox.Font = new Font(FontFamily.GenericMonospace, 8);

            _printer.Print("QMK Toolbox (http://qmk.fm/toolbox)", MessageType.Info);
            _printer.PrintResponse("Supporting following bootloaders:\n", MessageType.Info);
            _printer.PrintResponse(" - DFU (Atmel, LUFA) via dfu-programmer (http://dfu-programmer.github.io/)\n", MessageType.Info);
            _printer.PrintResponse(" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)\n", MessageType.Info);
            _printer.PrintResponse(" - Halfkay (Teensy, Ergodox EZ) via teensy_loader_cli (https://pjrc.com/teensy/loader_cli.html)\n", MessageType.Info);
            _printer.PrintResponse(" - STM32 (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
            _printer.PrintResponse(" - Kiibohd (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
            _printer.PrintResponse("And the following ISP flasher protocols:\n", MessageType.Info);
            _printer.PrintResponse(" - USBTiny (AVR Pocket)\n", MessageType.Info);
            _printer.PrintResponse(" - AVRISP (Arduino ISP)\n", MessageType.Info);

            var devices = new List<UsbDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                collection = searcher.Get();

            _usb.DetectBootloaderFromCollection(collection);

            UpdateHidDevices();
            UpdateHidList();

            if (_filePassedIn != string.Empty)
                SetFilePath(_filePassedIn);

            LoadKeyboardList();
            LoadKeymapList();
        }

        private void LoadKeyboardList()
        {
            using (var wc = new WebClient())
            {
                var json = wc.DownloadString("http://compile.qmk.fm/v1/keyboards");
                var keyboards = JsonConvert.DeserializeObject<List<string>>(json);
                keyboardBox.Items.Clear();
                foreach (var keyboard in keyboards)
                {
                    keyboardBox.Items.Add(keyboard);
                }
                if (keyboardBox.SelectedIndex == -1)
                    keyboardBox.SelectedIndex = 0;
                keyboardBox.Enabled = true;
            }
        }

        private void LoadKeymapList()
        {
            keymapBox.Items.Clear();
            keymapBox.Items.Add("default");
            keymapBox.SelectedIndex = 0;
            // keymapBox.Enabled = true;
            loadKeymap.Enabled = true;
        }

        private void loadKeymap_Click(object sender, EventArgs e)
        {
            SetFilePath($"qmk:http://qmk.fm/compiled/{keyboardBox.SelectedItem.ToString().Replace("/", "_")}_default.hex");
        }

        private void flashButton_Click(object sender, EventArgs e)
        {
            if (!InvokeRequired)
            {
                flashButton.Enabled = false;
                resetButton.Enabled = false;

                if (_usb.AreDevicesAvailable())
                {
                    var error = 0;
                    if (mcuBox.Text == "")
                    {
                        _printer.Print("Please select a microcontroller", MessageType.Error);
                        error++;
                    }
                    if (filepathBox.Text == "")
                    {
                        _printer.Print("Please select a file", MessageType.Error);
                        error++;
                    }
                    if (error == 0)
                    {
                        _printer.Print("Attempting to flash, please don't remove device", MessageType.Bootloader);
                        _flasher.Flash(mcuBox.Text, filepathBox.Text);
                    }
                }
                else
                {
                    _printer.Print("There are no devices available", MessageType.Error);
                }

                flashButton.Enabled = true;
                resetButton.Enabled = true;
            }
            else
            {
                Invoke(new Action<object, EventArgs>(flashButton_Click), sender, e);
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            if (!InvokeRequired)
            {
                flashButton.Enabled = false;
                resetButton.Enabled = false;

                if (_usb.AreDevicesAvailable())
                {
                    var error = 0;
                    if (mcuBox.Text == "")
                    {
                        _printer.Print("Please select a microcontroller", MessageType.Error);
                        error++;
                    }
                    if (error == 0)
                    {
                        _flasher.Reset(mcuBox.Text);
                    }
                }
                else
                {
                    _printer.Print("There are no devices available", MessageType.Error);
                }

                flashButton.Enabled = true;
                resetButton.Enabled = true;
            }
            else
            {
                Invoke(new Action<object, EventArgs>(resetButton_Click), sender, e);
            }
        }

        private void eepromResetButton_Click(object sender, EventArgs e)
        {
            if (!InvokeRequired)
            {
                eepromResetButton.Enabled = false;

                if (_usb.AreDevicesAvailable())
                {
                    var error = 0;
                    if (mcuBox.Text == "")
                    {
                        _printer.Print("Please select a microcontroller", MessageType.Error);
                        error++;
                    }
                    if (error == 0)
                    {
                        _flasher.EepromReset(mcuBox.Text);
                    }
                }
                else
                {
                    _printer.Print("There are no devices available", MessageType.Error);
                }

                eepromResetButton.Enabled = true;
            }
            else
            {
                Invoke(new Action<object, EventArgs>(eepromResetButton_Click), sender, e);
            }
        }

        private void UpdateHidDevices()
        {
            var devices = new List<HidDevice>(GetListableDevices());

            foreach (var device in devices)
            {
                var deviceExists = false;
                foreach (var existingDevice in _devices)
                {
                    deviceExists |= existingDevice.DevicePath.Equals(device.DevicePath);
                }
                if (device != null && !deviceExists)
                {
                    _devices.Add(device);
                    device.OpenDevice();
                    //device.Inserted += DeviceAttachedHandler;
                    //device.Removed += DeviceRemovedHandler;

                    device.MonitorDeviceEvents = true;

                    _printer.Print($"{GetManufacturerString(device)}: {GetProductString(device)} connected  -- {device.Attributes.VendorId:X4}:{device.Attributes.ProductId:X4}:{device.Attributes.Version:X4} ({GetParentIdPrefix(device)})", MessageType.Hid);

                    device.ReadReport(OnReport);
                    device.CloseDevice();
                }
            }
            foreach (var existingDevice in _devices)
            {
                var deviceExists = false;
                foreach (var device in devices)
                {
                    deviceExists |= existingDevice.DevicePath.Equals(device.DevicePath);
                }
                if (!deviceExists)
                {
                    _printer.Print($"HID device disconnected -- {existingDevice.Attributes.VendorId:X4}:{existingDevice.Attributes.ProductId:X4}:{existingDevice.Attributes.Version:X4} ({GetParentIdPrefix(existingDevice)})", MessageType.Hid);
                }
            }
            _devices = devices;
        }

        private string GetParentIdPrefix(HidDevice d)
        {
            var regex = new Regex("#([&0-9a-fA-F]+)#");
            var vp = regex.Match(d.DevicePath);
            return vp.Groups[1].ToString();
        }

        private IEnumerable<HidDevice> GetListableDevices()
        {
            var devices = HidDevices.Enumerate();
            var listenableDevices = new List<HidDevice>();
            foreach (var device in devices)
            {
                if ((ushort)device.Capabilities.UsagePage == Flashing.UsagePage)
                    listenableDevices.Add(device);
            }
            return listenableDevices;
        }

        private static string GetProductString(IHidDevice d)
        {
            if (d == null)
                return "";
            d.ReadProduct(out var bs);
            return bs.Where(b => b > 0).Aggregate("", (current, b) => current + ((char)b).ToString());
        }

        private string GetManufacturerString(HidDevice d)
        {
            if (d == null)
                return "";
            d.ReadManufacturer(out var bs);
            return bs.Where(b => b > 0).Aggregate("", (current, b) => current + ((char)b).ToString());
        }

        private void DeviceAttachedHandler()
        {
            // not sure if this will be useful
        }

        private void DeviceRemovedHandler()
        {
            // not sure if this will be useful
        }

        private void OnReport(HidReport report)
        {
            var data = report.Data;
            //Print(string.Format("* recv {0} bytes:", data.Length));

            var outputString = string.Empty;
            for (var i = 0; i < data.Length; i++)
            {
                outputString += (char)data[i];
                if (i % 16 != 15 || i >= data.Length) continue;

                _printer.PrintResponse(outputString, MessageType.Hid);
                outputString = string.Empty;
            }

            foreach (var device in _devices)
            {
                device.ReadReport(OnReport);
            }
        }

        private void filepathBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                SetFilePath(filepathBox.Text);
            }
        }

        private void SetFilePath(string filepath)
        {
            var uri = new Uri(filepath);
            if (uri.Scheme == "qmk")
            {
                string url;
                url = filepath.Replace(filepath.Contains("qmk://") ? "qmk://" : "qmk:", "");

                _printer.Print($"Downloading the file: {url}", MessageType.Info);
                var wb = new WebClient();
                if (!Directory.Exists(Path.Combine(Application.LocalUserAppDataPath, "downloads")))
                {
                    Directory.CreateDirectory(Path.Combine(Application.LocalUserAppDataPath, "downloads"));
                }
                wb.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.33 Safari/537.36");
                filepath = Path.Combine(KnownFolders.Downloads.Path, filepath.Substring(filepath.LastIndexOf("/") + 1).Replace(".", "_" + Guid.NewGuid().ToString().Substring(0, 8) + "."));
                wb.DownloadFile(url, filepath);
                _printer.PrintResponse($"File saved to: {filepath}", MessageType.Info);
            }
            if (filepath.EndsWith(".qmk", true, null))
            {
                _printer.Print("Found .qmk file", MessageType.Info);
                var qmkFilepath = $"{Path.GetTempPath()}qmk_toolbox{filepath.Substring(filepath.LastIndexOf("\\"))}\\";
                _printer.PrintResponse($"Extracting to {qmkFilepath}\n", MessageType.Info);
                if (Directory.Exists(qmkFilepath))
                    Directory.Delete(qmkFilepath, true);
                ZipFile.ExtractToDirectory(filepath, qmkFilepath);
                var files = Directory.GetFiles(qmkFilepath);
                var readme = "";
                var info = new Info();
                foreach (var file in files)
                {
                    _printer.PrintResponse($" - {file.Substring(file.LastIndexOf("\\") + 1)}\n", MessageType.Info);
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("firmware.hex", StringComparison.OrdinalIgnoreCase) ||
                        file.Substring(file.LastIndexOf("\\") + 1).Equals("firmware.bin", StringComparison.OrdinalIgnoreCase))
                        SetFilePath(file);
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("readme.md", StringComparison.OrdinalIgnoreCase))
                        readme = File.ReadAllText(file);
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("info.json", StringComparison.OrdinalIgnoreCase))
                        info = JsonConvert.DeserializeObject<Info>(File.ReadAllText(file));
                }
                if (!string.IsNullOrEmpty(info.Keyboard))
                {
                    _printer.Print($"Keymap for keyboard \"{info.Keyboard}\" - {info.VendorId}:{info.ProductId}", MessageType.Info);
                }

                if (string.IsNullOrEmpty(readme)) return;

                _printer.Print("Notes for this keymap:", MessageType.Info);
                _printer.PrintResponse(readme, MessageType.Info);
            }
            else
            {
                if (string.IsNullOrEmpty(filepath)) return;
                filepathBox.Text = filepath;
                if (!filepathBox.Items.Contains(filepath))
                    filepathBox.Items.Add(filepath);
            }
        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SetFilePath(openFileDialog.FileName);
            }
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            if (_usb.DetectBootloader(instance) && (autoflashCheckbox.Checked || flashWhenReadyCheckbox.Checked))
            {
                flashButton_Click(sender, e);

                if (flashWhenReadyCheckbox.Checked)
                {
                    Invoke(new Action(() => flashWhenReadyCheckbox.Checked = false));
                }
            }

            UpdateHidDevices();
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            _usb.DetectBootloader(instance, false);
            UpdateHidDevices();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");

            var insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += DeviceInsertedEvent;
            insertWatcher.Start();

            var removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity'");
            var removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += DeviceRemovedEvent;
            removeWatcher.Start();

            // Do something while waiting for events
            System.Threading.Thread.Sleep(2000000);
        }

        private void autoflashCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (autoflashCheckbox.Checked)
            {
                _printer.Print("Auto-flash enabled", MessageType.Info);
                flashButton.Enabled = false;
                resetButton.Enabled = false;
            }
            else
            {
                _printer.Print("Auto-flash disabled", MessageType.Info);
                flashButton.Enabled = true;
                resetButton.Enabled = true;
            }
        }

        // Set the button's status tip.
        private void btn_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Control obj) toolStripStatusLabel.Text = obj.Tag.ToString();
        }

        // Remove the button's status tip.
        private void btn_MouseLeave(object sender, EventArgs e)
        {
            //if (toolStripStatusLabel.Text.Equals((sender as Control).Tag)) {
            //    toolStripStatusLabel.Text = "";
            //}
        }

        private void UpdateHidList()
        {
            foreach (var device in _devices)
            {
                device.CloseDevice();
            }
            var selected = hidList.SelectedIndex != -1 ? hidList.SelectedIndex : 0;
            hidList.Items.Clear();
            foreach (var device in _devices)
            {
                if (device != null)
                {
                    device.OpenDevice();
                    var deviceString = $"{GetManufacturerString(device)}: {GetProductString(device)} -- {device.Attributes.VendorId:X4}:{device.Attributes.ProductId:X4}:{device.Attributes.Version:X4} ({GetParentIdPrefix(device)})";

                    hidList.Items.Add(deviceString);
                }
                else
                {
                    hidList.Items.Add("Invalid Device");
                }
                device.CloseDevice();
            }

            if (hidList.Items.Count > 0)
            {
                hidList.SelectedIndex = selected;
            }
        }

        private void listHidDevicesButton_Click(object sender, EventArgs e)
        {
            ((Button)sender).Enabled = false;
            foreach (var device in _devices)
            {
                device.CloseDevice();
            }
            _printer.Print($"Listing compatible HID devices: (must have Usage Page 0x{Flashing.UsagePage:X4})", MessageType.Hid);
            foreach (var device in _devices)
            {
                if (device != null)
                {
                    device.OpenDevice();
                    _printer.PrintResponse($"{$" - {GetManufacturerString(device)}: {GetProductString(device)} ".PadRight(0, ' ')}-- {device.Attributes.VendorId:X4}:{device.Attributes.ProductId:X4}:{device.Attributes.Version:X4} ({GetParentIdPrefix(device)})\n", MessageType.Info);
                }

                device?.CloseDevice();
            }
            ((Button)sender).Enabled = true;
        }

        private void ReportWritten(bool success)
        {
            if (!InvokeRequired)
            {
                jumpToBootloaderButton.Enabled = true;
                sayHelloButton.Enabled = true;
                if (success)
                {
                    _printer.PrintResponse("Report sent sucessfully\n", MessageType.Info);
                }
                else
                {
                    _printer.PrintResponse("Report errored\n", MessageType.Error);
                }
            }
            else
            {
                Invoke(new Action<bool>(ReportWritten), success);
            }
        }

        private void jumpToBootloaderButton_Click(object sender, EventArgs e)
        {
            jumpToBootloaderButton.Enabled = false;
            foreach (var device in _devices)
            {
                device.CloseDevice();
            }

            var deviceIndex = hidList.SelectedIndex;
            _devices[deviceIndex].OpenDevice();
            //device.Write(Encoding.ASCII.GetBytes("BOOTLOADER"), 0);
            var data = new byte[2];
            data[0] = 0;
            data[1] = 0xFE;
            var report = new HidReport(2, new HidDeviceData(data, HidDeviceData.ReadStatus.Success));
            _devices[deviceIndex].WriteReport(report, ReportWritten);
            _devices[deviceIndex].CloseDevice();
            _printer.Print("Sending report", MessageType.Hid);
        }

        private void sayHelloButton_Click(object sender, EventArgs e)
        {
            sayHelloButton.Enabled = false;
            foreach (var device in _devices)
            {
                device.CloseDevice();
            }
            var deviceIndex = hidList.SelectedIndex;
            _devices[deviceIndex].OpenDevice();
            //device.Write(Encoding.ASCII.GetBytes("BOOTLOADER"), 0);
            var data = new byte[2];
            data[0] = 0;
            data[1] = 0x01;
            var report = new HidReport(2, new HidDeviceData(data, HidDeviceData.ReadStatus.Success));
            _devices[deviceIndex].WriteReport(report, ReportWritten);
            _devices[deviceIndex].CloseDevice();
            _printer.Print("Sending report", MessageType.Hid);
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            SetFilePath(((string[])e.Data.GetData(DataFormats.FileDrop, false)).First());
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void flashWhenReadyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            flashButton.Enabled = !flashWhenReadyCheckbox.Checked;
            autoflashCheckbox.Enabled = !flashWhenReadyCheckbox.Checked;
            resetButton.Enabled = !flashWhenReadyCheckbox.Checked;
        }
    }

    internal class UsbDeviceInfo
    {
        public UsbDeviceInfo(string deviceId, string pnpDeviceId, string description)
        {
            DeviceId = deviceId;
            PnpDeviceId = pnpDeviceId;
            Description = description;
        }

        public string DeviceId { get; }
        public string PnpDeviceId { get; }
        public string Description { get; }
    }
}