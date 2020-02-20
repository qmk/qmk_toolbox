//  Created by Mike Cooper on 2/20/20.
//  Copyright © 2020 Mike Cooper. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class ProcessRunner : IProcessRunner
    {
        private readonly Process process;
        private readonly ProcessStartInfo startInfo;
        private readonly IPrinting printer;

        public ProcessRunner(IPrinting printer)
        {
            this.printer = printer;

            process = new Process();
            startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };
        }

        public void Run(string command, string args)
        {
            printer.Print($"{command} {args}", MessageType.Command);
            startInfo.WorkingDirectory = Application.LocalUserAppDataPath;
            startInfo.FileName = Path.Combine(Application.LocalUserAppDataPath, command);
            startInfo.Arguments = args;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;

            process.Start();

            // Thread that handles STDOUT
            Thread processOutputThread = new Thread(ProcessOutput);
            processOutputThread.Start(process.StandardOutput);

            // Thread that handles STDERR
            processOutputThread = new Thread(ProcessOutput);
            processOutputThread.Start(process.StandardError);

            process.WaitForExit();
        }

        private void ProcessOutput(Object streamReader)
        {
            StreamReader _stream = (StreamReader)streamReader;
            var output = "";

            while (!_stream.EndOfStream)
            {
                output = _stream.ReadLine() + "\n";
                printer.PrintResponse(output, MessageType.Info);

                if (output.Contains("Bootloader and code overlap.") || // DFU
                    output.Contains("exceeds remaining flash size!") || // BootloadHID
                    output.Contains("Not enough bytes in device info report")) // BootloadHID
                {
                    printer.Print("File is too large for device", MessageType.Error);
                }
            }
        }
    }
}
