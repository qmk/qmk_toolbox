using System;
using Avalonia.Threading;
// ReSharper disable StringLiteralTypo
using System.Diagnostics;
using System.Threading.Tasks;

namespace QMK_Toolbox.Usb.Bootloader;

internal abstract class BootloaderDevice
{
    public delegate void FlashOutputReceivedDelegate(BootloaderDevice device, string data, MessageType type);

    public FlashOutputReceivedDelegate OutputReceived;

    public BootloaderDevice(KnownHidDevice d)
    {
        KnownHidDevice = d;
    }

    private KnownHidDevice KnownHidDevice { get; }

    public ushort VendorId => KnownHidDevice.VendorId;

    public ushort ProductId => KnownHidDevice.ProductId;

    public ushort RevisionBcd => KnownHidDevice.RevisionBcd;

    public string ManufacturerString => KnownHidDevice.VendorName;

    public string ProductString => KnownHidDevice.ProductName;

    public bool IsEepromFlashable { get; protected set; }

    public bool IsResettable { get; protected set; }

    public BootloaderType Type { get; protected set; }

    public string Name { get; protected set; }

    public override string ToString()
    {
        return KnownHidDevice.ToString();
    }

    public virtual void Flash(string mcu, string file)
    {
        throw new NotImplementedException();
    }

    public virtual void FlashEeprom(string mcu, string file)
    {
        throw new NotImplementedException();
    }
    
    public virtual void Reset(string mcu)
    {
        throw new NotImplementedException();
    }

    protected async Task<int> RunProcessAsync(string command, string args)
    {
        using var process = new Process
        {
            StartInfo =
            {
                FileName = command,
                Arguments = args,
                WorkingDirectory ="/tmp",
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

        process.Exited += (sender, e) =>
        {
            process.WaitForExit();
            tcs.SetResult(process.ExitCode);
        };

        process.OutputDataReceived += ProcessOutput;
        process.ErrorDataReceived += ProcessErrorOutput;

        var started = process.Start();
        if (!started) PrintMessage($"Could not start process: {process}", MessageType.Error);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return tcs.Task;
    }

    private void ProcessOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) PrintMessage(e.Data, MessageType.CommandOutput);
    }

    private void ProcessErrorOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null) PrintMessage(e.Data, MessageType.CommandError);
    }

    protected void PrintMessage(string message, MessageType type)
    {
        Task.Delay(1).ContinueWith( t => Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                OutputReceived?.Invoke(this, message, type);
            }));
    }
}