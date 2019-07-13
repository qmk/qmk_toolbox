//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using QMK_Toolbox.Helpers;

namespace QMK_Toolbox
{
    public enum Chipset
    {
        Dfu,
        Halfkay,
        Caterina,
        Stm32,
        Kiibohd,
        Avrisp,
        Usbasp,
        UsbTiny,
        BootloadHID,
        AtmelSamBa,
        NumberOfChipsets
    };

    public class Flashing : EventArgs
    {
        private readonly Process _process;
        private readonly ProcessStartInfo _startInfo;

        public const ushort UsagePage = 0xFF31;
        public const int Usage = 0x0074;
        public string CaterinaPort = "";

        private readonly Printing _printer;
        public Usb Usb;

        private readonly string[] _resources = {
            "dfu-programmer.exe",
            "avrdude.exe",
            "avrdude.conf",
            "teensy_loader_cli.exe",
            "dfu-util.exe",
            "libusb-1.0.dll",
            "libusb0.dll", // x86/libusb0_x86.dll
            "mcu-list.txt",
            "reset.eep",
            "dfu-prog-usb-1.2.2.zip",
            "bootloadHID.exe",
            "mdloadler64.exe",
            "applet-flash-samd51j18a.bin"
        };

        

        public Flashing(Printing printer)
        {
            _printer = printer;
            EmbeddedResourceHelper.ExtractResources(_resources);

            /*
            var query = new System.Management.SelectQuery("Win32_SystemDriver") { Condition = "Name = 'libusb0'" };
            var searcher = new System.Management.ManagementObjectSearcher(query);
            var drivers = searcher.Get();

            if (drivers.Count > 0)
            {
                printer.Print("libusb0 driver found on system", MessageType.Info);
            }
            else
            {
                printer.Print("libusb0 driver not found - attempting to install", MessageType.Info);

                if (Directory.Exists(Path.Combine(Application.LocalUserAppDataPath, "dfu-prog-usb-1.2.2")))
                    Directory.Delete(Path.Combine(Application.LocalUserAppDataPath, "dfu-prog-usb-1.2.2"), true);
                ZipFile.ExtractToDirectory(Path.Combine(Application.LocalUserAppDataPath, "dfu-prog-usb-1.2.2.zip"), Application.LocalUserAppDataPath);

                var size = 0;
                var success = Program.SetupCopyOEMInf(Path.Combine(Application.LocalUserAppDataPath,
                        "dfu-prog-usb-1.2.2",
                        "atmel_usb_dfu.inf"),
                    "",
                    Program.OemSourceMediaType.SpostNone,
                    Program.OemCopyStyle.SpCopyNewer,
                    null,
                    0,
                    ref size,
                    null);
                if (!success)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    var errorString = new Win32Exception(errorCode).Message;
                    printer.Print("Error: " + errorString, MessageType.Error);
                }
            }
            */

            _process = new Process();
            //process.EnableRaisingEvents = true;
            //process.OutputDataReceived += OnOutputDataReceived;
            //process.ErrorDataReceived += OnErrorDataReceived;
            _startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.Write(e.Data);
            _printer.PrintResponse(e.Data, MessageType.Info);
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.Write(e.Data);
            _printer.PrintResponse(e.Data, MessageType.Info);
        }

        private void ProcessOutput(Object streamReader)
        {
            StreamReader _stream = (StreamReader)streamReader;
            var output = "";

            while (!_stream.EndOfStream)
            {
                output = _stream.ReadLine() + "\n";
                _printer.PrintResponse(output, MessageType.Info);

                if (output.Contains("Bootloader and code overlap.") || // DFU
                    output.Contains("exceeds remaining flash size!") || // BootloadHID
                    output.Contains("Not enough bytes in device info report")) // BootloadHID
                {
                    _printer.Print("File is too large for device", MessageType.Error);
                }
            }
        }
        private void RunProcess(string command, string args)
        {
            _printer.Print($"{command} {args}", MessageType.Command);
            _startInfo.WorkingDirectory = Application.LocalUserAppDataPath;
            _startInfo.FileName = Path.Combine(Application.LocalUserAppDataPath, command);
            _startInfo.Arguments = args;
            _startInfo.RedirectStandardOutput = true;
            _startInfo.RedirectStandardError = true;
            _startInfo.UseShellExecute = false;
            _process.StartInfo = _startInfo;

            _process.Start();

            // Thread that handles STDOUT
            Thread _ThreadProcessOutput = new Thread(ProcessOutput);
            _ThreadProcessOutput.Start(_process.StandardOutput);

            // Thread that handles STDERR
            _ThreadProcessOutput = new Thread(ProcessOutput);
            _ThreadProcessOutput.Start(_process.StandardError);

            _process.WaitForExit();
        }

        public string[] GetMcuList()
        {
            return File.ReadLines(Path.Combine(Application.LocalUserAppDataPath, "mcu-list.txt")).ToArray();
        }

        public void Flash(string mcu, string file)
        {
            if (Usb.CanFlash(Chipset.Dfu))
                FlashDfu(mcu, file);
            if (Usb.CanFlash(Chipset.Caterina))
                FlashCaterina(mcu, file);
            if (Usb.CanFlash(Chipset.Halfkay))
                FlashHalfkay(mcu, file);
            if (Usb.CanFlash(Chipset.Stm32))
                FlashStm32(mcu, file);
            if (Usb.CanFlash(Chipset.Kiibohd))
                FlashKiibohd(file);
            if (Usb.CanFlash(Chipset.Avrisp))
                FlashAvrisp(mcu, file);
            if (Usb.CanFlash(Chipset.Usbasp))
                FlashUsbasp(mcu, file);
            if (Usb.CanFlash(Chipset.UsbTiny))
                FlashUsbTiny(mcu, file);
            if (Usb.CanFlash(Chipset.BootloadHID))
                FlashBootloadHID(file);
            if (Usb.CanFlash(Chipset.AtmelSamBa))
                FlashAtmelSamBa(file);
        }

        public void Reset(string mcu)
        {
            if (Usb.CanFlash(Chipset.Dfu))
                ResetDfu(mcu);
            if (Usb.CanFlash(Chipset.Halfkay))
                ResetHalfkay(mcu);
            if (Usb.CanFlash(Chipset.BootloadHID))
                ResetBootloadHID();
        }

        public void EepromReset(string mcu)
        {
            if (Usb.CanFlash(Chipset.Dfu))
                EepromResetDfu(mcu);
            if (Usb.CanFlash(Chipset.Caterina))
                EepromResetCaterina(mcu);
        }

        private void FlashDfu(string mcu, string file)
        {
            RunProcess("dfu-programmer.exe", $"{mcu} erase --force");
            RunProcess("dfu-programmer.exe", $"{mcu} flash \"{file}\"");
            RunProcess("dfu-programmer.exe", $"{mcu} reset");
        }

        private void ResetDfu(string mcu) => RunProcess("dfu-programmer.exe", $"{mcu} reset");

        private void EepromResetDfu(string mcu) => RunProcess("dfu-programmer.exe", $"{mcu} flash --force --eeprom \"reset.eep\"");

        private void FlashCaterina(string mcu, string file) => RunProcess("avrdude.exe", $"-p {mcu} -c avr109 -U flash:w:\"{file}\":i -P {CaterinaPort}");

        private void EepromResetCaterina(string mcu) => RunProcess("avrdude.exe", $"-p {mcu} -c avr109 -U eeprom:w:\"reset.eep\":i -P {CaterinaPort}");

        private void FlashHalfkay(string mcu, string file) => RunProcess("teensy_loader_cli.exe", $"-mmcu={mcu} \"{file}\" -v");

        private void ResetHalfkay(string mcu) => RunProcess("teensy_loader_cli.exe", $"-mmcu={mcu} -bv");

        private void FlashStm32(string mcu, string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin") {
                RunProcess("dfu-util.exe", $"-a 0 -d 0483:df11 -s 0x08000000:leave -D \"{file}\"");
            } else {
                _printer.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        private void FlashKiibohd(string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin") {
                RunProcess("dfu-util.exe", $"-D \"{file}\"");
            } else {
                _printer.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        private void FlashAvrisp(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c avrisp -U flash:w:\"{file}\":i -P {CaterinaPort}");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashUsbasp(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c usbasp -U flash:w:\"{file}\":i");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashUsbTiny(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c usbtiny -U flash:w:\"{file}\":i -P {CaterinaPort}");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashBootloadHID(string file) => RunProcess("bootloadHID.exe", $"-r \"{file}\"");
        private void ResetBootloadHID() => RunProcess("bootloadHID.exe", $"-r");

        private void FlashAtmelSamBa(string file) => RunProcess("mdloader64.exe", $"-p {CaterinaPort} -D \"{file}\"");
    }
}
