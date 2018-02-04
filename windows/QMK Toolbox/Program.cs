using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///     

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupCopyOEMInf(

        string SourceInfFileName,
        string OEMSourceMediaLocation,
        OemSourceMediaType OEMSourceMediaType,
        OemCopyStyle CopyStyle,
        string DestinationInfFileName,
        int DestinationInfFileNameSize,
        ref int RequiredSize,
        string DestinationInfFileNameComponent

        );

        /// <summary>
        /// Driver media type
        /// </summary>
        internal enum OemSourceMediaType {
            SPOST_NONE = 0,
            //Only use the following if you have a pnf file as well
            SPOST_PATH = 1,
            SPOST_URL = 2,
            SPOST_MAX = 3
        }

        internal enum OemCopyStyle {
            SP_COPY_NEWER = 0x0000004,   // copy only if source newer than or same as target
            SP_COPY_NEWER_ONLY = 0x0010000,   // copy only if source file newer than target
            SP_COPY_OEMINF_CATALOG_ONLY = 0x0040000,   // (SetupCopyOEMInf only) don't copy INF--just catalog
        }

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        static Mutex mutex = new Mutex(true, "{8F7F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
        [STAThread]
        static void Main(string[] args) {
            if (mutex.WaitOne(TimeSpan.Zero, true) && args.Length > 0) {

                AttachConsole(ATTACH_PARENT_PROCESS);

                Printing printer = new Printing();
                if (args[0].Equals("list")) {
                    Flashing flasher = new Flashing(printer);
                    USB usb = new USB(flasher, printer);
                    flasher.usb = usb;

                    ManagementObjectCollection collection;
                    using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                        collection = searcher.Get();

                    usb.DetectBootloaderFromCollection(collection);
                    FreeConsole();
                    Environment.Exit(0);
                }

                if (args[0].Equals("flash")) {
                    Flashing flasher = new Flashing(printer);
                    USB usb = new USB(flasher, printer);
                    flasher.usb = usb;

                    ManagementObjectCollection collection;
                    using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                        collection = searcher.Get();

                    usb.DetectBootloaderFromCollection(collection);

                    if (usb.areDevicesAvailable()) {
                        var mcu = args[1];
                        var filepath = args[2];
                        printer.print("Attempting to flash, please don't remove device", MessageType.Bootloader);
                        flasher.flash(mcu, filepath);
                        FreeConsole();
                        Environment.Exit(0);
                    } else {
                        printer.print("There are no devices available", MessageType.Error);
                        FreeConsole();
                        Environment.Exit(1);
                    }
                }

                if (args[0].Equals("help")) {
                    printer.print("QMK Toolbox (http://qmk.fm/toolbox)", MessageType.Info);
                    printer.printResponse("Supporting following bootloaders:\n", MessageType.Info);
                    printer.printResponse(" - DFU (Atmel, LUFA) via dfu-programmer (http://dfu-programmer.github.io/)\n", MessageType.Info);
                    printer.printResponse(" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)\n", MessageType.Info);
                    printer.printResponse(" - Halfkay (Teensy, Ergodox EZ) via teensy_loader_cli (https://pjrc.com/teensy/loader_cli.html)\n", MessageType.Info);
                    printer.printResponse(" - STM32 (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
                    printer.printResponse(" - Kiibohd (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
                    printer.printResponse("And the following ISP flasher protocols:\n", MessageType.Info);
                    printer.printResponse(" - USBTiny (AVR Pocket)\n", MessageType.Info);
                    printer.printResponse(" - AVRISP (Arduino ISP)\n", MessageType.Info);
                    printer.printResponse("usage: qmk_toolbox.exe <mcu> <filepath>", MessageType.Info);
                    FreeConsole();
                    Environment.Exit(0);
                }

                printer.print("Command not found - use \"help\" for all commands", MessageType.Error);
                FreeConsole();
                Environment.Exit(1);
            } else {
                if (mutex.WaitOne(TimeSpan.Zero, true)) {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(args.Length == 0 ? new MainWindow(string.Empty) : new MainWindow(args[0]));
                    mutex.ReleaseMutex();
                } else {
                    // send our Win32 message to make the currently running instance
                    // jump on top of all the other windows
                    if (args.Length > 0) {
                        using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), "qmk_toolbox/file_passed_in.txt"))) {
                            sw.WriteLine(args[0]);
                        }
                    }
                    NativeMethods.PostMessage(
                        (IntPtr)NativeMethods.HWND_BROADCAST,
                        NativeMethods.WM_SHOWME,
                        IntPtr.Zero,
                        IntPtr.Zero);
                }
            }
        }
    }

    // this class just wraps some Win32 stuff that we're going to use
    internal class NativeMethods {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }
}
