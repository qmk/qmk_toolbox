using QMK_Toolbox.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public enum Chipset
    {
        Apm32Dfu,
        AtmelDfu,
        AtmelSamBa,
        AvrIsp,
        BootloadHid,
        Caterina,
        Halfkay,
        Kiibohd,
        LufaMs,
        QmkDfu,
        Stm32Dfu,
        Stm32Duino,
        UsbAsp,
        UsbTiny,
        NumberOfChipsets
    };

    public class Flashing : EventArgs
    {
        private readonly Process _process;
        private readonly ProcessStartInfo _startInfo;

        public string ComPort = "";
        public string MountPoint = "";

        private readonly Printing _printer;
        public Usb Usb;

        private readonly string[] _resources = {
            "avrdude.conf",
            "reset.eep",
            "reset_left.eep",
            "reset_right.eep",
            "avrdude.exe",
            "bootloadHID.exe",
            "dfu-programmer.exe",
            "dfu-util.exe",
            "mdloader.exe",
            "teensy_loader_cli.exe",
            "libftdi1.dll",
            "libusb0.dll",
            "libusb-0-1-4.dll",
            "libusb-1.0.dll",
            "libwinpthread-1.dll"
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

        private void ProcessOutput(object streamReader)
        {
            StreamReader _stream = (StreamReader)streamReader;
            string output;

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

        public void Flash(string mcu, string file)
        {
            if (Usb.CanFlash(Chipset.AtmelDfu) || Usb.CanFlash(Chipset.QmkDfu))
                FlashAtmelDfu(mcu, file);
            if (Usb.CanFlash(Chipset.Caterina))
                FlashCaterina(mcu, file);
            if (Usb.CanFlash(Chipset.Halfkay))
                FlashHalfkay(mcu, file);
            if (Usb.CanFlash(Chipset.Stm32Dfu))
                FlashStm32Dfu(file);
            if (Usb.CanFlash(Chipset.Apm32Dfu))
                FlashApm32Dfu(file);
            if (Usb.CanFlash(Chipset.Kiibohd))
                FlashKiibohd(file);
            if (Usb.CanFlash(Chipset.LufaMs))
                FlashLufaMs(file);
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
            if (Usb.CanFlash(Chipset.Stm32Duino))
                FlashStm32Duino(file);
        }

        public void Reset(string mcu)
        {
            if (Usb.CanFlash(Chipset.AtmelDfu) || Usb.CanFlash(Chipset.QmkDfu))
                ResetAtmelDfu(mcu);
            if (Usb.CanFlash(Chipset.Halfkay))
                ResetHalfkay(mcu);
            if (Usb.CanFlash(Chipset.BootloadHid))
                ResetBootloadHid();
            if (Usb.CanFlash(Chipset.AtmelSamBa))
                ResetAtmelSamBa();
        }

        public void ClearEeprom(string mcu)
        {
            if (Usb.CanFlash(Chipset.AtmelDfu) || Usb.CanFlash(Chipset.QmkDfu))
                ClearEepromAtmelDfu(mcu, !Usb.CanFlash(Chipset.QmkDfu));
            if (Usb.CanFlash(Chipset.Caterina))
                ClearEepromCaterina(mcu);
            if (Usb.CanFlash(Chipset.UsbAsp))
                ClearEepromUsbAsp(mcu);
        }

        public void SetHandedness(string mcu, bool rightHand)
        {
            if (Usb.CanFlash(Chipset.AtmelDfu) || Usb.CanFlash(Chipset.QmkDfu))
                SetHandednessAtmelDfu(mcu, rightHand, !Usb.CanFlash(Chipset.QmkDfu));
            if (Usb.CanFlash(Chipset.Caterina))
                SetHandednessCaterina(mcu, rightHand);
            if (Usb.CanFlash(Chipset.UsbAsp))
                SetHandednessUsbAsp(mcu, rightHand);
        }

        public bool CanFlash() => Usb.AreDevicesAvailable();

        public bool CanReset()
        {
            var resettable = new List<Chipset> {
                Chipset.AtmelDfu,
                Chipset.AtmelSamBa,
                Chipset.BootloadHid,
                Chipset.Halfkay,
                Chipset.QmkDfu
            };
            foreach (Chipset chipset in resettable)
            {
                if (Usb.CanFlash(chipset))
                    return true;
            }
            return false;
        }

        public bool CanClearEeprom()
        {
            var clearable = new List<Chipset>
            {
                Chipset.AtmelDfu,
                Chipset.Caterina,
                Chipset.QmkDfu,
                Chipset.UsbAsp
            };
            foreach (Chipset chipset in clearable)
            {
                if (Usb.CanFlash(chipset))
                    return true;
            }
            return false;
        }

        private void FlashApm32Dfu(string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                RunProcess("dfu-util.exe", $"-a 0 -d 314B:0106 -s 0x08000000:leave -D \"{file}\"");
            }
            else
            {
                _printer.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        private void FlashAtmelDfu(string mcu, string file)
        {
            RunProcess("dfu-programmer.exe", $"{mcu} erase --force");
            RunProcess("dfu-programmer.exe", $"{mcu} flash --force \"{file}\"");
            RunProcess("dfu-programmer.exe", $"{mcu} reset");
        }

        private void ResetAtmelDfu(string mcu) => RunProcess("dfu-programmer.exe", $"{mcu} reset");

        private void ClearEepromAtmelDfu(string mcu, bool erase)
        {
            if (erase)
            {
                RunProcess("dfu-programmer.exe", $"{mcu} erase --force");
            }
            RunProcess("dfu-programmer.exe", $"{mcu} flash --force --suppress-validation --eeprom reset.eep");
            if (erase)
            {
                _printer.Print("Please reflash device with firmware now", MessageType.Bootloader);
            }
        }

        private void SetHandednessAtmelDfu(string mcu, bool rightHand, bool erase)
        {
            if (erase)
            {
                RunProcess("dfu-programmer.exe", $"{mcu} erase --force");
            }
            RunProcess("dfu-programmer.exe", $"{mcu} flash --force --suppress-validation --eeprom reset_{(rightHand ? "right" : "left")}.eep");
            if (erase)
            {
                _printer.Print("Please reflash device with firmware now", MessageType.Bootloader);
            }
        }

        private void FlashAtmelSamBa(string file) => RunProcess("mdloader.exe", $"-p {ComPort} -D \"{file}\" --restart");

        private void ResetAtmelSamBa() => RunProcess("mdloader.exe", $"-p {ComPort} --restart");

        private void FlashAvrIsp(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c avrisp -U flash:w:\"{file}\":i -P {ComPort}");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashBootloadHid(string file) => RunProcess("bootloadHID.exe", $"-r \"{file}\"");

        private void ResetBootloadHid() => RunProcess("bootloadHID.exe", $"-r");

        private void FlashCaterina(string mcu, string file) => RunProcess("avrdude.exe", $"-p {mcu} -c avr109 -U flash:w:\"{file}\":i -P {ComPort}");

        private void ClearEepromCaterina(string mcu) => RunProcess("avrdude.exe", $"-p {mcu} -c avr109 -U eeprom:w:reset.eep:i -P {ComPort}");

        private void SetHandednessCaterina(string mcu, bool rightHand) => RunProcess("avrdude.exe", $"-p {mcu} -c avr109 -U eeprom:w:reset_{(rightHand ? "right" : "left")}.eep:i -P {ComPort}");

        private void FlashHalfkay(string mcu, string file) => RunProcess("teensy_loader_cli.exe", $"-mmcu={mcu} \"{file}\" -v");

        private void ResetHalfkay(string mcu) => RunProcess("teensy_loader_cli.exe", $"-mmcu={mcu} -bv");

        private void FlashKiibohd(string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                RunProcess("dfu-util.exe", $"-D \"{file}\"");
            }
            else
            {
                _printer.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        private void FlashLufaMs(string file)
        {
            if (MountPoint != null)
            {
                if (Path.GetExtension(file)?.ToLower() == ".bin")
                {
                    var destFile = $"{MountPoint}\\FLASH.BIN";

                    try
                    {
                        _printer.Print($"Deleting {destFile}...", MessageType.Command);
                        File.Delete(destFile);

                        _printer.Print($"Copying {file} to {destFile}...", MessageType.Command);
                        File.Copy(file, destFile);

                        _printer.Print("Done, please eject drive now.", MessageType.Info);
                    }
                    catch (IOException e)
                    {
                        _printer.Print($"IO ERROR: {e.Message}", MessageType.Error);
                    }
                }
                else
                {
                    _printer.Print("Only firmware files in .bin format can be flashed with this bootloader!", MessageType.Error);
                }
            }
            else
            {
                _printer.Print("Could not find drive letter for device!", MessageType.Error);
            }
        }

        private void FlashStm32Dfu(string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                RunProcess("dfu-util.exe", $"-a 0 -d 0483:DF11 -s 0x08000000:leave -D \"{file}\"");
            }
            else
            {
                _printer.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        private void FlashStm32Duino(string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                RunProcess("dfu-util.exe", $"-a 2 -d 1EAF:0003 -R -D \"{file}\"");
            }
            else
            {
                _printer.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        private void FlashUsbAsp(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c usbasp -U flash:w:\"{file}\":i");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void ClearEepromUsbAsp(string mcu) => RunProcess("avrdude.exe", $"-p {mcu} -c usbasp -U eeprom:w:reset.eep:i");

        private void SetHandednessUsbAsp(string mcu, bool rightHand) => RunProcess("avrdude.exe", $"-p {mcu} -c usbasp -U eeprom:w:reset_{(rightHand ? "right" : "left")}.eep:i");

        private void FlashUsbTiny(string mcu, string file)
        {
            RunProcess("avrdude.exe", $"-p {mcu} -c usbtiny -U flash:w:\"{file}\":i");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }
    }
}
