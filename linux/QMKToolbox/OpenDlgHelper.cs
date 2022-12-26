using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox;

public class OpenDlgHelper
{
    private StringBuilder StringBuilder { get; set; }

    public string GetFileName()
    {
        RunProcessAsync("/usr/bin/kdialog", "--getopenfilename /home/").Wait();
        return StringBuilder.ToString();
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
                WorkingDirectory = Path.GetDirectoryName(command),
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
        if (e.Data != null) StringBuilder.Append(e.Data);
    }

    private static void ProcessErrorOutput(object _, DataReceivedEventArgs e)
    {
        if (e.Data != null) Debug.WriteLine(e.Data);
    }
}