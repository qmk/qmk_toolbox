//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox {
    public enum Chipset {
        DFU,
        Halfkay,
        Caterina,
        STM32,
        Kiibohd,
        AVRISP,
        USBTiny,
        NumberOfChipsets
    };
    public class Flashing : EventArgs {
        private System.Diagnostics.Process process;
        private System.Diagnostics.ProcessStartInfo startInfo;

        public const ushort UsagePage = 0xFF31;
        public const int Usage = 0x0074;
        public string caterinaPort = "";
        
        private Printing printer;
        public USB usb;

        string[] resources = {
            "dfu-programmer.exe",
            "avrdude.exe",
            "avrdude.conf",
            "teensy_loader_cli.exe",
            "dfu-util.exe",
            "libusb-1.0.dll",
            "libusb0.dll", // x86/libusb0_x86.dll
            "mcu-list.txt",
            "atmega32u4_eeprom_reset.hex"
        };

        private void ExtractResource(string file) {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QMK_Toolbox." + file);
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            File.WriteAllBytes(Path.Combine(Application.LocalUserAppDataPath, file), bytes);
        }

        public Flashing(Printing printer) {
            this.printer = printer;

            foreach (string resource in resources) {
                ExtractResource(resource);
            }

            process = new System.Diagnostics.Process();
            //process.EnableRaisingEvents = true;
            //process.OutputDataReceived += OnOutputDataReceived;
            //process.ErrorDataReceived += OnErrorDataReceived;
            startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.CreateNoWindow = true;
        }

        void OnOutputDataReceived(object sender, DataReceivedEventArgs e) {
            Debug.Write(e.Data);
            printer.printResponse(e.Data, MessageType.Info);
        }

        void OnErrorDataReceived(object sender, DataReceivedEventArgs e) {
            Debug.Write(e.Data);
            printer.printResponse(e.Data, MessageType.Info);
        }

        private string runProcess(string command, string args) {
            printer.print(command + " " + args, MessageType.Command);
            startInfo.WorkingDirectory = Application.LocalUserAppDataPath;
            startInfo.FileName = Path.Combine(Application.LocalUserAppDataPath, command);
            startInfo.Arguments = args;
            process.StartInfo = startInfo;
            string output = "";

            process.Start();
            while (!process.HasExited) {
                char[] buffer = new char[4096];
                while (process.StandardOutput.Peek() > -1 || process.StandardError.Peek() > -1) {
                    if (process.StandardOutput.Peek() > -1) {
                        var length = process.StandardOutput.Read(buffer, 0, buffer.Length);
                        string data = new string(buffer, 0, length);
                        output += data;
                        printer.printResponse(data, MessageType.Info);
                    }
                    if (process.StandardError.Peek() > -1) {
                        var length = process.StandardError.Read(buffer, 0, buffer.Length);
                        string data = new string(buffer, 0, length);
                        output += data;
                        printer.printResponse(data, MessageType.Info);
                    }
                    Application.DoEvents(); // This keeps your form responsive by processing events
                }

            }
            return output;
        }

        public string[] getMCUList() {
            return File.ReadLines(Path.Combine(Application.LocalUserAppDataPath, "mcu-list.txt")).ToArray<string>();
        }

        public void flash(string mcu, string file) {
            if (usb.canFlash(Chipset.DFU))
                flashDFU(mcu, file);
            if (usb.canFlash(Chipset.Caterina))
                flashCaterina(mcu, file);
            if (usb.canFlash(Chipset.Halfkay))
                flashHalfkay(mcu, file);
            if (usb.canFlash(Chipset.STM32))
                flashSTM32(mcu, file);
            if (usb.canFlash(Chipset.Kiibohd))
                flashKiibohd(file);
            if (usb.canFlash(Chipset.AVRISP))
                flashAVRISP(mcu, file);
            if (usb.canFlash(Chipset.USBTiny))
                flashUSBTiny(mcu, file);
        }

        public void reset(string mcu) {
            if (usb.canFlash(Chipset.DFU))
                resetDFU(mcu);
            if (usb.canFlash(Chipset.Halfkay))
                resetHalfkay(mcu);
        }

        public void eepromReset(string mcu) {
            if (usb.canFlash(Chipset.DFU))
                eepromResetDFU(mcu);
            if (usb.canFlash(Chipset.Caterina))
                eepromResetCaterina(mcu);
        }

        private void flashDFU(string mcu, string file) {
            string result;
            result = runProcess("dfu-programmer.exe", mcu + " erase --force");
            result = runProcess("dfu-programmer.exe", mcu + " flash \"" + file + "\"");
            if (result.Contains("Bootloader and code overlap.")) {
                printer.print("File is too large for device", MessageType.Error);
            } else {
                result = runProcess("dfu-programmer.exe", mcu + " reset");
            }
        }

        private void resetDFU(string mcu) {
            string result = runProcess("dfu-programmer.exe", mcu + " reset");
        }

        private void eepromResetDFU(string mcu) {
            string result;
            string file = mcu + "_eeprom_reset.hex";
            result = runProcess("dfu-programmer.exe", mcu + " erase --force");
            result = runProcess("dfu-programmer.exe", mcu + " flash --eeprom \"" + file + "\"");
            printer.print("Device has been erased - please reflash", MessageType.Bootloader);
        }

        private void flashCaterina(string mcu, string file) {
            string result = runProcess("avrdude.exe", "-p " + mcu + " -c avr109 -U flash:w:\"" + file + "\":i -P " + caterinaPort);
        }

        private void eepromResetCaterina(string mcu) {
            string file = mcu + "_eeprom_reset.hex";
            string result = runProcess("avrdude.exe", "-p " + mcu + " -c avr109 -U eeprom:w:\"" + file + "\":i -P " + caterinaPort);
        }

        private void flashHalfkay(string mcu, string file) {
            string result = runProcess("teensy_loader_cli.exe", "-mmcu=" + mcu + " \"" + file + "\" -v");
        }

        private void resetHalfkay(string mcu) {
            runProcess("teensy_loader_cli.exe", "-mmcu=" + mcu + " -bv");
        }

        private void flashSTM32(string mcu, string file) {
            runProcess("dfu-util.exe", "-a 0 -d 0483:df11 -s 0x08000000 -D \"" + file + "\"");
        }

        private void flashKiibohd(string file) {
            runProcess("dfu-util.exe", "-D \"" + file + "\"");
        }

        private void flashAVRISP(string mcu, string file) {
            string result = runProcess("avrdude.exe", "-p " + mcu + " -c avrisp -U flash:w:\"" + file + "\":i -P " + caterinaPort);
            printer.print("Flash complete", MessageType.Bootloader);
        }

        private void flashUSBTiny(string mcu, string file) {
            string result = runProcess("avrdude.exe", "-p " + mcu + " -c usbtiny -U flash:w:\"" + file + "\":i -P " + caterinaPort);
            printer.print("Flash complete", MessageType.Bootloader);
        }
    }
}
