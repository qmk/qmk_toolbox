using System;
using System.Management;
using System.Text.RegularExpressions;

namespace QMK_Toolbox.Usb
{
    public class UsbDevice : IUsbDevice
    {
        private static readonly Regex HardwareIdTripletRegex = new Regex(@"USB\\VID_([0-9A-F]{4})&PID_([0-9A-F]{4})&REV_([0-9A-F]{4}).*");

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

            var hardwareIdTriplet = HardwareIdTripletRegex.Match(GetHardwareId(WmiDevice));
            VendorId = Convert.ToUInt16(hardwareIdTriplet.Groups[1].ToString(), 16);
            ProductId = Convert.ToUInt16(hardwareIdTriplet.Groups[2].ToString(), 16);
            RevisionBcd = Convert.ToUInt16(hardwareIdTriplet.Groups[3].ToString(), 16);

            Driver = GetDriverName(WmiDevice);
        }

        public override string ToString()
        {
            return $"{ManufacturerString} {ProductString} ({VendorId:X4}:{ProductId:X4}:{RevisionBcd:X4})";
        }

        private static string GetHardwareId(ManagementBaseObject d)
        {
            var hardwareIds = (string[])d.GetPropertyValue("HardwareID");
            if (hardwareIds != null && hardwareIds.Length > 0)
            {
                return hardwareIds[0];
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

        protected string FindComPort()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT PNPDeviceID, DeviceID FROM Win32_SerialPort"))
            {
                foreach (var device in searcher.Get())
                {
                    if (device.GetPropertyValue("PNPDeviceID").ToString().Equals(WmiDevice.GetPropertyValue("PNPDeviceID").ToString()))
                    {
                        return device.GetPropertyValue("DeviceID").ToString();
                    }
                }
            }

            return null;
        }
    }
}
