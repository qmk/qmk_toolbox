//  Created by Jack Humbert on 11/2/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace QMK_Toolbox
{
    public class Usb
    {
        private readonly int[] _devicesAvailable = new int[(int)Chipset.NumberOfChipsets];
        private readonly Flashing _flasher;
        private readonly Printing _printer;
        private readonly Regex DeviceIdRegex = new Regex(@"USB\\VID_([0-9A-F]+).*PID_([0-9A-F]+).*REV_([0-9A-F]+).*");

        public Usb(Flashing flasher, Printing printer)
        {
            _flasher = flasher;
            _printer = printer;
        }

        public bool DetectBootloaderFromCollection(ManagementObjectCollection collection, bool connected = true)
        {
            var found = false;
            foreach (var instance in collection)
            {
                found = DetectBootloader(instance, connected);
            }
            return found;
        }

        private static bool MatchVid(string did, ushort vid) => Regex.Match(did, $"USB\\\\VID_{vid:X4}.*").Success;

        private static bool MatchVidPid(string did, ushort vid, ushort pid) => Regex.Match(did, $"USB\\\\VID_{vid:X4}&PID_{pid:X4}.*").Success;

        private static bool MatchRev(string did, ushort rev) => Regex.Match(did, $"USB\\\\.+REV_{rev:X4}.*").Success;

        private static string[] caterinaVids =
        {
            "1B4F", // Spark Fun Electronics
            "1FFB", // Pololu Electronics
            "2341", // Arduino SA
            "239A", // Adafruit Industries LLC
            "2A03"  // dog hunter AG
        };

        private static string[] caterinaPids =
        {
            // Adafruit Industries LLC
            "000C", // Feather 32U4
            "000D", // ItsyBitsy 32U4 3V3/8MHz
            "000E", // ItsyBitsy 32U4 5V/16MHz
            // Arduino SA / dog hunter AG
            "0036", // Leonardo
            "0037", // Micro
            // Pololu Electronics
            "0101", // A-Star 32U4
            // Spark Fun Electronics
            "9203", // Pro Micro 3V3/8MHz
            "9205", // Pro Micro 5V/16MHz
            "9207"  // LilyPad 3V3/8MHz (and some Pro Micro clones)
        };

        private static string[] atmelDfuPids =
        {
            "2FEF", // ATmega16U2
            "2FF0", // ATmega32U2
            "2FF3", // ATmega16U4
            "2FF4", // ATmega32U4
            "2FF9", // AT90USB64
            "2FFB"  // AT90USB128
        };

        public bool DetectBootloader(ManagementBaseObject instance, bool connected = true)
        {
            var deviceId = GetHardwareId(instance);

            var vpr = DeviceIdRegex.Match(deviceId);
            var vid = vpr.Groups[1].ToString().PadLeft(4, '0');
            var pid = vpr.Groups[2].ToString().PadLeft(4, '0');
            var rev = vpr.Groups[3].ToString().PadLeft(4, '0');

            string deviceName;
            string comPort = null;
            string driverName = GetDriverName(instance);

            if (IsSerialDevice(instance))
            {
                comPort = GetComPort(instance);

                if (MatchVidPid(deviceId, 0x03EB, 0x6124)) // Atmel SAM-BA
                {
                    deviceName = "Atmel SAM-BA";
                    _flasher.ComPort = comPort;
                    _devicesAvailable[(int)Chipset.AtmelSamBa] += connected ? 1 : -1;
                }
                else if (caterinaVids.Contains(vid) && caterinaPids.Contains(pid)) // Caterina
                {
                    deviceName = "Caterina";
                    _flasher.ComPort = comPort;
                    _devicesAvailable[(int)Chipset.Caterina] += connected ? 1 : -1;
                }
                else if (MatchVidPid(deviceId, 0x16C0, 0x0483)) // ArduinoISP/AVRISP
                {
                    deviceName = "AVRISP";
                    _flasher.ComPort = comPort;
                    _devicesAvailable[(int)Chipset.AvrIsp] += connected ? 1 : -1;
                }
                else
                {
                    return false;
                }
            }
            else if (MatchVid(deviceId, 0x03EB) && atmelDfuPids.Contains(pid)) // Atmel DFU
            {
                deviceName = "Atmel DFU";
                _devicesAvailable[(int)Chipset.AtmelDfu] += connected ? 1 : -1;
            }
            else if (MatchVidPid(deviceId, 0x16C0, 0x0478)) // PJRC Teensy
            {
                deviceName = "Halfkay";
                _devicesAvailable[(int)Chipset.Halfkay] += connected ? 1 : -1;
            }
            else if (MatchVidPid(deviceId, 0x0483, 0xDF11)) // STM32 DFU
            {
                deviceName = "STM32 DFU";
                _devicesAvailable[(int)Chipset.Stm32Dfu] += connected ? 1 : -1;
            }
            else if (MatchVidPid(deviceId, 0x1C11, 0xB007)) // Kiibohd
            {
                deviceName = "Kiibohd";
                _devicesAvailable[(int)Chipset.Kiibohd] += connected ? 1 : -1;
            }
            else if (MatchVidPid(deviceId, 0x16C0, 0x05DF)) // Objective Development BootloadHID
            {
                deviceName = "BootloadHID";
                _devicesAvailable[(int)Chipset.BootloadHid] += connected ? 1 : -1;
            }
            else if (MatchVidPid(deviceId, 0x16C0, 0x05DC)) // USBAsp and USBAspLoader
            {
                deviceName = "USBAsp";
                _devicesAvailable[(int)Chipset.UsbAsp] += connected ? 1 : -1;
            }
            else if (MatchVidPid(deviceId, 0x1781, 0x0C9F)) // AVR Pocket ISP
            {
                deviceName = "USB Tiny";
                _devicesAvailable[(int)Chipset.UsbTiny] += connected ? 1 : -1;
            }
            else
            {
                return false;
            }

            var connectedString = connected ? "connected" : "disconnected";
            var comPortString = comPort != null ? $" [{comPort}]" : "";
            var driverString = driverName ?? "NO DRIVER";

            _printer.Print($"{deviceName} device {connectedString} ({driverString}): {instance.GetPropertyValue("Manufacturer")} {instance.GetPropertyValue("Name")} ({vid}:{pid}:{rev}){comPortString}", MessageType.Bootloader);

            return true;
        }

        public string GetHardwareId(ManagementBaseObject instance)
        {
            var hardwareIds = (System.String[])instance.GetPropertyValue("HardwareID");
            if (hardwareIds != null && hardwareIds.Length > 0)
            {
                return hardwareIds[0];
            }

            return null;
        }

        public string GetDriverName(ManagementBaseObject instance)
        {
            var service = (string)instance.GetPropertyValue("Service");
            if (service != null && service.Length > 0)
            {
                return service;
            }

            return null;
        }

        public bool IsSerialDevice(ManagementBaseObject instance)
        {
            var compatibleIds = (System.String[])instance.GetPropertyValue("CompatibleID");
            return (compatibleIds != null && compatibleIds.Contains("USB\\Class_02&SubClass_02")); // CDC-ACM
        }

        public string GetComPort(ManagementBaseObject instance)
        {
            using (var searcher = new ManagementObjectSearcher("SELECT PNPDeviceID, DeviceID FROM Win32_SerialPort"))
            {
                foreach (var device in searcher.Get())
                {
                    if (device.GetPropertyValue("PNPDeviceID").ToString().Equals(instance.GetPropertyValue("PNPDeviceID").ToString()))
                    {
                        return device.GetPropertyValue("DeviceID").ToString();
                    }
                }
            }

            return null;
        }

        public bool CanFlash(Chipset chipset) => _devicesAvailable[(int)chipset] > 0;

        public bool AreDevicesAvailable()
        {
            var available = false;
            for (var i = 0; i < (int)Chipset.NumberOfChipsets; i++)
            {
                available |= _devicesAvailable[i] > 0;
            }
            return available;
        }
    }
}
