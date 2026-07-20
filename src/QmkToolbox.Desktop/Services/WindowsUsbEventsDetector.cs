#if WINDOWS
using System.Diagnostics;
using System.Runtime.InteropServices;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

// Usb.Events is not used on Windows. Its Windows backend subscribes to Win32_PnPEntity via a WMI
// event query using "WITHIN 2", meaning events are polled every 2 seconds. Beyond the polling delay,
// WMI infrastructure initialisation on the first query in a fresh .NET 10 process takes ~7 seconds,
// making device detection feel broken on first plug-in. Usb.Events also drives WMI setup through
// reflection and COM interop, which is incompatible with PublishTrimmed — it required rooting
// Usb.Events and System.Management to suppress trim warnings, and BuiltInComInteropSupport=true
// to satisfy the COM layer. RegisterDeviceNotification is a kernel-mode callback — the driver stack
// delivers arrival and removal events to the window procedure synchronously with no polling,
// no cold-start overhead, and no trim incompatibilities.

/// <summary>
/// Windows USB detector using RegisterDeviceNotification via a message-only window.
/// Avoids WMI entirely — events arrive synchronously via WndProc with no polling latency.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public sealed class WindowsUsbEventsDetector : IUsbEventsDetector
{
    private readonly List<IUsbDevice> _devices = [];
    private readonly Lock _devicesLock = new();

    public Action<string>? DiagnosticTrace { get; set; }
    private Thread? _messageThread;
    private volatile IntPtr _hwnd = IntPtr.Zero;
    private IntPtr _notifyHandle = IntPtr.Zero;
    private uint _messageThreadId;
    private readonly ManualResetEventSlim _hwndReady = new(false);
    // Kept as a field — delegate must outlive the unmanaged window class registration.
    private WndProcDelegate? _wndProcDelegate;
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public event Action<IUsbDevice>? DeviceConnected;
    public event Action<IUsbDevice>? DeviceDisconnected;

    // GUID_DEVINTERFACE_USB_DEVICE — fires only for root USB device nodes, not composite children.
    private static readonly Guid GuidDevInterfaceUsbDevice =
        new("A5DCBF10-6530-11D2-901F-00C04FB951ED");

    private const uint WM_QUIT = 0x0012;
    private const uint WM_DEVICECHANGE = 0x0219;
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private const int DBT_DEVTYP_DEVICEINTERFACE = 5;    // DEV_BROADCAST_DEVICEINTERFACE
    private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;

    private const int CR_SUCCESS = 0x00000000;
    private const uint CM_DRP_DEVICEDESC = 0x00000001; // SPDRP_DEVICEDESC: device description / product string (REG_SZ)
    private const uint CM_DRP_HARDWAREID = 0x00000002; // SPDRP_HARDWAREID: hardware IDs incl. REV_ (REG_MULTI_SZ)
    private const uint CM_DRP_SERVICE = 0x00000005;    // SPDRP_SERVICE: driver service name e.g. "WinUSB" (REG_SZ)
    private const uint CM_DRP_MFG = 0x0000000C;        // SPDRP_MFG: manufacturer string (REG_SZ)

    private const uint CM_GET_DEVICE_INTERFACE_LIST_PRESENT = 0;

    // Driver service names in priority order for composite device interface selection.
    private static readonly string[] DriverPriority =
        ["WinUSB", "libusbK", "libusb0", "HidUsb", "usbser", "USBSTOR"];

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DEV_BROADCAST_DEVICEINTERFACE
    {
        public int Size;
        public int DeviceType;
        public int Reserved;
        public Guid ClassGuid;
        public char Name; // first char of variable-length null-terminated path string
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public int cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)] public string? lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int ptX, ptY;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClassExW(ref WNDCLASSEX lpWndClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowExW(
        uint dwExStyle, string lpClassName, string? lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool GetMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessageW(ref MSG lpMsg);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr RegisterDeviceNotificationW(
        IntPtr hRecipient, ref DEV_BROADCAST_DEVICEINTERFACE notificationFilter, uint flags);

    [DllImport("user32.dll")]
    private static extern bool UnregisterDeviceNotification(IntPtr handle);

    [DllImport("user32.dll")]
    private static extern bool PostThreadMessageW(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandleW(string? lpModuleName);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int CM_Locate_DevNodeW(out uint pdnDevInst, string pDeviceID, uint ulFlags);

    [DllImport("cfgmgr32.dll", ExactSpelling = true)]
    private static extern int CM_Get_Child(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

    [DllImport("cfgmgr32.dll", ExactSpelling = true)]
    private static extern int CM_Get_Sibling(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int CM_Get_DevNode_Registry_PropertyW(
        uint dnDevInst, uint ulProperty, out uint pulRegDataType,
        char[] buffer, ref uint pulLength, uint ulFlags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int CM_Get_Device_Interface_List_SizeW(
        out uint pulLen, ref Guid interfaceClassGuid, string? pDeviceID, uint ulFlags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int CM_Get_Device_Interface_ListW(
        ref Guid interfaceClassGuid, string? pDeviceID, char[] buffer, uint bufferLen, uint ulFlags);

    public void Start()
    {
        _wndProcDelegate = WndProc;
        _hwndReady.Reset();
        _messageThread = new Thread(MessagePump) { IsBackground = true, Name = "UsbDetectorMessagePump" };
        _messageThread.Start();
        _hwndReady.Wait();
        // RegisterDeviceNotification delivers future arrivals only; a board already sitting in
        // bootloader mode when the app launches must be swept up explicitly. Runs after the
        // notification window exists so nothing can slip between sweep and subscription (a
        // device delivered by both is dropped by HandleArrival's duplicate-path guard).
        EnumeratePresentDevices();
    }

    private void EnumeratePresentDevices()
    {
        try
        {
            Guid guid = GuidDevInterfaceUsbDevice;
            if (CM_Get_Device_Interface_List_SizeW(out uint len, ref guid, null, CM_GET_DEVICE_INTERFACE_LIST_PRESENT) != CR_SUCCESS || len <= 1)
                return;
            var buffer = new char[len];
            if (CM_Get_Device_Interface_ListW(ref guid, null, buffer, len, CM_GET_DEVICE_INTERFACE_LIST_PRESENT) != CR_SUCCESS)
                return;
            foreach (string path in new string(buffer).Split('\0', StringSplitOptions.RemoveEmptyEntries))
            {
                HandleArrival(path);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Initial USB device enumeration failed: {ex.Message}");
        }
    }

    public void Stop()
    {
        uint tid = _messageThreadId;
        if (tid != 0)
        {
            PostThreadMessageW(tid, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        }
        _messageThread?.Join(TimeSpan.FromSeconds(2));
        _messageThread = null;
        _messageThreadId = 0;
    }

    public void Dispose() => Stop();

    private void MessagePump()
    {
        _messageThreadId = GetCurrentThreadId();

        // Unique class name avoids conflicts if the process hosts multiple instances.
        string className = $"QmkToolboxUsbDetector_{Environment.ProcessId}";
        var wndClass = new WNDCLASSEX
        {
            cbSize = Marshal.SizeOf<WNDCLASSEX>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate!),
            hInstance = GetModuleHandleW(null),
            lpszClassName = className,
        };
        RegisterClassExW(ref wndClass);

        // HWND_MESSAGE (-3): message-only window — no taskbar entry, no screen real estate.
        _hwnd = CreateWindowExW(0, className, null, 0, 0, 0, 0, 0,
            new IntPtr(-3), IntPtr.Zero, GetModuleHandleW(null), IntPtr.Zero);

        if (_hwnd != IntPtr.Zero)
        {
            var filter = new DEV_BROADCAST_DEVICEINTERFACE
            {
                Size = Marshal.SizeOf<DEV_BROADCAST_DEVICEINTERFACE>(),
                DeviceType = DBT_DEVTYP_DEVICEINTERFACE,
                ClassGuid = GuidDevInterfaceUsbDevice,
            };
            _notifyHandle = RegisterDeviceNotificationW(_hwnd, ref filter, DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        _hwndReady.Set();

        while (GetMessageW(out MSG msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessageW(ref msg);
        }

        if (_notifyHandle != IntPtr.Zero)
        {
            UnregisterDeviceNotification(_notifyHandle);
            _notifyHandle = IntPtr.Zero;
        }
        _hwnd = IntPtr.Zero;
    }

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_DEVICECHANGE && lParam != IntPtr.Zero)
        {
            int eventType = wParam.ToInt32();
            if (eventType is DBT_DEVICEARRIVAL or DBT_DEVICEREMOVECOMPLETE)
            {
                var hdr = Marshal.PtrToStructure<DEV_BROADCAST_DEVICEINTERFACE>(lParam);
                if (hdr.DeviceType == DBT_DEVTYP_DEVICEINTERFACE)
                {
                    int nameOffset = Marshal.OffsetOf<DEV_BROADCAST_DEVICEINTERFACE>(
                        nameof(DEV_BROADCAST_DEVICEINTERFACE.Name)).ToInt32();
                    string deviceInterfacePath = Marshal.PtrToStringUni(lParam + nameOffset) ?? "";

                    if (eventType == DBT_DEVICEARRIVAL)
                    {
                        HandleArrival(deviceInterfacePath);
                    }
                    else
                    {
                        HandleRemoval(deviceInterfacePath);
                    }
                }
            }
        }
        return DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    private void HandleArrival(string deviceInterfacePath)
    {
        UsbDeviceInfo? device = BuildDeviceInfo(deviceInterfacePath);
        if (device == null)
            return;
        lock (_devicesLock)
        {
            // A device can be delivered twice when the startup sweep and a WM_DEVICECHANGE
            // arrival race; interface paths are canonical, so drop the duplicate.
            if (_devices.Any(d => string.Equals(d.DevicePath, deviceInterfacePath, StringComparison.OrdinalIgnoreCase)))
                return;
            _devices.Add(device);
        }
        DiagnosticTrace?.Invoke(
            $"[USB+] {DeviceTrace.VidPidRev(device)} path:{DeviceTrace.Path(deviceInterfacePath)}");
        DeviceConnected?.Invoke(device);
    }

    private void HandleRemoval(string deviceInterfacePath)
    {
        IUsbDevice? existing;
        lock (_devicesLock)
        {
            // Windows interface paths are canonical and always present — match by path only
            // (case-insensitive), no VID/PID fallback.
            existing = UsbDeviceMatcher.Find(_devices, deviceInterfacePath, vidPidFallback: null, StringComparison.OrdinalIgnoreCase);
            if (existing != null)
                _devices.Remove(existing);
        }
        if (DiagnosticTrace != null)
        {
            if (existing != null)
                DiagnosticTrace($"[USB-] path:{DeviceTrace.Path(deviceInterfacePath)} -> matched  ({DeviceTrace.VidPid(existing)})");
            else
                DiagnosticTrace($"[USB-] path:{DeviceTrace.Path(deviceInterfacePath)} -> no match -> dropped");
        }
        if (existing != null)
            DeviceDisconnected?.Invoke(existing);
    }

    private static UsbDeviceInfo? BuildDeviceInfo(string deviceInterfacePath)
    {
        // Convert device interface path to device instance ID for cfgmgr32:
        //   \\?\USB#VID_0483&PID_DF11#serial#{A5DCBF10-...}  →  USB\VID_0483&PID_DF11\serial
        string instanceId = InterfacePathToInstanceId(deviceInterfacePath);

        if (!UsbDeviceParser.TryParseHwId(instanceId, out ushort vid, out ushort pid, out ushort rev))
            return null;

        string product = "";
        string manufacturer = "";
        string driver = "";

        if (CM_Locate_DevNodeW(out uint devNode, instanceId, 0) == CR_SUCCESS)
        {
            // Instance IDs never carry REV_; the hardware-ID list (REG_MULTI_SZ) does —
            // e.g. USB\VID_03EB&PID_2FF4&REV_0936.
            if (rev == 0 &&
                UsbDeviceParser.TryParseRevisionFromHardwareIds(ReadDevNodeMultiSz(devNode, CM_DRP_HARDWAREID), out ushort hwRev))
            {
                rev = hwRev;
            }
            product = ReadDevNodeProperty(devNode, CM_DRP_DEVICEDESC);
            manufacturer = ReadDevNodeProperty(devNode, CM_DRP_MFG);
            string service = ReadDevNodeProperty(devNode, CM_DRP_SERVICE);
            // usbccgp is the USB composite device driver — surface the most relevant child interface instead.
            if (string.Equals(service, "usbccgp", StringComparison.OrdinalIgnoreCase))
            {
                service = GetBestCompositeInterfaceService(devNode);
            }
            driver = service;
        }

        return new UsbDeviceInfo(vid, pid, rev, manufacturer, product, driver, deviceInterfacePath);
    }

    // \\?\USB#VID_0483&PID_DF11#3C00...#{a5dcbf10-...}  →  USB\VID_0483&PID_DF11\3C00...
    private static string InterfacePathToInstanceId(string path)
    {
        if (path.StartsWith(@"\\?\", StringComparison.Ordinal))
        {
            path = path[4..];
        }
        path = path.Replace('#', '\\');
        // Strip the trailing \{interface-class-guid} segment.
        int guidStart = path.LastIndexOf('\\');
        if (guidStart > 0 && guidStart + 1 < path.Length && path[guidStart + 1] == '{')
        {
            path = path[..guidStart];
        }
        return path;
    }

    private static string GetBestCompositeInterfaceService(uint rootDevNode)
    {
        try
        {
            var services = new List<string>();
            if (CM_Get_Child(out uint child, rootDevNode, 0) != CR_SUCCESS)
            {
                return "";
            }
            do
            {
                string svc = ReadDevNodeProperty(child, CM_DRP_SERVICE);
                if (!string.IsNullOrEmpty(svc) &&
                    !string.Equals(svc, "usbccgp", StringComparison.OrdinalIgnoreCase))
                {
                    services.Add(svc);
                }
            }
            while (CM_Get_Sibling(out child, child, 0) == CR_SUCCESS);

            foreach (string preferred in DriverPriority)
            {
                if (services.Any(s => string.Equals(s, preferred, StringComparison.OrdinalIgnoreCase)))
                {
                    return preferred;
                }
            }
            return services.FirstOrDefault() ?? "";
        }
        catch (Exception ex) { Trace.WriteLine($"cfgmgr32 composite interface query failed: {ex.Message}"); }
        return "";
    }

    private static string ReadDevNodeProperty(uint devNode, uint property)
    {
        var buffer = new char[260];
        uint size = (uint)(buffer.Length * 2);
        return CM_Get_DevNode_Registry_PropertyW(devNode, property, out _, buffer, ref size, 0) == CR_SUCCESS && size > 2
            ? new string(buffer, 0, (int)(size / 2) - 1)
            : "";
    }

    private static string[] ReadDevNodeMultiSz(uint devNode, uint property)
    {
        var buffer = new char[1024];
        uint size = (uint)(buffer.Length * 2);
        return CM_Get_DevNode_Registry_PropertyW(devNode, property, out _, buffer, ref size, 0) != CR_SUCCESS || size < 2
            ? []
            : new string(buffer, 0, (int)(size / 2)).Split('\0', StringSplitOptions.RemoveEmptyEntries);
    }
}
#endif
