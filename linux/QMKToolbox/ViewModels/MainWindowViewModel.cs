
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Metadata;
using Avalonia.Threading;
using QMK_Toolbox.Helpers;
using QMK_Toolbox.Properties;
using QMK_Toolbox.Usb;
using QMK_Toolbox.Usb.Bootloader;
using QMK_Toolbox.Views;
using ReactiveUI;

namespace QMK_Toolbox.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    internal readonly string Prompt = "Click Open or drag to window to select file";
    private string _hexFile;
    private List<string> _mcus;
    private int _selectedMcu=10;
    private bool _isAutoFlash;
    private readonly StringBuilder _log = new();
    private readonly string _arg;
    private readonly IWindow _mainWindow;
    private readonly UsbListener _usbListener;
    private BootloaderDevice _currentBootloader;
    private bool _canOpenFile = true;
    private bool _canFlash;
    private bool _canReset;
    private bool _canClearEeprom;
    private bool _canAutoFlash = true;

    public MainWindowViewModel(string arg, IWindow mainWindow)
    {
        _arg = arg;
        _mainWindow = mainWindow;
        _usbListener = new UsbListener();
        _usbListener.Start();
        HexFile = Prompt;
    }
    
    // ReSharper disable once InconsistentNaming
    private void EnableUI()
    {
        Task.Delay(1).ContinueWith( t => Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                CanOpenFile = true;
                CanFlash = true;
                CanReset = true;
                CanClearEeprom = true;
                CanAutoFlash = true;
            }));
    }
       
    // ReSharper disable once InconsistentNaming
    private void DisableUI()
    {
        Task.Delay(1).ContinueWith( t => Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                CanOpenFile = true;
                CanFlash = false;
                CanReset = false;
                CanClearEeprom = false;
                CanAutoFlash = true;
            }));
    }
    
    public bool CanAutoFlash
    {
        get => _canAutoFlash;
        set => this.RaiseAndSetIfChanged(ref _canAutoFlash, value);
    }
    
    public bool CanOpenFile
    {
        get => _canOpenFile;
        set => this.RaiseAndSetIfChanged(ref _canOpenFile, value);
    }
    
    public bool CanFlash
    {
        get => _canFlash;
        set => this.RaiseAndSetIfChanged(ref _canFlash, value);
    }
    
    public bool CanReset
    {
        get => _canReset;
        set => this.RaiseAndSetIfChanged(ref _canReset, value);
    }

    public bool CanClearEeprom
    {
        get => _canClearEeprom;
        set => this.RaiseAndSetIfChanged(ref _canClearEeprom, value);
    }   
  
    public List<string> Mcus
    {
        get => _mcus;
        set => this.RaiseAndSetIfChanged(ref _mcus, value);
    }
    
    public string Log => _log.ToString();

    private void AddToLog(string text, bool notify=true)
    {
        _log.AppendLine(text);
        if (notify)
            this.RaisePropertyChanged(nameof(Log));
    }
    
    public bool IsAutoFlash
    {
        get => _isAutoFlash;
        set => this.RaiseAndSetIfChanged(ref _isAutoFlash, value);
    }
    
    public int SelectedMcuIndex
    {
        get => _selectedMcu;
        set => this.RaiseAndSetIfChanged(ref _selectedMcu, value);
    }
  
    private AssemblyFileVersionAttribute VersionAttribute =>
        typeof(MainWindowViewModel).Assembly
            .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).FirstOrDefault() 
                as AssemblyFileVersionAttribute;

    public void InitUi()
    {
        Mcus = PopulateMcuListFromResourceFile();
        UpdateHexFileFromCommandLineArguments();
        RestoreLastSavedHexFile();
        ShowInitialLogMessages();
        SubscribeToEvents();

        EmbeddedResourceHelper.ExtractResources(EmbeddedResourceHelper.Resources);
    }

    private void SubscribeToEvents()
    {
        _usbListener.OutputReceived += (_, data, _) => { AddToLog(data); };
        _usbListener.BootloaderDeviceConnected += (device) =>
        {
            EnableUI();
            AddToLog($"Bootloader device connected: {device.ManufacturerString} - {device.ProductString}");
            _currentBootloader = device;
            _currentBootloader.OutputReceived += (_, data, _) => { AddToLog(data); };
            if (_isAutoFlash)
                FlashCommand();
        };
        _usbListener.BootloaderDeviceDisconnected += (device) =>
        {
            DisableUI();
            AddToLog($"Bootloader device disconnected: {device.ManufacturerString} - {device.ProductString}");
            _currentBootloader = null;
        };
    }

    private void RestoreLastSavedHexFile()
    {
        if (Settings.Default.hexFile != null && string.IsNullOrEmpty(_arg))
        {
            HexFile = Settings.Default.hexFile;
        }
    }

    private void UpdateHexFileFromCommandLineArguments()
    {
        if (string.IsNullOrEmpty(_arg)) 
            return;
        var extension = Path.GetExtension(_arg)?.ToLower();
        if (extension is ".hex" or ".bin")
        {
            HexFile = _arg;
        }
        else
        {
            _mainWindow
                .ShowMessage("QMK Toolbox doesn't support this kind of file, only .hex and .bin are supported.");
        }
    }

    private void ShowInitialLogMessages()
    {
        AddToLog($"* QMK Toolbox {VersionAttribute.Version} (https://qmk.fm/toolbox)", false);
        AddToLog("* Supported bootloaders:", false);
        AddToLog(
            "* - ARM DFU (APM32, Kiibohd, STM32, STM32duino) and RISC-V DFU (GD32V) via dfu-util (http://dfu-util.sourceforge.net/)",
            false);
        AddToLog("* - Atmel/LUFA/QMK DFU via dfu-programmer (http://dfu-programmer.github.io/)", false);
        AddToLog("* - Atmel SAM-BA (Massdrop) via Massdrop Loader (https://github.com/massdrop/mdloader)", false);
        AddToLog(
            "* - BootloadHID (Atmel, PS2AVRGB) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)",
            false);
        AddToLog("* - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)", false);
        AddToLog("* - HalfKay (Teensy, Ergodox EZ) via Teensy Loader (https://pjrc.com/teensy/loader_cli.html)", false);
        AddToLog("* - LUFA/QMK HID via hid_bootloader_cli (https://github.com/abcminiuser/lufa)", false);
        AddToLog("* - WB32 DFU via wb32-dfu-updater_cli (https://github.com/WestberryTech/wb32-dfu-updater)", false);
        AddToLog("* - LUFA Mass Storage", false);
        AddToLog("* Supported ISP flashers:", false);
        AddToLog("* - AVRISP (Arduino ISP)", false);
        AddToLog("* - USBasp (AVR ISP)", false);
        AddToLog("* - USBTiny (AVR Pocket)", true);
    }

    public string HexFile
    {
        get => _hexFile;
        set
        {
            this.RaiseAndSetIfChanged(ref _hexFile, value);
            _hexFile = value;
            Settings.Default.hexFile = value;
            Settings.Default.Save();
        }
    }

    public void CloseCommand()
    {
        Task.Delay(1).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(
            () => { _mainWindow.OnClose(); }));
    }

    public void OpenFileCommand()
    {
        Task.Delay(30).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(
            () =>
          { _mainWindow.OnFileOpen(); }));
    }

    public void AutoFlashMenuCommand()
    {
        CanAutoFlash = !CanAutoFlash;
    }

    [DependsOn(nameof(CanAutoFlash))]
    public bool CanAutoFlashMenuCommand()
    {
        return  _canAutoFlash;
    }

    [DependsOn(nameof(CanOpenFile))]
    public bool CanOpenFileCommand(object _)
    {
        return _canOpenFile;
    }
    
    public void FlashCommand()
    {
        DisableUI();
        _currentBootloader.Flash(Mcus[SelectedMcuIndex], HexFile);
        EnableUI();
    }
    
    [DependsOn(nameof(CanFlash))]
    public bool CanFlashCommand(object _)
    {
        return _canFlash;
    }

    // ReSharper disable once InconsistentNaming
    public void ClearEEPromCommand()
    {
        DisableUI();
        _currentBootloader.FlashEeprom(Mcus[SelectedMcuIndex], HexFile);
        EnableUI();
    }
    
    // ReSharper disable once InconsistentNaming
    [DependsOn(nameof(CanClearEeprom))]
    public bool CanClearEEPromCommand(object _)
    {
        return _canClearEeprom;
    }

    public void ExitDfuCommand()
    {
        DisableUI();
        _currentBootloader.Reset(Mcus[SelectedMcuIndex]);
        EnableUI();
    }
    
    [DependsOn(nameof(CanReset))]
    public bool CanExitDfuCommand(object _)
    {
        return _canReset;
    }
    
    private List<string> PopulateMcuListFromResourceFile()
    {
        List<string> mcuList = new();
        var microcontrollers = EmbeddedResourceHelper.GetResourceContent("mcu-list.txt").Split('\n');

        foreach (var microcontroller in microcontrollers)
        {
            if (microcontroller.Length <= 0) 
                continue;
            var parts = microcontroller.Split(':');
            mcuList.Add(parts[0]);
        }

        return mcuList;
    }
}
