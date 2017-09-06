using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        static Mutex mutex = new Mutex(true, "{8F7F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
        [STAThread]
        static void Main(string[] args) {
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
