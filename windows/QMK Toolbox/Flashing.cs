//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Collections.Generic;
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
        AtmelDfu,
        Halfkay,
        Caterina,
        Stm32,
        Kiibohd,
        AvrIsp,
        UsbAsp,
        UsbTiny,
        BootloadHid,
        AtmelSamBa,
        NumberOfChipsets
    };

    public class Flashing : EventArgs
    {
        private readonly Process _process;
        private readonly ProcessStartInfo _startInfo;

        public const ushort ConsoleUsagePage = 0xFF31;
        public const int ConsoleUsage = 0x0074;
        public string ComPort = "";

        private readonly Printing _printer;
        public Usb Usb;

        private readonly string[] _resources = {
            "dfu-programmer.exe",
            "avrdude.exe",
            "avrdude.conf",
            "teensy_loader_cli.exe",
            "dfu-util.exe",
            "libusb-1.0.dll",
            "libusb0.dll",
            "mcu-list.txt",
            "reset.eep",
            "bootloadHID.exe",
            "mdloader_windows.exe",
            "applet-flash-samd51j18a.bin"
        };

        public Flashing(Printing printer)
        {
            _printer = printer;
            EmbeddedResourceHelper.ExtractResources(_resources);

            _process = new Process();
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
            if (Usb.CanFlash(Chipset.AtmelDfu))
                FlashAtmelDfu(mcu, file);
            if (Usb.CanFlash(Chipset.Caterina))
                FlashCaterina(mcu, file);
            if (Usb.CanFlash(Chipset.Halfkay))
                FlashHalfkay(mcu, file);
            if (Usb.CanFlash(Chipset.Stm32))
                FlashStm32(mcu, file);
            if (Usb.CanFlash(Chipset.Kiibohd))
                FlashKiibohd(file);
            if (Usb.CanFlash(Chipset.AvrIsp))
                FlashAvrIsp(mcu, file);
            if (Usb.CanFlash(Chipset.UsbAsp))
                FlashUsbAsp(mcu, file);
            if (Usb.CanFlash(Chipset.UsbTiny))
                FlashUsbTiny(mcu, file);
            if (Usb.CanFlash(Chipset.BootloadHid))
                FlashBootloadHid(file);
            if (Usb.CanFlash(Chipset.AtmelSamBa))
                FlashAtmelSamBa(file);
        }

        public void Reset(string mcu)
        {
            if (Usb.CanFlash(Chipset.AtmelDfu))
                ResetAtmelDfu(mcu);
            if (Usb.CanFlash(Chipset.Halfkay))
                ResetHalfkay(mcu);
            if (Usb.CanFlash(Chipset.BootloadHid))
                ResetBootloadHid();
            if (Usb.CanFlash(Chipset.AtmelSamBa))
                ResetAtmelSamBa();
        }

        public bool CanReset()
        {
            var resettable = new List<Chipset> {
                Chipset.AtmelDfu,
                Chipset.Halfkay,
                Chipset.BootloadHid,
                Chipset.AtmelSamBa
            };
            foreach (Chipset chipset in resettable)
            {
                if (Usb.CanFlash(chipset))
                    return true;
            }
            return false;
        }

        public void ClearEeprom(string mcu)
        {
            if (Usb.CanFlash(Chipset.AtmelDfu))
                ClearEepromAtmelDfu(mcu);
            if (Usb.CanFlash(Chipset.Caterina))
                ClearEepromCaterina(mcu);
            if (Usb.CanFlash(Chipset.UsbAsp))
                ClearEepromUsbAsp(mcu);
        }

        private void FlashAtmelDfu(string mcu, string file)
        {
            RunProcess("dfu-programmer.exe", $"{mcu} erase --force");
            RunProcess("dfu-programmer.exe", $"{mcu} flash --force \"{file}\"");
            RunProcess("dfu-programmer.exe", $"{mcu} reset");
        }

        private void ResetAtmelDfu(string mcu) => RunProcess("dfu-programmer.exe", $"{mcu} reset");

        private void ClearEepromAtmelDfu(string mcu)
        {
            RunProcess("dfu-programmer.exe", $"{mcu} erase --force");
            RunProcess("dfu-programmer.exe", $"{mcu} flash --force --eeprom \"reset.eep\"");
            _printer.Print("Please reflash device with firmware now", MessageType.Bootloader);
        }

        private void FlashCaterina(string mcu, string file) => RunProcess("avrdude.exe", $"-p {mcu} -c avr109 -U flash:w:\"{file}\":i -P {ComPort}");

        private void ClearEepromCaterina(string mcu) => RunProcess("avrdude.exe", $"-p {mcu} -c avr109 -U eeprom:w:\"reset.eep\":i -P {ComPort}");

        private void ClearEepromUsbAsp(string mcu) => RunProcess("avrdude.exe", $"-p {mcu} -c usbasp -U eeprom:w:\"reset.eep\":i");

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

        private void FlashAvrIsp(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c avrisp -U flash:w:\"{file}\":i -P {ComPort}");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashUsbAsp(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c usbasp -U flash:w:\"{file}\":i");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashUsbTiny(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c usbtiny -U flash:w:\"{file}\":i");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashBootloadHid(string file) => RunProcess("bootloadHID.exe", $"-r \"{file}\"");
        private void ResetBootloadHid() => RunProcess("bootloadHID.exe", $"-r");

        private void FlashAtmelSamBa(string file) => RunProcess("mdloader_windows.exe", $"-p {ComPort} -D \"{file}\" --restart");

        private void ResetAtmelSamBa() => RunProcess("mdloader_windows.exe", $"-p {ComPort} --restart");
    }
}
