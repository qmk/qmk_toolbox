using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Reads USB device properties from the macOS IOKit registry. Usb.Events does not surface
/// <c>bcdDevice</c> (nor the <c>io_service_t</c> needed to query it), so the registry is
/// re-queried by VID/PID at arrival time.
/// </summary>
[SupportedOSPlatform("macos")]
internal static class MacUsbRegistry
{
    private const string IOKitLib = "/System/Library/Frameworks/IOKit.framework/IOKit";
    private const string CoreFoundationLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    private const uint KCfStringEncodingUtf8 = 0x08000100;
    private const int KCfNumberIntType = 9;

    [DllImport(IOKitLib, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern IntPtr IOServiceMatching(string name);

    [DllImport(IOKitLib, ExactSpelling = true)]
    private static extern int IOServiceGetMatchingServices(IntPtr mainPort, IntPtr matching, out IntPtr iterator);

    [DllImport(IOKitLib, ExactSpelling = true)]
    private static extern IntPtr IOIteratorNext(IntPtr iterator);

    [DllImport(IOKitLib, ExactSpelling = true)]
    private static extern int IOObjectRelease(IntPtr obj);

    [DllImport(IOKitLib, ExactSpelling = true)]
    private static extern IntPtr IORegistryEntryCreateCFProperty(IntPtr entry, IntPtr key, IntPtr allocator, uint options);

    [DllImport(CoreFoundationLib, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern IntPtr CFStringCreateWithCString(IntPtr alloc, string cStr, uint encoding);

    [DllImport(CoreFoundationLib, ExactSpelling = true)]
    private static extern bool CFNumberGetValue(IntPtr number, int theType, out int value);

    [DllImport(CoreFoundationLib, ExactSpelling = true)]
    private static extern void CFRelease(IntPtr cf);

    /// <summary>
    /// Returns <c>bcdDevice</c> for the first present USB device matching the VID/PID, or 0
    /// when no match is found. Two identical boards report the same revision in practice, so
    /// VID/PID matching is sufficient for the QMK-revision-marker check.
    /// </summary>
    public static ushort ReadBcdDevice(ushort vendorId, ushort productId)
    {
        try
        {
            // IOUSBHostDevice is the modern class (10.11+); IOUSBDevice covers older stacks.
            ushort rev = Query("IOUSBHostDevice", vendorId, productId);
            return rev != 0 ? rev : Query("IOUSBDevice", vendorId, productId);
        }
        catch (Exception)
        {
            // A failed registry lookup must never break device detection.
            return 0;
        }
    }

    private static ushort Query(string className, ushort vendorId, ushort productId)
    {
        // IOServiceMatching's returned dictionary is consumed by IOServiceGetMatchingServices.
        IntPtr matching = IOServiceMatching(className);
        if (matching == IntPtr.Zero)
            return 0;
        if (IOServiceGetMatchingServices(IntPtr.Zero, matching, out IntPtr iterator) != 0 || iterator == IntPtr.Zero)
            return 0;
        try
        {
            IntPtr service;
            while ((service = IOIteratorNext(iterator)) != IntPtr.Zero)
            {
                try
                {
                    if (ReadUShortProperty(service, "idVendor") == vendorId &&
                        ReadUShortProperty(service, "idProduct") == productId)
                    {
                        ushort rev = ReadUShortProperty(service, "bcdDevice");
                        if (rev != 0)
                            return rev;
                    }
                }
                finally
                {
                    IOObjectRelease(service);
                }
            }
        }
        finally
        {
            IOObjectRelease(iterator);
        }
        return 0;
    }

    private static ushort ReadUShortProperty(IntPtr service, string key)
    {
        IntPtr cfKey = CFStringCreateWithCString(IntPtr.Zero, key, KCfStringEncodingUtf8);
        if (cfKey == IntPtr.Zero)
            return 0;
        try
        {
            IntPtr number = IORegistryEntryCreateCFProperty(service, cfKey, IntPtr.Zero, 0);
            if (number == IntPtr.Zero)
                return 0;
            try
            {
                return CFNumberGetValue(number, KCfNumberIntType, out int value) ? (ushort)value : (ushort)0;
            }
            finally
            {
                CFRelease(number);
            }
        }
        finally
        {
            CFRelease(cfKey);
        }
    }
}
