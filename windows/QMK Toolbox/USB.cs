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

        private static UInt16[] caterinaVids =
        {
            0x1B4F, // Spark Fun Electronics
            0x1FFB, // Pololu Electronics
            0x2341, // Arduino SA
            0x239A, // Adafruit Industries LLC
            0x2A03  // dog hunter AG
        };

        private static UInt16[] caterinaPids =
        {
            // Adafruit Industries LLC
            0x000C, // Feather 32U4
            0x000D, // ItsyBitsy 32U4 3V3/8MHz
            0x000E, // ItsyBitsy 32U4 5V/16MHz
            // Arduino SA / dog hunter AG
            0x0036, // Leonardo
            0x0037, // Micro
            // Pololu Electronics
            0x0101, // A-Star 32U4
            // Spark Fun Electronics
            0x9203, // Pro Micro 3V3/8MHz
            0x9205, // Pro Micro 5V/16MHz
            0x9207  // LilyPad 3V3/8MHz (and some Pro Micro clones)
        };

        private static UInt16[] atmelDfuPids =
        {
            0x2FEF, // ATmega16U2
            0x2FF0, // ATmega32U2
            0x2FF3, // ATmega16U4
            0x2FF4, // ATmega32U4
            0x2FF9, // AT90USB64
            0x2FFB  // AT90USB128
        };

        public bool DetectBootloader(ManagementBaseObject instance, bool connected = true)
        {
            var deviceId = GetHardwareId(instance);

            var vpr = DeviceIdRegex.Match(deviceId);
            if (vpr.Success)
            {
                var vendorId = Convert.ToUInt16(vpr.Groups[1].ToString(), 16);
                var productId = Convert.ToUInt16(vpr.Groups[2].ToString(), 16);
                var revisionBcd = Convert.ToUInt16(vpr.Groups[3].ToString(), 16);

                string deviceName;
                string comPort = null;
                string driverName = GetDriverName(instance);
                Chipset deviceType;

                if (IsSerialDevice(instance))
                {
                    if (vendorId == 0x03EB && productId == 0x6124) // Atmel SAM-BA
                    {
                        deviceName = "Atmel SAM-BA";
                        deviceType = Chipset.AtmelSamBa;
                    }
                    else if (caterinaVids.Contains(vendorId) && caterinaPids.Contains(productId)) // Caterina
                    {
                        deviceName = "Caterina";
                        deviceType = Chipset.Caterina;
                    }
                    else if (vendorId == 0x16C0 && productId == 0x0483) // ArduinoISP/AVRISP
                    {
                        deviceName = "AVRISP";
                        deviceType = Chipset.AvrIsp;
                    }
                    else
                    {
                        return false;
                    }

                    comPort = GetComPort(instance);
                    _flasher.ComPort = comPort;
                }
                else if (vendorId == 0x03EB && atmelDfuPids.Contains(productId)) // Atmel DFU
                {
                    deviceName = "Atmel DFU";
                    deviceType = Chipset.AtmelDfu;
                }
                else if (vendorId == 0x16C0 && productId == 0x0478) // PJRC Teensy
                {
                    deviceName = "Halfkay";
                    deviceType = Chipset.Halfkay;
                }
                else if (vendorId == 0x0483 && productId == 0xDF11) // STM32 DFU
                {
                    deviceName = "STM32 DFU";
                    deviceType = Chipset.Stm32Dfu;
                }
                else if (vendorId == 0x314B && productId == 0x0106) // APM32 DFU
                {
                    deviceName = "APM32 DFU";
                    deviceType = Chipset.Apm32Dfu;
                }
                else if (vendorId == 0x1C11 && productId == 0xB007) // Kiibohd
                {
                    deviceName = "Kiibohd";
                    deviceType = Chipset.Kiibohd;
                }
                else if (vendorId == 0x16C0 && productId == 0x05DF) // Objective Development BootloadHID
                {
                    deviceName = "BootloadHID";
                    deviceType = Chipset.BootloadHid;
                }
                else if (vendorId == 0x16C0 && productId == 0x05DC) // USBasp and USBaspLoader
                {
                    deviceName = "USBasp";
                    deviceType = Chipset.UsbAsp;
                }
                else if (vendorId == 0x1781 && productId == 0x0C9F) // AVR Pocket ISP
                {
                    deviceName = "USB Tiny";
                    deviceType = Chipset.UsbTiny;
                }
                else if (vendorId == 0x1EAF && productId == 0x0003) // STM32Duino
                {
                    deviceName = "STM32Duino";
                    deviceType = Chipset.Stm32Duino;
                }
                else
                {
                    return false;
                }

                var connectedString = connected ? "connected" : "disconnected";
                var comPortString = comPort != null ? $" [{comPort}]" : "";
                var driverString = driverName ?? "NO DRIVER";

                _printer.Print($"{deviceName} device {connectedString} ({driverString}): {instance.GetPropertyValue("Manufacturer")} {instance.GetPropertyValue("Name")} ({vendorId:X4}:{productId:X4}:{revisionBcd:X4}){comPortString}", MessageType.Bootloader);

                _devicesAvailable[(int)deviceType] += (connected ? 1 : -1);

                return true;
            }

            return false;
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
