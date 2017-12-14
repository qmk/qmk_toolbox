//  Created by Jack Humbert on 11/2/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;

namespace QMK_Toolbox {

    public class USB {

        private int[] devicesAvailable = new int[(int)Chipset.NumberOfChipsets];
        Flashing flasher;
        Printing printer;

        public USB(Flashing flasher, Printing printer) {
            this.flasher = flasher;
            this.printer = printer;
        }

        public bool DetectBootloaderFromCollection(ManagementObjectCollection collection, bool connected = true) {
            bool found = false;
            foreach (var instance in collection) {
                found = DetectBootloader(instance, connected);
            }
            return found;
        }

        static private bool matchVID(string DID, UInt16 VID) {
            var reg = Regex.Match(DID, "USB.*VID_" + VID.ToString("X4") + ".*");
            return reg.Success;
        }

        static private bool matchPID(string DID, UInt16 PID) {
            var reg = Regex.Match(DID, "USB.*PID_" + PID.ToString("X4") + ".*");
            return reg.Success;
        }

        public bool DetectBootloader(ManagementBaseObject instance, bool connected = true) {
            string connected_string = connected ? "connected" : "disconnected";
            string device_id = instance.GetPropertyValue("DeviceID").ToString();

            Regex deviceid_regex = new Regex(@"VID_([0-9A-F]+).*PID_([0-9A-F]+)\\([0-9A-F]+)");
            var vp = deviceid_regex.Match(instance.GetPropertyValue("DeviceID").ToString());
            string VID = vp.Groups[1].ToString();
            string PID = vp.Groups[2].ToString();
            string VER = vp.Groups[3].ToString();

            string device_name;
            // Detects Atmel Vendor ID
            if (matchVID(device_id, 0x03EB)) {
                device_name = "DFU";
                devicesAvailable[(int)Chipset.DFU] += connected ? 1 : -1;
                // Detects Arduino Vendor ID, Sparkfun Vendor ID, Adafruit Vendor ID
            } else if (matchVID(device_id, 0x2341) || matchVID(device_id, 0x1B4F) || matchVID(device_id, 0x239a)) {
                device_name = "Caterina";
                Regex regex = new Regex("(COM[0-9]+)");
                var v = regex.Match(instance.GetPropertyValue("Name").ToString());
                flasher.caterinaPort = v.Groups[1].ToString();
                devicesAvailable[(int)Chipset.Caterina] += connected ? 1 : -1;
                // Detects PJRC VID & PID
            } else if (matchVID(device_id, 0x16C0) && matchPID(device_id, 0x0478)) {
                device_name = "Halfkay";
                devicesAvailable[(int)Chipset.Halfkay] += connected ? 1 : -1;
                // Detects STM32 PID & VID
            } else if (matchVID(device_id, 0x0483) && matchPID(device_id, 0xDF11)) {
                device_name = "STM32";
                devicesAvailable[(int)Chipset.STM32] += connected ? 1 : -1;
                // Detects Kiibohd VID & PID
            } else if (matchVID(device_id, 0x1C11) && matchPID(device_id, 0xB007)) {
                device_name = "Kiibohd";
                devicesAvailable[(int)Chipset.Kiibohd] += connected ? 1 : -1;
                // Detects Arduino ISP VID & PID
            } else if (matchVID(device_id, 0x16C0) && matchPID(device_id, 0x0483)) {
                device_name = "AVRISP";
                Regex regex = new Regex("(COM[0-9]+)");
                var v = regex.Match(instance.GetPropertyValue("Name").ToString());
                flasher.caterinaPort = v.Groups[1].ToString();
                devicesAvailable[(int)Chipset.AVRISP] += connected ? 1 : -1;
                // Detects AVR Pocket ISP VID & PID
            } else if (matchVID(device_id, 0x1781) && matchPID(device_id, 0x0C9F)) {
                device_name = "USB Tiny";
                Regex regex = new Regex("(COM[0-9]+)");
                var v = regex.Match(instance.GetPropertyValue("Name").ToString());
                flasher.caterinaPort = v.Groups[1].ToString();
                devicesAvailable[(int)Chipset.USBTiny] += connected ? 1 : -1;
            } else {
                return false;
            }

            printer.print(device_name + " device " + connected_string + ": " + instance.GetPropertyValue("Name") + " -- " + VID + ":" + PID + ":" + VER, MessageType.Bootloader);
            return true;
        }

        public bool canFlash(Chipset chipset) {
            return (devicesAvailable[(int)chipset] > 0);
        }

        public bool areDevicesAvailable() {
            bool available = false;
            for (int i = 0; i < (int)Chipset.NumberOfChipsets; i++) {
                available |= (devicesAvailable[i] > 0);
            }
            return available;
        }


    }

}