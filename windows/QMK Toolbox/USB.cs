//  Created by Jack Humbert on 11/2/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;

namespace QMK_Toolbox
{
    public class Usb
    {
        private readonly int[] _devicesAvailable = new int[(int)Chipset.NumberOfChipsets];
        private readonly Flashing _flasher;
        private readonly Printing _printer;

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

        private static bool MatchVid(string did, ushort vid) => Regex.Match(did, $"USB.*VID_{vid:X4}.*").Success;

        private static bool MatchPid(string did, ushort pid) => Regex.Match(did, $"USB.*PID_{pid:X4}.*").Success;

        public bool DetectBootloader(ManagementBaseObject instance, bool connected = true)
        {
            var connectedString = connected ? "connected" : "disconnected";
            var deviceId = instance.GetPropertyValue("DeviceID").ToString();

            var deviceidRegex = new Regex(@"VID_([0-9A-F]+).*PID_([0-9A-F]+)\\([0-9A-F]+)");
            var vp = deviceidRegex.Match(instance.GetPropertyValue("DeviceID").ToString());
            var vid = vp.Groups[1].ToString().PadLeft(4, '0');
            var pid = vp.Groups[2].ToString().PadLeft(4, '0');
            var ver = vp.Groups[3].ToString().PadLeft(4, '0');
            var uni = instance.GetPropertyValue("ClassGuid").ToString();
            var comRegex = new Regex("(COM[0-9]+)");

            string deviceName;
            if (MatchVid(deviceId, 0x03EB) && MatchPid(deviceId, 0x6124)) // Detects Atmel SAM-BA VID & PID
            {
                deviceName = "Atmel SAM-BA";
                _flasher.CaterinaPort = GetComPort(deviceId);
                _devicesAvailable[(int)Chipset.AtmelSamBa] += connected ? 1 : -1;
            }
            else if (MatchVid(deviceId, 0x03EB)) // Detects Atmel Vendor ID for other Atmel devices
            {
                deviceName = "DFU";
                _devicesAvailable[(int)Chipset.Dfu] += connected ? 1 : -1;
            }
            else if (MatchVid(deviceId, 0x2341) || MatchVid(deviceId, 0x1B4F) || MatchVid(deviceId, 0x239a)) // Detects Arduino Vendor ID, Sparkfun Vendor ID, Adafruit Vendor ID
            {
                deviceName = "Caterina";
                _flasher.CaterinaPort = GetComPort(deviceId);
                _devicesAvailable[(int)Chipset.Caterina] += connected ? 1 : -1;
            }
            else if (MatchVid(deviceId, 0x16C0) && MatchPid(deviceId, 0x0478)) // Detects PJRC VID & PID
            {
                deviceName = "Halfkay";
                _devicesAvailable[(int)Chipset.Halfkay] += connected ? 1 : -1;
            }
            else if (MatchVid(deviceId, 0x0483) && MatchPid(deviceId, 0xDF11)) // Detects STM32 PID & VID
            {
                deviceName = "STM32";
                _devicesAvailable[(int)Chipset.Stm32] += connected ? 1 : -1;
            }
            else if (MatchVid(deviceId, 0x1C11) && MatchPid(deviceId, 0xB007)) // Detects Kiibohd VID & PID
            {
                deviceName = "Kiibohd";
                _devicesAvailable[(int)Chipset.Kiibohd] += connected ? 1 : -1;
            }
            else if (MatchVid(deviceId, 0x16C0) && MatchPid(deviceId, 0x0483)) // Detects Arduino ISP VID & PID
            {
                deviceName = "AVRISP";
                _flasher.CaterinaPort = GetComPort(deviceId);
                _devicesAvailable[(int)Chipset.Avrisp] += connected ? 1 : -1;
            }
            else if (MatchVid(deviceId, 0x16C0) && MatchPid(deviceId, 0x05DC)) // Detects AVR USBAsp VID & PID
            {
                deviceName = "USBAsp";
                _flasher.CaterinaPort = GetComPort(deviceId);
                _devicesAvailable[(int)Chipset.Usbasp] += connected ? 1 : -1;
            }
            else if (MatchVid(deviceId, 0x1781) && MatchPid(deviceId, 0x0C9F)) // Detects AVR Pocket ISP VID & PID
            {
                deviceName = "USB Tiny";

                _flasher.CaterinaPort = GetComPort(deviceId);
                _devicesAvailable[(int)Chipset.UsbTiny] += connected ? 1 : -1;
            } 
            else if (MatchVid(deviceId, 0x16C0) && MatchPid(deviceId, 0x05DF)) // Detects Objective Development VID & PID
            {
                deviceName = "BootloadHID";
                _devicesAvailable[(int)Chipset.BootloadHID] += connected ? 1 : -1;
            } 
            else
            {
                return false;
            }

            _printer.Print($"{deviceName} device {connectedString}: {instance.GetPropertyValue("Name")} -- {vid}:{pid}:{ver} {uni}", MessageType.Bootloader);
            return true;
        }

        public string GetComPort(string deviceId)
        {
            using (var searcher = new ManagementObjectSearcher("Select * from Win32_SerialPort"))
            {
                foreach (var device in searcher.Get())
                {
                    if (device.GetPropertyValue("PNPDeviceID").ToString().Equals(deviceId))
                    {
                        return device.GetPropertyValue("DeviceID").ToString();
                    }
                }
            }

            return string.Empty;
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