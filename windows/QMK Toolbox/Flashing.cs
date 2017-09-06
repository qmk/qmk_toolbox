using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox {
    public class Flashing {
        private System.Diagnostics.Process process_dfu;
        private System.Diagnostics.Process process_avrdude;
        private System.Diagnostics.Process process_teensy;
        private System.Diagnostics.Process process_dfuUtil;
        private System.Diagnostics.ProcessStartInfo startInfo_dfu;
        private System.Diagnostics.ProcessStartInfo startInfo_avrdude;
        private System.Diagnostics.ProcessStartInfo startInfo_teensy;
        private System.Diagnostics.ProcessStartInfo startInfo_dfuUtil;

        private string dfuPath;
        private string avrdudePath;
        private string avrdudeConfPath;
        private string teensyPath;
        private string dfuUtilPath;
        private string libusbPath;
        public string mcuListPath;

        public const ushort UsagePage = 0xFF31;
        public const int Usage = 0x0074;
        public string _COM = "";
        public bool dfuAvailable = false;
        public bool caterinaAvailable = false;
        public bool halfkayAvailable = false;
        public bool dfuUtilAvailable = false;

        private Form1 f;

        private void ExtractResource(string resource, string path) {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            File.WriteAllBytes(path, bytes);
        }

        public Flashing(Form1 form) {
            f = form;

            dfuPath = Application.LocalUserAppDataPath + "\\dfu-programmer.exe";
            ExtractResource("QMK_Toolbox.dfu-programmer.exe", dfuPath);

            avrdudePath = Application.LocalUserAppDataPath + "\\avrdude.exe";
            ExtractResource("QMK_Toolbox.avrdude.exe", avrdudePath);
            avrdudeConfPath = Application.LocalUserAppDataPath + "\\avrdude.conf";
            ExtractResource("QMK_Toolbox.avrdude.conf", avrdudeConfPath);

            teensyPath = Application.LocalUserAppDataPath + "\\teensy_loader_cli.exe";
            ExtractResource("QMK_Toolbox.teensy_loader_cli.exe", teensyPath);

            dfuUtilPath = Application.LocalUserAppDataPath + "\\dfu-util.exe";
            ExtractResource("QMK_Toolbox.dfu-util.exe", dfuUtilPath);

            libusbPath = Application.LocalUserAppDataPath + "\\libusb-1.0.dll";
            ExtractResource("QMK_Toolbox.libusb-1.0.dll", libusbPath);

            mcuListPath = Application.LocalUserAppDataPath + "\\mcu-list.txt";
            ExtractResource("QMK_Toolbox.mcu-list.txt", mcuListPath);
        }

        public void SetupDFU() {
            process_dfu = new System.Diagnostics.Process();
            startInfo_dfu = new System.Diagnostics.ProcessStartInfo();
            startInfo_dfu.UseShellExecute = false;
            startInfo_dfu.RedirectStandardError = true;
            startInfo_dfu.RedirectStandardOutput = true;
            startInfo_dfu.FileName = dfuPath;
            startInfo_dfu.CreateNoWindow = true;


            //f.P("dfu-programmer --version", MessageType.Command);
            //startInfo_dfu.Arguments = "--version";
            //process_dfu.StartInfo = startInfo_dfu;
            //process_dfu.Start();
            //string output = process_dfu.StandardOutput.ReadToEnd();
            //output += process_dfu.StandardError.ReadToEnd();
            //process_dfu.WaitForExit();
            //f.R(output, MessageType.Command);
        }
        
        public void SetupAvrdude() {
            process_avrdude = new System.Diagnostics.Process();
            startInfo_avrdude = new System.Diagnostics.ProcessStartInfo();
            startInfo_avrdude.UseShellExecute = false;
            startInfo_avrdude.RedirectStandardError = true;
            startInfo_avrdude.RedirectStandardOutput = true;
            startInfo_avrdude.FileName = avrdudePath;
            startInfo_avrdude.CreateNoWindow = true;


            //f.P("avrdude -v", MessageType.Command);
            //startInfo_avrdude.Arguments = "-v";
            //process_avrdude.StartInfo = startInfo_avrdude;
            //process_avrdude.Start();
            //string output = process_avrdude.StandardOutput.ReadToEnd();
            //output += process_avrdude.StandardError.ReadToEnd();
            //output = string.Join("\n", output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Take(4));
            //process_avrdude.WaitForExit();
            //f.R(output.Substring(1) + "\n", MessageType.Command);
        }

        public void SetupTeensy() {
            process_teensy = new System.Diagnostics.Process();
            startInfo_teensy = new System.Diagnostics.ProcessStartInfo();
            startInfo_teensy.UseShellExecute = false;
            startInfo_teensy.RedirectStandardError = true;
            startInfo_teensy.RedirectStandardOutput = true;
            startInfo_teensy.FileName = teensyPath;
            startInfo_teensy.CreateNoWindow = true;


            //f.P("teensy_loader_cli --list-mcus", MessageType.Command);
            //startInfo_teensy.Arguments = "--list-mcus";
            //process_teensy.StartInfo = startInfo_teensy;
            //process_teensy.Start();
            //string output = process_teensy.StandardOutput.ReadToEnd();
            //output += process_teensy.StandardError.ReadToEnd();
            ////output = string.Join("\n", output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Take(4));
            //process_teensy.WaitForExit();
            //f.R(output.Substring(0) + "\n", MessageType.Command);
        }

        public void SetupDfuUtil() {
            process_dfuUtil = new System.Diagnostics.Process();
            startInfo_dfuUtil = new System.Diagnostics.ProcessStartInfo();
            startInfo_dfuUtil.UseShellExecute = false;
            startInfo_dfuUtil.RedirectStandardError = true;
            startInfo_dfuUtil.RedirectStandardOutput = true;
            startInfo_dfuUtil.FileName = dfuUtilPath;
            startInfo_dfuUtil.CreateNoWindow = true;


            //f.P("dfu-util -V", MessageType.Command);
            //startInfo_dfuUtil.Arguments = "-V";
            //process_dfuUtil.StartInfo = startInfo_dfuUtil;
            //process_dfuUtil.Start();
            //string output = process_dfuUtil.StandardOutput.ReadToEnd();
            //output += process_dfuUtil.StandardError.ReadToEnd();
            //output = string.Join("\n", output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Take(6));
            //process_dfuUtil.WaitForExit();
            //f.R(output.Substring(0) + "\n", MessageType.Command);
        }


        public void Flash() {
            f.P("Attempting to flash - please don't remove device", MessageType.Bootloader);
            string hexFile = f.getHexFile() ;
            if (caterinaAvailable && _COM != "") {
                if (f.getTarget() == "") {
                    f.P("Please select an MCU", MessageType.Error);
                } else if (hexFile == "") {
                    f.P("Please select a .hex file", MessageType.Error);
                } else {
                    RunAvrdude(hexFile);
                }
                caterinaAvailable = false;
            } else if (dfuAvailable) {
                if (f.getTarget() == "") {
                    f.P("Please select an MCU", MessageType.Error);
                } else if (hexFile == "") {
                    f.P("Please select a .hex file", MessageType.Error);
                    RunDFU("reset");
                    dfuAvailable = false;
                } else {
                    if (mcuIsAvailable()) {
                        RunDFU("erase --force");
                        if (RunDFU("flash " + hexFile)) {
                            RunDFU("reset");
                            dfuAvailable = false;
                        }
                    }
                }
            } else if (halfkayAvailable) {
                if (f.getTarget() == "") {
                    f.P("Please select an MCU", MessageType.Error);
                } else if (hexFile == "") {
                    f.P("Please select a .hex file", MessageType.Error);
                    TeensyReset();
                    halfkayAvailable = false;
                } else {
                    TeensyFlash(hexFile);
                    halfkayAvailable = false;
                }
            } else if (dfuUtilAvailable) {
                if (f.getTarget() == "") {
                    f.P("Please select an MCU", MessageType.Error);
                } else if (hexFile == "") {
                    f.P("Please select a .hex file", MessageType.Error);
                } else {
                    RunDfuUtil(hexFile);
                    halfkayAvailable = false;
                }
            } else {
                f.P("No flashable device connected", MessageType.Error);
            }
        }

        public void Reset() {
            if (f.getTarget() == "") {
                f.P("Please select an MCU", MessageType.Error);
            } else if (dfuAvailable && mcuIsAvailable()) {
                RunDFU("reset");
            } else if (halfkayAvailable) {
                TeensyReset();
            } else {
                f.P("No resettable device connected", MessageType.Error);
            }
        }

        private bool RunDFU(string dfuArgs) {
            bool status = true;
            f.P("dfu-programmer " + f.getTarget() + " " + dfuArgs, MessageType.Command);
            startInfo_dfu.Arguments = f.getTarget() + " " + dfuArgs;
            process_dfu.StartInfo = startInfo_dfu;
            process_dfu.Start();
            string output = process_dfu.StandardOutput.ReadToEnd();
            output += process_dfu.StandardError.ReadToEnd();
            if (output.Contains("Bootloader and code overlap.")) {
                status = false;
            }
            process_dfu.WaitForExit();
            f.R(output, MessageType.Command);
            return status;
        }

        private void RunAvrdude(string file) {
            f.P("avrdude -p " + f.getTarget() + " -c avr109 -U flash:w:\"" + file + "\" -P " + _COM, MessageType.Command);
            startInfo_avrdude.Arguments = "-p " + f.getTarget() + " -c avr109 -U flash:w:\"" + file + "\":i -P " + _COM;
            process_avrdude.StartInfo = startInfo_avrdude;
            process_avrdude.Start();
            string output = process_avrdude.StandardOutput.ReadToEnd();
            output += process_avrdude.StandardError.ReadToEnd();
            process_avrdude.WaitForExit();
            f.R(output, MessageType.Command);
        }

        private void TeensyFlash(string file) {
            f.P("teensy_loader_cli -mmcu=" + f.getTarget() + " \"" + file + "\" -v", MessageType.Command);
            startInfo_teensy.Arguments = "-mmcu=" + f.getTarget() + " \"" + file + "\" -v";
            process_teensy.StartInfo = startInfo_teensy;
            process_teensy.Start();
            string output = process_teensy.StandardOutput.ReadToEnd();
            output += process_teensy.StandardError.ReadToEnd();
            process_teensy.WaitForExit();
            f.R(output, MessageType.Command);
        }

        private void TeensyReset() {
            f.P("teensy_loader_cli -mmcu=" + f.getTarget() + " -bv", MessageType.Command);
            startInfo_teensy.Arguments = "-mmcu=" + f.getTarget() + " -bv";
            process_teensy.StartInfo = startInfo_teensy;
            process_teensy.Start();
            string output = process_teensy.StandardOutput.ReadToEnd();
            output += process_teensy.StandardError.ReadToEnd();
            process_teensy.WaitForExit();
            f.R(output, MessageType.Command);
        }

        private void RunDfuUtil(string file) {
            f.P("dfu-util-a 0 -d 0483:df11 -s 0x08000000 -D " + file, MessageType.Command);
            startInfo_dfuUtil.Arguments = "-a 0 -d 0483:df11 -s 0x08000000 -D " + file;
            process_dfuUtil.StartInfo = startInfo_dfuUtil;
            process_dfuUtil.Start();
            string output = process_dfuUtil.StandardOutput.ReadToEnd();
            output += process_dfuUtil.StandardError.ReadToEnd();
            process_dfuUtil.WaitForExit();
            f.R(output, MessageType.Command);
        }

        private bool mcuIsAvailable() {
            startInfo_dfu.Arguments = f.getTarget() + " read";
            process_dfu.StartInfo = startInfo_dfu;
            process_dfu.Start();
            string output = process_dfu.StandardOutput.ReadToEnd();
            output += process_dfu.StandardError.ReadToEnd();
            process_dfu.WaitForExit();
            if (output.Contains("no device present")) {
                f.P("No \"" + f.getTarget() + "\" DFU device connected", MessageType.Error);
                return false;
            } else {
                return true;
            }
        }

    }
}
