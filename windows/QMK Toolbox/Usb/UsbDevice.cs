using System;
using System.Management;
using System.Text.RegularExpressions;

namespace QMK_Toolbox.Usb
{
    public class UsbDevice : IUsbDevice
    {
        private static readonly Regex HardwareIdTripletRegex = new(@"USB\\VID_([0-9A-F]{4})&PID_([0-9A-F]{4})&REV_([0-9A-F]{4}).*");

        public ManagementBaseObject WmiDevice { get; }

        public ushort VendorId { get; }

        public ushort ProductId { get; }

        public ushort RevisionBcd { get; }

        public string ManufacturerString { get; }

        public string ProductString { get; }

        public string Driver { get; }

        public UsbDevice(ManagementBaseObject d)
        {
            WmiDevice = d;

            ManufacturerString = (string)WmiDevice.GetPropertyValue("Manufacturer");
            ProductString = (string)WmiDevice.GetPropertyValue("Name");

            var hardwareIdTriplet = GetHardwareId(WmiDevice).Value;
            VendorId = hardwareIdTriplet.Item1;
            ProductId = hardwareIdTriplet.Item2;
            RevisionBcd = hardwareIdTriplet.Item3;

            Driver = GetDriverName(WmiDevice);
        }

        public override string ToString()
        {
            return $"{ManufacturerString} {ProductString} ({VendorId:X4}:{ProductId:X4}:{RevisionBcd:X4})";
        }

        private static (ushort, ushort, ushort)? GetHardwareId(ManagementBaseObject d)
        {
            var hardwareIds = (string[])d.GetPropertyValue("HardwareID");
            if (hardwareIds != null)
            {
                foreach (string hardwareId in hardwareIds)
                {
                    Match match = HardwareIdTripletRegex.Match(hardwareId);
                    if (match.Success)
                    {
                        return (
                            Convert.ToUInt16(match.Groups[1].ToString(), 16),
                            Convert.ToUInt16(match.Groups[2].ToString(), 16),
                            Convert.ToUInt16(match.Groups[3].ToString(), 16)
                        );
                    }
                }
            }

            return null;
        }

        private static string GetDriverName(ManagementBaseObject d)
        {
            var service = (string)d.GetPropertyValue("Service");
            if (service != null && service.Length > 0)
            {
                return service;
            }

            return "NO DRIVER";
        }
    }
}
