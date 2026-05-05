using System.Runtime.InteropServices;
using QmkToolbox.Core.Bootloader;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

public class FlashOrchestrator(
    IFlashToolProvider toolProvider,
    ISerialPortService serialPortService,
    IMountPointService mountPointService)
{
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    private readonly List<BootloaderDevice> _bootloaders = [];

    public event Action<string, MessageType>? OutputReceived;
    public event Action? StateChanged;

    public bool HasBootloaders => _bootloaders.Count > 0;
    public bool HasResettable => _bootloaders.Any(b => b.IsResettable);
    public bool HasEepromFlashable => _bootloaders.Any(b => b.IsEepromFlashable);

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
        return false;
    }

    public void OnDeviceDisconnected(IUsbDevice device, bool showAllDevices)
    {
        BootloaderDevice? bd = (!string.IsNullOrEmpty(device.DevicePath)
            ? _bootloaders.FirstOrDefault(b => b.DevicePath == device.DevicePath)
            : null)
            ?? _bootloaders.FirstOrDefault(b => b.VendorId == device.VendorId && b.ProductId == device.ProductId);
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
        StateChanged?.Invoke();
    }

    public async Task FlashAllAsync(string mcu, string firmwarePath)
    {
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
            StateChanged?.Invoke();
        }
    }

    public async Task ResetAllAsync(string mcu)
    {
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

    public async Task FlashEepromAsync(string mcu, string fileName, string startMessage, string completeMessage)
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
