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

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        static Mutex mutex = new Mutex(true, "{8F7F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
        [STAThread]
        static void Main(string[] args) {
            if (args.Length > 0) {

                AttachConsole(ATTACH_PARENT_PROCESS);

                Printing printer = new Printing();
                if (args.Length < 3) {
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
                } else {
                    printer.print("QMK Toolbox (http://qmk.fm/toolbox)", MessageType.Info);
                    Flashing flasher = new Flashing(printer);
                    USB usb = new USB(flasher, printer);
                    flasher.usb = usb;

                    ManagementObjectCollection collection;
                    using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                        collection = searcher.Get();

                    usb.DetectBootloaderFromCollection(collection);

                    if (args[0].Equals("flash")) {
                        var mcu = args[1];
                        var filepath = args[2];
                        flasher.flash(mcu, filepath);
                    } else if (args[0].Equals("list")) {

                    } else if (args[0].Equals("eepromReset")) {

                    }

                }

                FreeConsole();
            } else {
                if (mutex.WaitOne(TimeSpan.Zero, true)) {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(args.Length == 0 ? new Form1(string.Empty) : new Form1(args[0]));
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
