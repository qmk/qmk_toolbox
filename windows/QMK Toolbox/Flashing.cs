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
        AvrIsp,
        UsbAsp,
        UsbTiny,
        BootloadHid,
        AtmelSamBa,
        NumberOfChipsets
    };
    
    public class Flashing : EventArgs, IFlashing
    {
        public const ushort UsagePage = 0xFF31;
        public const int Usage = 0x0074;

        public string CaterinaPort { get; set; } = string.Empty;

        private readonly IPrinting _printer;
        private readonly IProcessRunner _processRunner;

        public IUsb Usb { get; set; }

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

        public Flashing(IPrinting printer, IProcessRunner processRunner, bool extractResources = true)
        {
            _printer = printer;
            _processRunner = processRunner;

            if (extractResources)
            {
                // Allow this to be turned off for testing
                EmbeddedResourceHelper.ExtractResources(_resources);
            }
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
            if (Usb.CanFlash(Chipset.Dfu))
                ResetDfu(mcu);
            if (Usb.CanFlash(Chipset.Halfkay))
                ResetHalfkay(mcu);
            if (Usb.CanFlash(Chipset.BootloadHid))
                ResetBootloadHid();
            if (Usb.CanFlash(Chipset.AtmelSamBa))
                ResetAtmelSamBa();
        }

        public void ClearEeprom(string mcu)
        {
            if (Usb.CanFlash(Chipset.Dfu))
                ClearEepromDfu(mcu);
            if (Usb.CanFlash(Chipset.Caterina))
                ClearEepromCaterina(mcu);
            if (Usb.CanFlash(Chipset.UsbAsp))
                ClearEepromUsbAsp(mcu);
        }

        private void FlashDfu(string mcu, string file)
        {
            _processRunner.Run("dfu-programmer.exe", $"{mcu} erase --force");
            _processRunner.Run("dfu-programmer.exe", $"{mcu} flash \"{file}\"");
            _processRunner.Run("dfu-programmer.exe", $"{mcu} reset");
        }

        private void ResetDfu(string mcu) => _processRunner.Run("dfu-programmer.exe", $"{mcu} reset");

        private void ClearEepromDfu(string mcu) => _processRunner.Run("dfu-programmer.exe", $"{mcu} flash --force --eeprom \"reset.eep\"");

        private void FlashCaterina(string mcu, string file) => _processRunner.Run("avrdude.exe", $"-p {mcu} -c avr109 -U flash:w:\"{file}\":i -P {CaterinaPort}");

        private void ClearEepromCaterina(string mcu) => _processRunner.Run("avrdude.exe", $"-p {mcu} -c avr109 -U eeprom:w:\"reset.eep\":i -P {CaterinaPort}");

        private void ClearEepromUsbAsp(string mcu) => _processRunner.Run("avrdude.exe", $"-p {mcu} -c usbasp -U eeprom:w:\"reset.eep\":i");

        private void FlashHalfkay(string mcu, string file) => _processRunner.Run("teensy_loader_cli.exe", $"-mmcu={mcu} \"{file}\" -v");

        private void ResetHalfkay(string mcu) => _processRunner.Run("teensy_loader_cli.exe", $"-mmcu={mcu} -bv");

        private void FlashStm32(string mcu, string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                _processRunner.Run("dfu-util.exe", $"-a 0 -d 0483:df11 -s 0x08000000:leave -D \"{file}\"");
            }
            else
            {
                _printer.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        private void FlashKiibohd(string file)
        {
            if (Path.GetExtension(file)?.ToLower() == ".bin")
            {
                _processRunner.Run("dfu-util.exe", $"-D \"{file}\"");
            }
            else
            {
                _printer.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error);
            }
        }

        private void FlashAvrIsp(string mcu, string file)
        {
            _processRunner.Run("avrdude.exe", $"-p {mcu} -c avrisp -U flash:w:\"{file}\":i -P {CaterinaPort}");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashUsbAsp(string mcu, string file)
        {
            _processRunner.Run("avrdude.exe", $"-p {mcu} -c usbasp -U flash:w:\"{file}\":i");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashUsbTiny(string mcu, string file)
        {
            _processRunner.Run("avrdude.exe", $"-p {mcu} -c usbtiny -U flash:w:\"{file}\":i -P {CaterinaPort}");
            _printer.Print("Flash complete", MessageType.Bootloader);
        }

        private void FlashBootloadHid(string file) => _processRunner.Run("bootloadHID.exe", $"-r \"{file}\"");

        private void ResetBootloadHid() => _processRunner.Run("bootloadHID.exe", $"-r");

        private void FlashAtmelSamBa(string file) => _processRunner.Run("mdloader_windows.exe", $"-p {CaterinaPort} -D \"{file}\" --restart");

        private void ResetAtmelSamBa() => _processRunner.Run("mdloader_windows.exe", $"-p {CaterinaPort} --restart");
    }
}
