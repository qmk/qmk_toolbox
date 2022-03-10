using System.IO;
using System.Management;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader
{
    class LufaMsDevice : BootloaderDevice
    {
        public string MountPoint { get; }

        public LufaMsDevice(UsbDevice d) : base(d)
        {
            Type = BootloaderType.LufaMs;
            Name = "LUFA MS";
            PreferredDriver = "USBSTOR";

            MountPoint = FindMountPoint();
        }

        public async override Task Flash(string mcu, string file)
        {
            await Task.Run(() =>
            {
                if (MountPoint == null)
                {
                    PrintMessage("Mount point not found!", MessageType.Error);
                    return;
                }

                if (Path.GetExtension(file)?.ToLower() == ".bin")
                {
                    var destFile = $"{MountPoint}\\FLASH.BIN";

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
                    PrintMessage("Only firmware files in .bin format can be flashed with this bootloader!", MessageType.Error);
                }
            });
        }

        public override string ToString() => $"{base.ToString()} [{MountPoint}]";

        private string FindMountPoint()
        {
            foreach (ManagementObject usbHub in new ManagementObjectSearcher("SELECT * FROM Win32_USBHub").Get())
            {
                if (usbHub.GetPropertyValue("PNPDeviceID").ToString().Equals(WmiDevice.GetPropertyValue("PNPDeviceID").ToString()))
                {
                    foreach (ManagementObject usbController in usbHub.GetRelated("Win32_USBController"))
                    {
                        foreach (ManagementObject assoc in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_USBController.DeviceID='" + usbController["PNPDeviceID"].ToString() + "'}").Get())
                        {
                            if (assoc.GetPropertyValue("CreationClassName").Equals("Win32_PnPEntity") && assoc.GetPropertyValue("DeviceID").ToString().Contains("USBSTOR"))
                            {
                                foreach (ManagementObject diskDrive in new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive").Get())
                                {
                                    if (diskDrive.GetPropertyValue("PNPDeviceID").ToString().Equals(assoc.GetPropertyValue("PNPDeviceID").ToString()))
                                    {
                                        foreach (ManagementObject partition in diskDrive.GetRelated("Win32_DiskPartition"))
                                        {
                                            foreach (ManagementObject logicalDisk in partition.GetRelated("Win32_LogicalDisk"))
                                            {
                                                return logicalDisk.GetPropertyValue("Name").ToString();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
