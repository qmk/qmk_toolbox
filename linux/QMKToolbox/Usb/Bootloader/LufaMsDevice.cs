// ReSharper disable StringLiteralTypo
using System.IO;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal class LufaMsDevice : BootloaderDevice
{
    private string _mountPoint;

    public LufaMsDevice(KnownHidDevice d) : base(d)
    {
        Type = BootloaderType.LufaMs;
        Name = "LUFA MS";

        MountPoint = FindMountPoint();
    }

    private string MountPoint { get; }
    
    public override void Flash(string mcu, string file)
    {
        Task.Run(() =>
        {
            if (MountPoint == null)
            {
                PrintMessage("Mount point not found!", MessageType.Error);
                return;
            }

            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                var destFile = $"{MountPoint}/FLASH.BIN";

                try
                {
                    PrintMessage($"Deleting {destFile}...", MessageType.Command);
                    File.Delete(destFile);
                    PrintMessage($"Copying {file} to {destFile}...", MessageType.Command);
                    File.Copy(file, destFile);

                    PrintMessage("Done, please eject drive now.", MessageType.Bootloader);
                }
                catch (IOException e)
                {
                    PrintMessage($"IO ERROR: {e.Message}", MessageType.Error);
                }
            }
            else
            {
                PrintMessage("Only firmware files in .bin format can be flashed with this bootloader!",
                    MessageType.Error);
            }
        }).Wait();
    }

    public override string ToString()
    {
        return $"{base.ToString()} [{MountPoint}]";
    }

    private string FindMountPoint()
    {
        // TODO find out if this works, rework to match to usb device that was mounted.
        // this mounts the first removable drive.
        OutputReceived += (_, data, _) => { _mountPoint = data; };
        RunProcessAsync("bash", "-c \"lsblk | grep media | head -n 1 | awk '{print $7}'\"").Wait();

        return _mountPoint.Contains("media") ? _mountPoint : null;
    }
}