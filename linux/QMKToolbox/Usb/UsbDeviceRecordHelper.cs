using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb;

public class UsbDeviceRecordHelper
{
    private StringBuilder StringBuilder { get; set; }

    public List<UsbDeviceNameRecord> GetUsbDeviceRecordsWithNames(ushort vendorId=0, ushort productId=0)
    {
        var usbDeviceRecordList = new List<UsbDeviceNameRecord>();
        if (vendorId == 0 && productId == 0)
            RunProcessAsync("lsusb","").Wait();
        else
            RunProcessAsync("lsusb",$"-d {vendorId:x4}:{productId:x4}").Wait();

        var outputLines= StringBuilder.ToString().Split('\n');
        foreach (var line in outputLines)
        {
            var deviceRecord = new UsbDeviceNameRecord();
            if (string.IsNullOrEmpty(line))
                continue;
            var split = line.Split(" ");
            var productVendorSplit = split[5].Split(":");
            deviceRecord.VendorId=Convert.ToUInt16(productVendorSplit[0], 16);
            deviceRecord.ProductId= Convert.ToUInt16(split[1], 16);

            var sb= new StringBuilder();
            for (var i = 6; i < split.Length; i++)
            {
                sb.Append(split[i]);
                sb.Append(" ");
            }
            deviceRecord.ManufacturerName = sb.ToString();
            usbDeviceRecordList.Add(deviceRecord);
        }

        return usbDeviceRecordList;
    }


    private async Task<int> RunProcessAsync(string command, string args)
    {
        StringBuilder = new StringBuilder();
        using var process = new Process
        {
            StartInfo =
            {
                FileName = command,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(command) ?? "",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            },
            EnableRaisingEvents = true
        };
        return await RunProcessAsync(process).ConfigureAwait(false);
    }

    private Task<int> RunProcessAsync(Process process)
    {
        var tcs = new TaskCompletionSource<int>();

        process.Exited += (_, _) =>
        {
            process.WaitForExit();
            tcs.SetResult(process.ExitCode);
        };

        process.OutputDataReceived += ProcessOutput;
        process.ErrorDataReceived += ProcessErrorOutput;

        var started = process.Start();
        if (!started) Debug.WriteLine($"Could not start process: {process}");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return tcs.Task;
    }

    private void ProcessOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
            StringBuilder.AppendLine(e.Data);
    }

    private static void ProcessErrorOutput(object _, DataReceivedEventArgs e)
    {
        if (e.Data != null)
            Debug.WriteLine(e.Data);
    }
}