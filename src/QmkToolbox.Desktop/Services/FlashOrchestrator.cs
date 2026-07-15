using QmkToolbox.Core.Bootloader;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

public class FlashOrchestrator(
    IFlashToolProvider toolProvider,
    ISerialPortService serialPortService,
    IMountPointService mountPointService)
{
    private static readonly bool IsWindows = OperatingSystem.IsWindows();

    private readonly List<BootloaderDevice> _bootloaders = [];

    public event Action<string, MessageType>? OutputReceived;
    public event Action? StateChanged;

    public Action<string>? DiagnosticTrace { get; set; }

    public bool HasBootloaders => _bootloaders.Count > 0;
    public bool HasResettable => _bootloaders.Any(b => b.IsResettable);
    public bool HasEepromFlashable => _bootloaders.Any(b => b.IsEepromFlashable);
    public int BootloaderCount => _bootloaders.Count;

    /// <summary> True while a flash / reset / EEPROM / resource-maintenance operation is running. </summary>
    public bool IsBusy { get; private set; }

    /// <summary>
    /// Registers a connected USB device as a bootloader if recognised.
    /// Returns <see langword="true"/> if a bootloader device was added (caller may trigger auto-flash).
    /// </summary>
    public bool OnDeviceConnected(IUsbDevice device, bool showAllDevices)
    {
        BootloaderDevice? bd = BootloaderFactory.CreateDevice(device, toolProvider, serialPortService, mountPointService);
        if (bd != null)
        {
            bd.OutputReceived += OnFlashOutput;
            _bootloaders.Add(bd);
            DiagnosticTrace?.Invoke(
                $"[ORCH+] VID:{device.VendorId:X4} PID:{device.ProductId:X4} " +
                $"path:{(string.IsNullOrEmpty(device.DevicePath) ? "(empty)" : $"\"{device.DevicePath}\"")}" +
                $" -> {bd.Name}  (bootloaders:{_bootloaders.Count})");
            StateChanged?.Invoke();
            // Await port resolution (instant for most devices; up to ~2.5 s for serial-port
            // bootloaders) so the connected message includes the resolved port in ToString().
            _ = bd.WhenReadyAsync().ContinueWith(_ =>
            {
                Emit($"{bd.Name} device connected{WindowsDriverSuffix(bd.Driver)}: {bd}", MessageType.Bootloader);
                if (IsWindows && !string.IsNullOrEmpty(bd.Driver) && !string.IsNullOrEmpty(bd.PreferredDriver) && bd.PreferredDriver != bd.Driver)
                    Emit($"{bd.Name} device has {bd.Driver} driver assigned but should be {bd.PreferredDriver}. Flashing may not succeed.", MessageType.Error);
            }, TaskScheduler.Default);
            return true;
        }
        else if (showAllDevices)
        {
            Emit($"USB device connected{WindowsDriverSuffix(device.Driver)}: {device}", MessageType.Usb);
        }
        DiagnosticTrace?.Invoke(
            $"[ORCH+] VID:{device.VendorId:X4} PID:{device.ProductId:X4} -> not a bootloader");
        return false;
    }

    public void OnDeviceDisconnected(IUsbDevice device, bool showAllDevices)
    {
        bool matchedByPath = false;
        BootloaderDevice? bd = null;
        if (!string.IsNullOrEmpty(device.DevicePath))
        {
            bd = _bootloaders.FirstOrDefault(b => b.DevicePath == device.DevicePath);
            if (bd != null)
                matchedByPath = true;
        }
        bd ??= _bootloaders.FirstOrDefault(b => b.VendorId == device.VendorId && b.ProductId == device.ProductId);

        if (bd != null)
        {
            bd.OutputReceived -= OnFlashOutput;
            _bootloaders.Remove(bd);
            Emit($"{bd.Name} device disconnected{WindowsDriverSuffix(bd.Driver)}: {bd}", MessageType.Bootloader);
        }
        else if (showAllDevices)
        {
            Emit($"USB device disconnected{WindowsDriverSuffix(device.Driver)}: {device}", MessageType.Usb);
        }

        if (DiagnosticTrace != null)
        {
            string pathStr = string.IsNullOrEmpty(device.DevicePath) ? "(empty)" : $"\"{device.DevicePath}\"";
            if (bd != null)
            {
                DiagnosticTrace(
                    $"[ORCH-] VID:{device.VendorId:X4} PID:{device.ProductId:X4} path:{pathStr}" +
                    $" -> matched by {(matchedByPath ? "path" : "VID/PID")}  (bootloaders:{_bootloaders.Count})");
            }
            else if (_bootloaders.Count > 0)
            {
                DiagnosticTrace(
                    $"[ORCH-] VID:{device.VendorId:X4} PID:{device.ProductId:X4} path:{pathStr}" +
                    $" -> *** no match  (bootloaders:{_bootloaders.Count} – possible phantom entry)");
            }
            else
            {
                DiagnosticTrace(
                    $"[ORCH-] VID:{device.VendorId:X4} PID:{device.ProductId:X4} path:{pathStr}" +
                    $" -> not a tracked bootloader  (bootloaders:0)");
            }
        }

        StateChanged?.Invoke();
    }

    /// <summary>
    /// Runs <paramref name="operation"/> as the single in-flight flash / reset / EEPROM /
    /// resource-maintenance operation, returning <see langword="true"/> if it ran or
    /// <see langword="false"/> without running when one is already in progress.
    /// <see cref="IsBusy"/> is UI-thread-affine — every caller marshals to the UI thread, so the
    /// check-then-set before the first await is atomic and needs no lock.
    /// </summary>
    internal async Task<bool> RunExclusiveAsync(Func<Task> operation)
    {
        if (IsBusy)
            return false;

        IsBusy = true;
        StateChanged?.Invoke();
        try
        {
            await operation();
            return true;
        }
        finally
        {
            IsBusy = false;
            StateChanged?.Invoke();
        }
    }

    public Task<bool> FlashAllAsync(string mcu, string firmwarePath) =>
        RunExclusiveAsync(() => FlashAllCore(mcu, firmwarePath));

    public Task<bool> ResetAllAsync(string mcu) =>
        RunExclusiveAsync(() => ResetAllCore(mcu));

    public Task<bool> FlashEepromAsync(string mcu, string fileName, string startMessage, string completeMessage) =>
        RunExclusiveAsync(() => FlashEepromCore(mcu, fileName, startMessage, completeMessage));

    private async Task FlashAllCore(string mcu, string firmwarePath)
    {
        DiagnosticTrace?.Invoke($"[FLASH] FlashAllAsync start  (bootloaders:{_bootloaders.Count})");
        try
        {
            foreach (BootloaderDevice b in _bootloaders.ToList())
            {
                try
                {
                    Emit("Attempting to flash, please don't remove device", MessageType.Bootloader);
                    await b.FlashAsync(mcu, firmwarePath);
                    Emit("Flash complete", MessageType.Bootloader);
                }
                catch (Exception ex) when (ex is UnsupportedFileFormatException or ComPortNotFoundException)
                {
                    Emit(ex.Message, MessageType.Error);
                }
            }
        }
        finally
        {
            DiagnosticTrace?.Invoke($"[FLASH] FlashAllAsync finally  (bootloaders:{_bootloaders.Count})");
        }
    }

    private async Task ResetAllCore(string mcu)
    {
        DiagnosticTrace?.Invoke($"[RESET] ResetAllAsync start  (bootloaders:{_bootloaders.Count})");
        foreach (BootloaderDevice b in _bootloaders.Where(b => b.IsResettable).ToList())
        {
            try
            {
                await b.ResetAsync(mcu);
            }
            catch (ComPortNotFoundException ex)
            {
                Emit(ex.Message, MessageType.Error);
            }
        }
    }

    private async Task FlashEepromCore(string mcu, string fileName, string startMessage, string completeMessage)
    {
        foreach (BootloaderDevice b in _bootloaders.Where(b => b.IsEepromFlashable).ToList())
        {
            try
            {
                Emit(startMessage, MessageType.Bootloader);
                await b.FlashEepromAsync(mcu, fileName);
                Emit(completeMessage, MessageType.Bootloader);
            }
            catch (ComPortNotFoundException ex)
            {
                Emit(ex.Message, MessageType.Error);
            }
        }
    }

    private void OnFlashOutput(BootloaderDevice device, string data, MessageType type) => Emit(data, type);

    private void Emit(string message, MessageType type) => OutputReceived?.Invoke(message, type);

    // Driver info is Windows-only; on other platforms the field is always empty.
    // Matches upstream behaviour: show the driver name, or NO DRIVER if none is assigned.
    private static string WindowsDriverSuffix(string driver) =>
        IsWindows ? $" ({(string.IsNullOrEmpty(driver) ? "NO DRIVER" : driver)})" : "";
}
