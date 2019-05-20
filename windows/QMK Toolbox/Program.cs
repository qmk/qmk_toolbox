using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        ///

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupCopyOEMInf(

        string sourceInfFileName,
        string oemSourceMediaLocation,
        OemSourceMediaType oemSourceMediaType,
        OemCopyStyle copyStyle,
        string destinationInfFileName,
        int destinationInfFileNameSize,
        ref int requiredSize,
        string destinationInfFileNameComponent

        );

        /// <summary>
        /// Driver media type
        /// </summary>
        internal enum OemSourceMediaType
        {
            SpostNone = 0,

            //Only use the following if you have a pnf file as well
            SpostPath = 1,

            SpostUrl = 2,
            SpostMax = 3
        }

        internal enum OemCopyStyle
        {
            SpCopyNewer = 0x0000004,   // copy only if source newer than or same as target
            SpCopyNewerOnly = 0x0010000,   // copy only if source file newer than target
            SpCopyOeminfCatalogOnly = 0x0040000,   // (SetupCopyOEMInf only) don't copy INF--just catalog
        }

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int AttachParentProcess = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        private static readonly Mutex Mutex = new Mutex(true, "{8F7F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        [STAThread]
        private static void Main(string[] args)
        {
            if (Mutex.WaitOne(TimeSpan.Zero, true) && args.Length > 0)
            {
                AttachConsole(AttachParentProcess);

                var printer = new Printing();
                if (args[0].Equals("list"))
                {
                    var flasher = new Flashing(printer);
                    var usb = new Usb(flasher, printer);
                    flasher.Usb = usb;

                    ManagementObjectCollection collection;
                    using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                        collection = searcher.Get();

                    usb.DetectBootloaderFromCollection(collection);
                    FreeConsole();
                    Environment.Exit(0);
                }

                if (args[0].Equals("flash"))
                {
                    var flasher = new Flashing(printer);
                    var usb = new Usb(flasher, printer);
                    flasher.Usb = usb;

                    ManagementObjectCollection collection;
                    using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                        collection = searcher.Get();

                    usb.DetectBootloaderFromCollection(collection);

                    if (usb.AreDevicesAvailable())
                    {
                        var mcu = args[1];
                        var filepath = args[2];
                        printer.Print("Attempting to flash, please don't remove device", MessageType.Bootloader);
                        flasher.Flash(mcu, filepath);
                        FreeConsole();
                        Environment.Exit(0);
                    }
                    else
                    {
                        printer.Print("There are no devices available", MessageType.Error);
                        FreeConsole();
                        Environment.Exit(1);
                    }
                }

                if (args[0].Equals("help"))
                {
                    printer.Print("QMK Toolbox (http://qmk.fm/toolbox)", MessageType.Info);
                    printer.PrintResponse("Supporting following bootloaders:\n", MessageType.Info);
                    printer.PrintResponse(" - DFU (Atmel, LUFA) via dfu-programmer (http://dfu-programmer.github.io/)\n", MessageType.Info);
                    printer.PrintResponse(" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)\n", MessageType.Info);
                    printer.PrintResponse(" - Halfkay (Teensy, Ergodox EZ) via teensy_loader_cli (https://pjrc.com/teensy/loader_cli.html)\n", MessageType.Info);
                    printer.PrintResponse(" - STM32 (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
                    printer.PrintResponse(" - Kiibohd (ARM) via dfu-util (http://dfu-util.sourceforge.net/)\n", MessageType.Info);
                    printer.PrintResponse(" - BootloadHID (Atmel, ps2avrGB, CA66) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)\n", MessageType.Info);
                    printer.PrintResponse("And the following ISP flasher protocols:\n", MessageType.Info);
                    printer.PrintResponse(" - USBTiny (AVR Pocket)\n", MessageType.Info);
                    printer.PrintResponse(" - AVRISP (Arduino ISP)\n", MessageType.Info);
                    printer.PrintResponse("usage: qmk_toolbox.exe <mcu> <filepath>", MessageType.Info);
                    printer.PrintResponse(" - USBASP (AVR ISP)\n", MessageType.Info);
                    FreeConsole();
                    Environment.Exit(0);
                }

                printer.Print("Command not found - use \"help\" for all commands", MessageType.Error);
                FreeConsole();
                Environment.Exit(1);
            }
            else
            {
                if (Mutex.WaitOne(TimeSpan.Zero, true))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(args.Length == 0 ? new MainWindow(string.Empty) : new MainWindow(args[0]));
                    Mutex.ReleaseMutex();
                }
                else
                {
                    // send our Win32 message to make the currently running instance
                    // jump on top of all the other windows
                    if (args.Length > 0)
                    {
                        using (var sw = new StreamWriter(Path.Combine(Path.GetTempPath(), "qmk_toolbox/file_passed_in.txt")))
                        {
                            sw.WriteLine(args[0]);
                        }
                    }
                    NativeMethods.PostMessage(
                        (IntPtr)NativeMethods.HwndBroadcast,
                        NativeMethods.WmShowme,
                        IntPtr.Zero,
                        IntPtr.Zero);
                }
            }
        }
    }

    // this class just wraps some Win32 stuff that we're going to use
    internal class NativeMethods
    {
        public const int HwndBroadcast = 0xffff;
        public static readonly int WmShowme = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }
}
