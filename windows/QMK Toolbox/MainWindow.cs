using QMK_Toolbox.Helpers;
using QMK_Toolbox.Hid;
using QMK_Toolbox.KeyTester;
using QMK_Toolbox.Properties;
using QMK_Toolbox.Usb;
using QMK_Toolbox.Usb.Bootloader;
using Syroot.Windows.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public partial class MainWindow : Form
    {
        private readonly WindowState windowState = new();

        private readonly string _filePassedIn = string.Empty;

        #region Window Events
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(string path) : this()
        {
            if (path != string.Empty)
            {
                var extension = Path.GetExtension(path)?.ToLower();
                if (extension == ".hex" || extension == ".bin")
                {
                    _filePassedIn = path;
                }
                else
                {
                    MessageBox.Show("QMK Toolbox doesn't support this kind of file", "File Type Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            windowStateBindingSource.DataSource = windowState;
            windowState.PropertyChanged += AutoFlashEnabledChanged;
            windowState.PropertyChanged += ShowAllDevicesEnabledChanged;
            windowState.ShowAllDevices = Settings.Default.showAllDevices;

            if (Settings.Default.hexFileCollection != null)
            {
                filepathBox.Items.AddRange(Settings.Default.hexFileCollection.ToArray());
            }

            mcuBox.SelectedValue = Settings.Default.targetSetting;

            logTextBox.LogInfo($"QMK Toolbox {Application.ProductVersion} (https://qmk.fm/toolbox)");
            logTextBox.LogInfo("Supported bootloaders:");
            logTextBox.LogInfo(" - ARM DFU (APM32, Kiibohd, STM32, STM32duino) and RISC-V DFU (GD32V) via dfu-util (http://dfu-util.sourceforge.net/)");
            logTextBox.LogInfo(" - Atmel/LUFA/QMK DFU via dfu-programmer (http://dfu-programmer.github.io/)");
            logTextBox.LogInfo(" - Atmel SAM-BA (Massdrop) via Massdrop Loader (https://github.com/massdrop/mdloader)");
            logTextBox.LogInfo(" - BootloadHID (Atmel, PS2AVRGB) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)");
            logTextBox.LogInfo(" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)");
            logTextBox.LogInfo(" - HalfKay (Teensy, Ergodox EZ) via Teensy Loader (https://pjrc.com/teensy/loader_cli.html)");
            logTextBox.LogInfo(" - LUFA/QMK HID via hid_bootloader_cli (https://github.com/abcminiuser/lufa)");
            logTextBox.LogInfo(" - WB32 DFU via wb32-dfu-updater_cli (https://github.com/WestberryTech/wb32-dfu-updater)");
            logTextBox.LogInfo(" - LUFA Mass Storage");
            logTextBox.LogInfo("Supported ISP flashers:");
            logTextBox.LogInfo(" - AVRISP (Arduino ISP)");
            logTextBox.LogInfo(" - USBasp (AVR ISP)");
            logTextBox.LogInfo(" - USBTiny (AVR Pocket)");

            usbListener.usbDeviceConnected += UsbDeviceConnected;
            usbListener.usbDeviceDisconnected += UsbDeviceDisconnected;
            usbListener.bootloaderDeviceConnected += BootloaderDeviceConnected;
            usbListener.bootloaderDeviceDisconnected += BootloaderDeviceDisconnected;
            usbListener.outputReceived += BootloaderCommandOutputReceived;

            try
            {
                usbListener.Start();
            }
            catch (COMException ex)
            {
                logTextBox.LogError("USB device enumeration failed.");
                logTextBox.LogError($"{ex}");
            }

            if (_filePassedIn != string.Empty)
            {
                SetFilePath(_filePassedIn);
            }

            EnableUI();
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            if (Settings.Default.firstStart)
            {
                EmbeddedResourceHelper.InitResourceFolder();
                Settings.Default.Upgrade();
            }

            if (Settings.Default.firstStart)
            {
                DriverInstaller.DisplayPrompt();
                Settings.Default.firstStart = false;
                Settings.Default.Save();
            }
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    var extension = Path.GetExtension(files.First())?.ToLower();
                    if (extension == ".hex" || extension == ".bin")
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                }
            }
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            SetFilePath(((string[])e.Data.GetData(DataFormats.FileDrop, false)).First());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WmShowme)
            {
                ShowMe();
                if (File.Exists(Path.Combine(Path.GetTempPath(), "qmk_toolbox_file.txt")))
                {
                    using (var sr = new StreamReader(Path.Combine(Path.GetTempPath(), "qmk_toolbox_file.txt")))
                    {
                        SetFilePath(sr.ReadLine());
                    }
                    File.Delete(Path.Combine(Path.GetTempPath(), "qmk_toolbox_file.txt"));
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
            Activate();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            var arraylist = new ArrayList(filepathBox.Items);
            Settings.Default.hexFileCollection = arraylist;
            Settings.Default.targetSetting = (string)mcuBox.SelectedValue;
            Settings.Default.Save();

            usbListener.Dispose();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }
        #endregion Window Events

        #region USB Devices & Bootloaders
        private readonly UsbListener usbListener = new();

        private void BootloaderDeviceConnected(BootloaderDevice device)
        {
            Invoke(new Action(() =>
            {
                logTextBox.LogBootloader($"{device.Name} device connected ({device.Driver}): {device}");

                if (device.PreferredDriver != device.Driver)
                {
                    logTextBox.LogError($"{device.Name} device has {device.Driver} driver assigned but should be {device.PreferredDriver}. Flashing may not succeed.");
                }

                if (windowState.AutoFlashEnabled)
                {
                    FlashAllAsync();
                }
                else
                {
                    EnableUI();
                }
            }));
        }

        private void BootloaderDeviceDisconnected(BootloaderDevice device)
        {
            Invoke(new Action(() =>
            {
                logTextBox.LogBootloader($"{device.Name} device disconnected ({device.Driver}): {device}");

                if (!windowState.AutoFlashEnabled)
                {
                    EnableUI();
                }
            }));
        }

        private void BootloaderCommandOutputReceived(BootloaderDevice device, string data, MessageType type)
        {
            Invoke(new Action(() =>
            {
                logTextBox.Log(data, type);
            }));
        }

        private void UsbDeviceConnected(UsbDevice device)
        {
            Invoke(new Action(() =>
            {
                if (windowState.ShowAllDevices)
                {
                    logTextBox.LogUsb($"USB device connected ({device.Driver}): {device}");
                }
            }));
        }

        private void UsbDeviceDisconnected(UsbDevice device)
        {
            Invoke(new Action(() =>
            {
                if (windowState.ShowAllDevices)
                {
                    logTextBox.LogUsb($"USB device disconnected ({device.Driver}): {device}");
                }
            }));
        }
        #endregion

        #region UI Interaction
        private void AutoFlashEnabledChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AutoFlashEnabled")
            {
                if (windowState.AutoFlashEnabled)
                {
                    logTextBox.LogInfo("Auto-flash enabled");
                    DisableUI();
                }
                else
                {
                    logTextBox.LogInfo("Auto-flash disabled");
                    EnableUI();
                }
            }
        }

        private void ShowAllDevicesEnabledChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowAllDevices")
            {
                Settings.Default.showAllDevices = windowState.ShowAllDevices;
            }
        }

        private async void FlashAllAsync()
        {
            string selectedMcu = (string)mcuBox.SelectedValue;
            string filePath = filepathBox.Text;

            if (filePath.Length == 0)
            {
                logTextBox.LogError("Please select a file");
                return;
            }

            if (!File.Exists(filePath))
            {
                logTextBox.LogError("File does not exist!");
                return;
            }

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(DisableUI));
            }

            foreach (BootloaderDevice b in FindBootloaders())
            {
                logTextBox.LogBootloader("Attempting to flash, please don't remove device");
                await b.Flash(selectedMcu, filePath);
                logTextBox.LogBootloader("Flash complete");
            }

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(EnableUI));
            }
        }

        private async void ResetAllAsync()
        {
            string selectedMcu = (string)mcuBox.SelectedValue;

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(DisableUI));
            }

            foreach (BootloaderDevice b in FindBootloaders())
            {
                if (b.IsResettable)
                {
                    await b.Reset(selectedMcu);
                }
            }

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(EnableUI));
            }
        }

        private async void ClearEepromAllAsync()
        {
            string selectedMcu = (string)mcuBox.SelectedValue;

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(DisableUI));
            }

            foreach (BootloaderDevice b in FindBootloaders())
            {
                if (b.IsEepromFlashable)
                {
                    logTextBox.LogBootloader("Attempting to clear EEPROM, please don't remove device");
                    await b.FlashEeprom(selectedMcu, "reset.eep");
                    logTextBox.LogBootloader("EEPROM clear complete");
                }
            }

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(EnableUI));
            }
        }

        private async void SetHandednessAllAsync(bool left)
        {
            string selectedMcu = (string)mcuBox.SelectedValue;
            string file = left ? "reset_left.eep" : "reset_right.eep";

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(DisableUI));
            }

            foreach (BootloaderDevice b in FindBootloaders())
            {
                if (b.IsEepromFlashable)
                {
                    logTextBox.LogBootloader("Attempting to set handedness, please don't remove device");
                    await b.FlashEeprom(selectedMcu, file);
                    logTextBox.LogBootloader("EEPROM write complete");
                }
            }

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(EnableUI));
            }
        }

        private void FlashButton_Click(object sender, EventArgs e)
        {
            FlashAllAsync();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            ResetAllAsync();
        }

        private void ClearEepromButton_Click(object sender, EventArgs e)
        {
            ClearEepromAllAsync();
        }

        private void SetHandednessButton_Click(object sender, EventArgs e)
        {
            SetHandednessAllAsync(sender == eepromLeftToolStripMenuItem);
        }

        private List<BootloaderDevice> FindBootloaders()
        {
            return usbListener.Devices.Where(d => d is BootloaderDevice).Select(b => b as BootloaderDevice).ToList();
        }

        private void OpenFileButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SetFilePath(openFileDialog.FileName);
            }
        }

        private void FilepathBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetFilePath(filepathBox.Text);
                e.Handled = true;
            }
        }

        private void SetFilePath(string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                if (filepath.StartsWith("qmk:"))
                {
                    string unwrappedUrl = filepath[(filepath.StartsWith("qmk://") ? 6 : 4)..];
                    DownloadFile(unwrappedUrl);
                }
                else
                {
                    LoadLocalFile(filepath);
                }
            }
        }

        private void LoadLocalFile(string path)
        {
            if (!filepathBox.Items.Contains(path))
            {
                filepathBox.Items.Add(path);
            }
            filepathBox.SelectedItem = path;
        }

        private async void DownloadFile(string url)
        {
            logTextBox.LogInfo($"Downloading the file: {url}");

            try
            {
                string destFile = Path.Combine(KnownFolders.Downloads.Path, url[(url.LastIndexOf("/") + 1)..]);

                var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("QMK Toolbox");

                var response = await client.GetAsync(url);
                using (var fs = new FileStream(destFile, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                    logTextBox.LogInfo($"File saved to: {destFile}");
                }

                LoadLocalFile(destFile);
            }
            catch (Exception e)
            {
                logTextBox.LogError($"Could not download file: {e.Message}");
            }
        }

        private void DisableUI()
        {
            windowState.CanFlash = false;
            windowState.CanReset = false;
            windowState.CanClearEeprom = false;
        }

        private void EnableUI()
        {
            List<BootloaderDevice> bootloaders = FindBootloaders();
            windowState.CanFlash = bootloaders.Any();
            windowState.CanReset = bootloaders.Any(b => b.IsResettable);
            windowState.CanClearEeprom = bootloaders.Any(b => b.IsEepromFlashable);
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void InstallDriversMenuItem_Click(object sender, EventArgs e)
        {
            DriverInstaller.DisplayPrompt();
        }

        private void ClearResourcesMenuItem_Click(object sender, EventArgs e)
        {
            EmbeddedResourceHelper.InitResourceFolder();
        }

        private void KeyTesterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeyTesterWindow.GetInstance().Show(this);
            KeyTesterWindow.GetInstance().Focus();
        }

        private void HidConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HidConsoleWindow.GetInstance().Show(this);
            HidConsoleWindow.GetInstance().Focus();
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
