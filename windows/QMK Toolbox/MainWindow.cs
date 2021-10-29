using QMK_Toolbox.Helpers;
using QMK_Toolbox.HidConsole;
using QMK_Toolbox.KeyTester;
using QMK_Toolbox.Properties;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    using Newtonsoft.Json;
    using Syroot.Windows.IO;
    using System.Collections;
    using System.Management;
    using System.Net;

    public partial class MainWindow : Form
    {
        private readonly WindowState windowState = new WindowState();

        private readonly Printing _printer;

        private readonly Flashing _flasher;

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
                if (extension == ".qmk" || extension == ".hex" || extension == ".bin")
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

            _usb.StartListeningForDeviceEvents(DeviceEvent);
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            windowStateBindingSource.DataSource = windowState;
            windowState.PropertyChanged += AutoFlashEnabledChanged;

            if (Settings.Default.hexFileCollection != null)
            {
                filepathBox.Items.AddRange(Settings.Default.hexFileCollection.ToArray());
            }

            mcuBox.SelectedValue = Settings.Default.targetSetting;

            _printer.Print($"QMK Toolbox {Application.ProductVersion} (https://qmk.fm/toolbox)", MessageType.Info);
            _printer.PrintResponse("Supported bootloaders:\n", MessageType.Info);
            _printer.PrintResponse(" - ARM DFU (APM32, Kiibohd, STM32, STM32duino) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
            _printer.PrintResponse(" - Atmel/LUFA/QMK DFU via dfu-programmer (http://dfu-programmer.github.io/)\n", MessageType.Info);
            _printer.PrintResponse(" - Atmel SAM-BA (Massdrop) via Massdrop Loader (https://github.com/massdrop/mdloader)\n", MessageType.Info);
            _printer.PrintResponse(" - BootloadHID (Atmel, PS2AVRGB) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)\n", MessageType.Info);
            _printer.PrintResponse(" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)\n", MessageType.Info);
            _printer.PrintResponse(" - HalfKay (Teensy, Ergodox EZ) via Teensy Loader (https://pjrc.com/teensy/loader_cli.html)\n", MessageType.Info);
            _printer.PrintResponse(" - LUFA Mass Storage\n", MessageType.Info);
            _printer.PrintResponse("Supported ISP flashers:\n", MessageType.Info);
            _printer.PrintResponse(" - AVRISP (Arduino ISP)\n", MessageType.Info);
            _printer.PrintResponse(" - USBasp (AVR ISP)\n", MessageType.Info);
            _printer.PrintResponse(" - USBTiny (AVR Pocket)\n", MessageType.Info);

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE 'USB%'"))
            {
                collection = searcher.Get();
            }

            _usb.DetectBootloaderFromCollection(collection);
            EnableUI();

            consoleListener.consoleDeviceConnected += ConsoleDeviceConnected;
            consoleListener.consoleDeviceDisconnected += ConsoleDeviceDisconnected;
            consoleListener.consoleReportReceived += ConsoleReportReceived;
            consoleListener.Start();

            if (_filePassedIn != string.Empty)
            {
                SetFilePath(_filePassedIn);
            }
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            if (Settings.Default.firstStart)
            {
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
                    if (extension == ".qmk" || extension == ".hex" || extension == ".bin")
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

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
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

            _usb.StopListeningForDeviceEvents();
            consoleListener.Dispose();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }
        #endregion Window Events

        #region HID Console
        private readonly HidConsoleListener consoleListener = new HidConsoleListener();

        private HidConsoleDevice lastReportedDevice;

        private void ConsoleDeviceConnected(HidConsoleDevice device)
        {
            lastReportedDevice = device;
            UpdateConsoleList();
            _printer.Print($"HID console connected: {device}", MessageType.Hid);
        }

        private void ConsoleDeviceDisconnected(HidConsoleDevice device)
        {
            lastReportedDevice = null;
            UpdateConsoleList();
            _printer.Print($"HID console disconnected: {device}", MessageType.Hid);
        }

        private void ConsoleReportReceived(HidConsoleDevice device, string report)
        {
            if (!InvokeRequired)
            {
                int selectedDevice = consoleList.SelectedIndex;
                if (selectedDevice == 0 || consoleListener.Devices[selectedDevice - 1] == device)
                {
                    if (lastReportedDevice != device)
                    {
                        _printer.Print($"{device.ManufacturerString} {device.ProductString}:", MessageType.Hid);
                        lastReportedDevice = device;
                    }
                    _printer.PrintResponse(report, MessageType.Hid);
                }
            }
            else
            {
                Invoke(new Action<HidConsoleDevice, string>(ConsoleReportReceived), device, report);
            }
        }

        private void UpdateConsoleList()
        {
            if (!InvokeRequired)
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
            else
            {
                Invoke(new Action(UpdateConsoleList));
            }
        }
        #endregion HID Console

        #region USB Devices & Bootloaders
        private readonly Usb _usb;

        private void DeviceEvent(object sender, EventArrivedEventArgs e)
        {
            (sender as ManagementEventWatcher)?.Stop();

            if (!(e.NewEvent["TargetInstance"] is ManagementBaseObject instance))
            {
                return;
            }

            var deviceDisconnected = e.NewEvent.ClassPath.ClassName.Equals("__InstanceDeletionEvent");

            if (deviceDisconnected)
            {
                _usb.DetectBootloader(instance, false);
            }
            else if (_usb.DetectBootloader(instance) && windowState.AutoFlashEnabled)
            {
                FlashButton_Click(sender, e);
            }

            (sender as ManagementEventWatcher)?.Start();

            if (!windowState.AutoFlashEnabled)
            {
                Invoke(new Action(EnableUI));
            }
        }
        #endregion

        #region UI Interaction
        private void AutoFlashEnabledChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AutoFlashEnabled")
            {
                if (windowState.AutoFlashEnabled)
                {
                    _printer.Print("Auto-flash enabled", MessageType.Info);
                    DisableUI();
                }
                else
                {
                    _printer.Print("Auto-flash disabled", MessageType.Info);
                    EnableUI();
                }
            }
        }

        private void FlashButton_Click(object sender, EventArgs e)
        {
            if (!InvokeRequired)
            {
                var mcu = (string)mcuBox.SelectedValue;
                var filePath = filepathBox.Text;

                // Keep the form responsive during firmware flashing
                new Thread(() =>
                {
                    if (_usb.AreDevicesAvailable())
                    {
                        if (mcu.Length > 0)
                        {
                            if (filePath.Length > 0)
                            {
                                if (!windowState.AutoFlashEnabled)
                                {
                                    Invoke(new Action(DisableUI));
                                }

                                _printer.Print("Attempting to flash, please don't remove device", MessageType.Bootloader);
                                _flasher.Flash(mcu, filePath);

                                if (!windowState.AutoFlashEnabled)
                                {
                                    Invoke(new Action(EnableUI));
                                }
                            }
                            else
                            {
                                _printer.Print("Please select a file", MessageType.Error);
                            }
                        }
                        else
                        {
                            _printer.Print("Please select a microcontroller", MessageType.Error);
                        }
                    }
                    else
                    {
                        _printer.Print("There are no devices available", MessageType.Error);
                    }
                }).Start();
            }
            else
            {
                Invoke(new Action<object, EventArgs>(FlashButton_Click), sender, e);
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            if (!InvokeRequired)
            {
                if (_usb.AreDevicesAvailable())
                {
                    if (mcuBox.SelectedIndex >= 0)
                    {
                        if (!windowState.AutoFlashEnabled)
                        {
                            Invoke(new Action(DisableUI));
                        }

                        _flasher.Reset((string)mcuBox.SelectedValue);

                        if (!windowState.AutoFlashEnabled)
                        {
                            Invoke(new Action(EnableUI));
                        }
                    }
                    else
                    {
                        _printer.Print("Please select a microcontroller", MessageType.Error);
                    }
                }
                else
                {
                    _printer.Print("There are no devices available", MessageType.Error);
                }
            }
            else
            {
                Invoke(new Action<object, EventArgs>(ResetButton_Click), sender, e);
            }
        }

        private void ClearEepromButton_Click(object sender, EventArgs e)
        {
            if (!InvokeRequired)
            {
                if (_usb.AreDevicesAvailable())
                {
                    if (mcuBox.SelectedIndex >= 0)
                    {
                        if (!windowState.AutoFlashEnabled)
                        {
                            Invoke(new Action(DisableUI));
                        }

                        _flasher.ClearEeprom((string)mcuBox.SelectedValue);

                        if (!windowState.AutoFlashEnabled)
                        {
                            Invoke(new Action(EnableUI));
                        }
                    }
                    else
                    {
                        _printer.Print("Please select a microcontroller", MessageType.Error);
                    }
                }
                else
                {
                    _printer.Print("There are no devices available", MessageType.Error);
                }
            }
            else
            {
                Invoke(new Action<object, EventArgs>(ClearEepromButton_Click), sender, e);
            }
        }

        private void SetHandednessButton_Click(object sender, EventArgs e)
        {

            if (!InvokeRequired)
            {
                if (_usb.AreDevicesAvailable())
                {
                    if (mcuBox.SelectedIndex >= 0)
                    {
                        if (!windowState.AutoFlashEnabled)
                        {
                            Invoke(new Action(DisableUI));
                        }

                        ToolStripMenuItem item = sender as ToolStripMenuItem;
                        _flasher.SetHandedness((string)mcuBox.SelectedValue, (string)item.Tag == "right");

                        if (!windowState.AutoFlashEnabled)
                        {
                            Invoke(new Action(EnableUI));
                        }
                    }
                    else
                    {
                        _printer.Print("Please select a microcontroller", MessageType.Error);
                    }
                }
                else
                {
                    _printer.Print("There are no devices available", MessageType.Error);
                }
            }
            else
            {
                Invoke(new Action<object, EventArgs>(SetHandednessButton_Click), sender, e);
            }
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
            if (!filepath.StartsWith("\\\\wsl$"))
            {
                var uri = new Uri(filepath);
                if (uri.Scheme == "qmk")
                {
                    string url;
                    url = filepath.Replace(filepath.Contains("qmk://") ? "qmk://" : "qmk:", "");
                    filepath = Path.Combine(KnownFolders.Downloads.Path, filepath.Substring(filepath.LastIndexOf("/") + 1).Replace(".", "_" + Guid.NewGuid().ToString().Substring(0, 8) + "."));

                    try
                    {
                        _printer.Print($"Downloading the file: {url}", MessageType.Info);
                        DownloadFirmwareFile(url, filepath);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            // Try .bin extension if hex 404'd
                            url = Path.ChangeExtension(url, "bin");
                            filepath = Path.ChangeExtension(filepath, "bin");
                            _printer.Print($"No .hex file found, trying {url}", MessageType.Info);
                            DownloadFirmwareFile(url, filepath);
                        }
                        catch (Exception)
                        {
                            _printer.PrintResponse("Something went wrong when trying to get the default keymap file.", MessageType.Error);
                            return;
                        }
                    }
                    _printer.PrintResponse($"File saved to: {filepath}", MessageType.Info);
                }
            }
            if (filepath.EndsWith(".qmk", true, null))
            {
                _printer.Print("Found .qmk file", MessageType.Info);
                var qmkFilepath = $"{Path.GetTempPath()}qmk_toolbox{filepath.Substring(filepath.LastIndexOf("\\"))}\\";
                _printer.PrintResponse($"Extracting to {qmkFilepath}\n", MessageType.Info);
                if (Directory.Exists(qmkFilepath))
                {
                    Directory.Delete(qmkFilepath, true);
                }
                ZipFile.ExtractToDirectory(filepath, qmkFilepath);
                var files = Directory.GetFiles(qmkFilepath);
                var readme = "";
                var info = new Info();
                foreach (var file in files)
                {
                    _printer.PrintResponse($" - {file.Substring(file.LastIndexOf("\\") + 1)}\n", MessageType.Info);
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("firmware.hex", StringComparison.OrdinalIgnoreCase) ||
                        file.Substring(file.LastIndexOf("\\") + 1).Equals("firmware.bin", StringComparison.OrdinalIgnoreCase))
                    {
                        SetFilePath(file);
                    }
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("readme.md", StringComparison.OrdinalIgnoreCase))
                    {
                        readme = File.ReadAllText(file);
                    }
                    if (file.Substring(file.LastIndexOf("\\") + 1).Equals("info.json", StringComparison.OrdinalIgnoreCase))
                    {
                        info = JsonConvert.DeserializeObject<Info>(File.ReadAllText(file));
                    }
                }
                if (!string.IsNullOrEmpty(info.Keyboard))
                {
                    _printer.Print($"Keymap for keyboard \"{info.Keyboard}\" - {info.VendorId}:{info.ProductId}", MessageType.Info);
                }

                if (string.IsNullOrEmpty(readme))
                {
                    return;
                }

                _printer.Print("Notes for this keymap:", MessageType.Info);
                _printer.PrintResponse(readme, MessageType.Info);
            }
            else
            {
                if (string.IsNullOrEmpty(filepath))
                {
                    return;
                }
                filepathBox.Text = filepath;
                if (!filepathBox.Items.Contains(filepath))
                {
                    filepathBox.Items.Add(filepath);
                }
            }
        }

        private void DownloadFirmwareFile(string url, string filepath)
        {
            using (var wb = new WebClient())
            {
                wb.Headers.Add("User-Agent", "QMK Toolbox");
                wb.DownloadFile(url, filepath);
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
            windowState.CanFlash = _flasher.CanFlash();
            windowState.CanReset = _flasher.CanReset();
            windowState.CanClearEeprom = _flasher.CanClearEeprom();
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

        private void KeyTesterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KeyTesterWindow.GetInstance().Show();
            KeyTesterWindow.GetInstance().Focus();
        }
        #endregion

        #region Log Box
        private void LogContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            copyToolStripMenuItem.Enabled = (logTextBox.SelectedText.Length > 0);
            selectAllToolStripMenuItem.Enabled = (logTextBox.Text.Length > 0);
            clearToolStripMenuItem.Enabled = (logTextBox.Text.Length > 0);
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
