using System.Runtime.Versioning;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Cross-platform mount point service for mass-storage bootloader devices (LUFA MS, UF2).
/// Only volumes carrying the caller's marker file qualify, so an unrelated removable drive
/// is never selected; among qualifying volumes the most recently mounted wins.
/// <para>
/// Known limitation: with two devices of the same bootloader family mounted simultaneously,
/// the newer volume is selected. In practice users rarely have two devices in bootloader
/// mode at once.
/// </para>
/// </summary>
public class DesktopMountPointService : IMountPointService
{
    public string? FindMountPoint(IUsbDevice device, string markerFile) =>
        OperatingSystem.IsWindows() ? FindMountPointWindows(markerFile) :
        OperatingSystem.IsLinux() ? FindMountPointLinux(markerFile) :
        OperatingSystem.IsMacOS() ? FindMountPointMacOS(markerFile) :
        null;

    private static bool HasMarker(string mountPoint, string markerFile) =>
        File.Exists(Path.Combine(mountPoint, markerFile));

    /// <summary>Returns the most recently created removable drive carrying the marker file.</summary>
    [SupportedOSPlatform("windows")]
    private static string? FindMountPointWindows(string markerFile)
    {
        return DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Removable && d.IsReady && HasMarker(d.Name, markerFile))
            .Select(d => new DirectoryInfo(d.Name))
            .OrderByDescending(d => d.CreationTime)
            .FirstOrDefault()?.FullName.TrimEnd('\\', '/');
    }

    /// <summary>
    /// Scans /proc/mounts for the most recently mounted volume carrying the marker file.
    /// Entries in /proc/mounts appear in mount order, so the last matching
    /// entry is the newest — no timestamp comparison needed.
    /// Matches mount points under /media/, /run/media/, and /mnt/ (covering
    /// udisks2-managed volumes on modern desktops as well as distros and setups
    /// that mount removable devices under /mnt/), which handles all USB
    /// mass-storage device node types (/dev/sd*, /dev/mmcblk*, /dev/vd*, etc.)
    /// without enumerating device-path prefixes.
    /// </summary>
    [SupportedOSPlatform("linux")]
    private static string? FindMountPointLinux(string markerFile)
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
                && HasMarker(mountPoint, markerFile))
            {
                newest = mountPoint;
            }
        }
        return newest;
    }

    /// <summary>Returns the most recently created /Volumes entry carrying the marker file.</summary>
    [SupportedOSPlatform("macos")]
    private static string? FindMountPointMacOS(string markerFile)
    {
        const string volumes = "/Volumes";
        return !Directory.Exists(volumes)
            ? null
            : (Directory.EnumerateDirectories(volumes)
            .Where(d => HasMarker(d, markerFile))
            .Select(d => new DirectoryInfo(d))
            .OrderByDescending(d => d.CreationTime)
            .FirstOrDefault()?.FullName);
    }
}
