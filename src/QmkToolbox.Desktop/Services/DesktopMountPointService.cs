using System.Runtime.Versioning;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Cross-platform mount point service for LUFA Mass Storage devices.
/// Uses a "most recently mounted" heuristic: since FindMountPoint is called
/// immediately after device detection, the target volume will be the newest.
/// <para>
/// Known limitation: a second USB mass-storage device plugged in between the
/// bootloader detection event and this call could be selected instead. In practice
/// this window is very small and users rarely have two devices in bootloader mode
/// simultaneously.
/// </para>
/// </summary>
public class DesktopMountPointService : IMountPointService
{
    public string? FindMountPoint(IUsbDevice device) =>
        OperatingSystem.IsWindows() ? FindMountPointWindows() :
        OperatingSystem.IsLinux() ? FindMountPointLinux() :
        OperatingSystem.IsMacOS() ? FindMountPointMacOS() :
        null;

    /// <summary>
    /// Returns the most recently created removable drive.
    /// Since FindMountPoint is called immediately after device detection,
    /// the target volume will be the newest removable drive.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static string? FindMountPointWindows()
    {
        return DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
            .Select(d => new DirectoryInfo(d.Name))
            .OrderByDescending(d => d.CreationTime)
            .FirstOrDefault()?.FullName.TrimEnd('\\', '/');
    }

    /// <summary>
    /// Scans /proc/mounts for the most recently mounted removable volume.
    /// Entries in /proc/mounts appear in mount order, so the last matching
    /// entry is the newest — no timestamp comparison needed.
    /// Matches mount points under /media/, /run/media/, and /mnt/ (covering
    /// udisks2-managed volumes on modern desktops as well as distros and setups
    /// that mount removable devices under /mnt/), which handles all USB
    /// mass-storage device node types (/dev/sd*, /dev/mmcblk*, /dev/vd*, etc.)
    /// without enumerating device-path prefixes.
    /// </summary>
    [SupportedOSPlatform("linux")]
    private static string? FindMountPointLinux()
    {
        const string procMounts = "/proc/mounts";
        if (!File.Exists(procMounts))
            return null;

        string? newest = null;
        foreach (string line in File.ReadLines(procMounts))
        {
            string[] parts = line.Split(' ');
            if (parts.Length < 2)
                continue;
            // /proc/mounts encodes spaces in paths as \040 (octal 040 = space).
            string mountPoint = parts[1].Replace("\\040", " ");
            if ((mountPoint.StartsWith("/media/") || mountPoint.StartsWith("/run/media/") || mountPoint.StartsWith("/mnt/"))
                && Directory.Exists(mountPoint))
            {
                newest = mountPoint;
            }
        }
        return newest;
    }

    /// <summary>
    /// Returns the most recently created directory under /Volumes.
    /// Since FindMountPoint is called immediately after device detection,
    /// the target volume will be the newest entry.
    /// </summary>
    [SupportedOSPlatform("macos")]
    private static string? FindMountPointMacOS()
    {
        const string volumes = "/Volumes";
        return !Directory.Exists(volumes)
            ? null
            : (Directory.EnumerateDirectories(volumes)
            .Select(d => new DirectoryInfo(d))
            .OrderByDescending(d => d.CreationTime)
            .FirstOrDefault()?.FullName);
    }
}
