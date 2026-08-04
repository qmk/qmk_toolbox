using QmkToolbox.Core.Bootloader;
using QmkToolbox.Core.Bootloader.Impl;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Services;

public class FlashOrchestrator(
    IFlashToolProvider toolProvider,
    ISerialPortService serialPortService,
    IMountPointService mountPointService)
{
    private static readonly bool IsWindows = OperatingSystem.IsWindows();

    private readonly List<BootloaderDevice> _bootloaders = [];

    // Pending volume probes — one per unknown mass-storage device (mostly not bootloaders;
    // e.g. thumb drives), watching for a marker volume to appear — so a disconnect can end
    // its device's probe. Like IsBusy, mutated only on the UI thread: every caller marshals
    // to it, and the probe's awaits must keep the captured context (no ConfigureAwait(false))
    // so the finally that removes the entry resumes there too.
    private readonly List<(IUsbDevice Device, CancellationTokenSource Cancellation)> _volumeProbes = [];

    // Every unknown mass-storage device is polled for a marker volume for as long as it
    // stays connected — desktops like KDE mount removable drives only when the user asks,
    // which can be minutes after the USB arrival. Init-only so tests can shrink the cadence.
    public int VolumeProbeDelayMs { get; init; } = 250;

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
    /// Devices outside the VID/PID map that expose a mass-storage interface are probed for a
    /// volume carrying a bootloader marker file until it mounts or the device is removed, so
    /// completion can lag the arrival by however long the user takes to mount the drive.
    /// </summary>
    public async Task<bool> OnDeviceConnectedAsync(IUsbDevice device, bool showAllDevices)
    {
        BootloaderDevice? bd = BootloaderFactory.CreateDevice(device, toolProvider, serialPortService, mountPointService);
        if (bd == null)
        {
            // Report unknown devices right away: the volume probe below can run for the
            // device's whole lifetime, and nothing user-visible may wait on it.
            if (showAllDevices)
                Emit($"USB device connected{WindowsDriverSuffix(device.Driver)}: {device}", MessageType.Usb);
            DiagnosticTrace?.Invoke(
                $"[ORCH+] {DeviceTrace.VidPidRev(device)} -> not a bootloader");
            bd = await TryCreateMassStorageDeviceAsync(device);
            if (bd == null)
                return false;
        }

        bd.OutputReceived += OnFlashOutput;
        _bootloaders.Add(bd);
        DiagnosticTrace?.Invoke(
            $"[ORCH+] {DeviceTrace.VidPidRev(device)} path:{DeviceTrace.Path(device.DevicePath)}" +
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

    /// <summary>
    /// Polls a mass-storage device outside the VID/PID map for a volume carrying one of the
    /// probeable families' marker files (<see cref="MassStorageBootloader.Probeable"/>) until
    /// one appears or the device is removed. Marker-probed bootloaders carry per-board
    /// VID/PIDs, so the marker is the only general way to recognise them — and the volume
    /// appears only when the OS (or the user, on desktops that don't automount) mounts the
    /// drive.
    /// </summary>
    private async Task<BootloaderDevice?> TryCreateMassStorageDeviceAsync(IUsbDevice device)
    {
        if (!device.IsMassStorage)
            return null;
        string markers = string.Join(", ", MassStorageBootloader.Probeable.Select(f => f.MarkerFile));
        DiagnosticTrace?.Invoke(
            $"[ORCH+] {DeviceTrace.VidPidRev(device)} -> mass storage, probing for {markers} until removal");
        var cancellation = new CancellationTokenSource();
        _volumeProbes.Add((device, cancellation));
        try
        {
            while (true)
            {
                foreach (MassStorageBootloader family in MassStorageBootloader.Probeable)
                {
                    string? mount = mountPointService.FindMountPoint(device, family.MarkerFile);
                    if (mount == null || IsMountClaimed(mount))
                        continue;
                    string? boardId = family.BoardIdReader?.Invoke(Path.Combine(mount, family.MarkerFile));
                    DiagnosticTrace?.Invoke(
                        $"[ORCH+] {DeviceTrace.VidPidRev(device)} -> {family.Name} volume at \"{mount}\"" +
                        (boardId == null ? "" : $" (Board-ID: {boardId})"));
                    return BootloaderFactory.CreateMassStorageDevice(family.Type, device, toolProvider, mountPointService, boardId, mount);
                }
                await Task.Delay(VolumeProbeDelayMs, cancellation.Token);
            }
        }
        catch (OperationCanceledException)
        {
            DiagnosticTrace?.Invoke(
                $"[ORCH+] {DeviceTrace.VidPidRev(device)} -> volume probe ended, device removed");
            return null;
        }
        finally
        {
            _volumeProbes.RemoveAll(p => p.Cancellation == cancellation);
            cancellation.Dispose();
        }
    }

    // A marker volume already backing a registered mass-storage device can't be claimed
    // again — with several unknown devices probing at once (e.g. a thumb drive alongside a
    // keyboard), only one may register per volume.
    private bool IsMountClaimed(string mount) =>
        _bootloaders.Any(b => b is MassStorageDevice ms && ms.MountPoint == mount);

    private void CancelVolumeProbe(IUsbDevice device)
    {
        // Path first, VID/PID fallback — mirrors the bootloader matching in OnDeviceDisconnected.
        (IUsbDevice Device, CancellationTokenSource Cancellation) probe = default;
        if (!string.IsNullOrEmpty(device.DevicePath))
            probe = _volumeProbes.FirstOrDefault(p => p.Device.DevicePath == device.DevicePath);
        if (probe.Cancellation == null)
            probe = _volumeProbes.FirstOrDefault(p => p.Device.VendorId == device.VendorId && p.Device.ProductId == device.ProductId);
        probe.Cancellation?.Cancel();
    }

    public void OnDeviceDisconnected(IUsbDevice device, bool showAllDevices)
    {
        CancelVolumeProbe(device);

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
            string prefix = $"[ORCH-] {DeviceTrace.VidPid(device)} path:{DeviceTrace.Path(device.DevicePath)}";
            if (bd != null)
            {
                DiagnosticTrace(
                    $"{prefix} -> matched by {(matchedByPath ? "path" : "VID/PID")}  (bootloaders:{_bootloaders.Count})");
            }
            else if (_bootloaders.Count > 0)
            {
                DiagnosticTrace(
                    $"{prefix} -> *** no match  (bootloaders:{_bootloaders.Count} – possible phantom entry)");
            }
            else
            {
                DiagnosticTrace($"{prefix} -> not a tracked bootloader  (bootloaders:0)");
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
    public async Task<bool> RunExclusiveAsync(Func<Task> operation)
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
