//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox {
    public enum Chipset {
        DFU,
        Halfkay,
        Caterina,
        STM32,
        NumberOfChipsets
    };
    public class Flashing {
        private System.Diagnostics.Process process;
        private System.Diagnostics.ProcessStartInfo startInfo;

        public const ushort UsagePage = 0xFF31;
        public const int Usage = 0x0074;
        public string caterinaPort = "";

        private Form1 f;
        private Printing printer;

        string[] resources = {
            "dfu-programmer.exe",
            "avrdude.exe",
            "avrdude.conf",
            "teensy_loader_cli.exe",
            "dfu-util.exe",
            "libusb-1.0.dll",
            "mcu-list.txt",
            "atmega32u4_eeprom_reset.hex"
        };

        private void ExtractResource(string file) {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QMK_Toolbox." + file);
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            File.WriteAllBytes(Path.Combine(Application.LocalUserAppDataPath, file), bytes);
        }

        public Flashing(Form1 form, Printing printer) {
            this.f = form;
            this.printer = printer;
            
            foreach (string resource in resources) {
                ExtractResource(resource);
            }

            process = new System.Diagnostics.Process();
            startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
        }

        private string runProcess(string command, string args) {
            printer.print(command + " " + args, MessageType.Command);
            startInfo.WorkingDirectory = Application.LocalUserAppDataPath;
            startInfo.FileName = Path.Combine(Application.LocalUserAppDataPath, command);
            startInfo.Arguments = args;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            output += process.StandardError.ReadToEnd();
            printer.printResponse(output, MessageType.Command);
            return output;
        }

        public string[] getMCUList() {
            return File.ReadLines(Path.Combine(Application.LocalUserAppDataPath, "mcu-list.txt")).ToArray<string>();
        }

        public void flash(string mcu, string file) {
            if (f.canFlash(Chipset.DFU))
                flashDFU(mcu, file);
            if (f.canFlash(Chipset.Caterina))
                flashCaterina(mcu, file);
            if (f.canFlash(Chipset.Halfkay))
                flashHalfkay(mcu, file);
            if (f.canFlash(Chipset.STM32))
                flashSTM32(mcu, file);
        }

        public void reset(string mcu) {
            if (f.canFlash(Chipset.DFU))
                resetDFU(mcu);
            if (f.canFlash(Chipset.Halfkay))
                resetHalfkay(mcu);
        }

        public void eepromReset(string mcu) {
            if (f.canFlash(Chipset.DFU))
                eepromResetDFU(mcu);
            if (f.canFlash(Chipset.Caterina))
                eepromResetCaterina(mcu);
        }

        private void flashDFU(string mcu, string file) {
            string result;
            result = runProcess("dfu-programmer.exe", mcu + " erase --force");
            result = runProcess("dfu-programmer.exe", mcu + " flash " + file);
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
            result = runProcess("dfu-programmer.exe", mcu + " flash --eeprom " + file);
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
            runProcess("dfu-util.exe", "-a 0 -d 0483:df11 -s 0x08000000 -D " + file);
        }
    }

}
