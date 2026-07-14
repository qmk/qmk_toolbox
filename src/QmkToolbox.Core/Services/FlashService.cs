using System.Diagnostics;
using System.Text;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Core.Services;

public static class FlashService
{
    private const int FlashTimeoutMinutes = 5;

    public delegate void OutputReceivedDelegate(string data, MessageType type);

    /// <summary>
    /// Launches a flash tool as a child process and returns its exit code.
    /// stdout/stderr are forwarded as raw text chunks as they arrive — embedded '\r'/'\n'
    /// intact, no line assembly — so a terminal-style consumer can render progress bars
    /// and partial lines immediately.
    /// </summary>
    /// <param name="toolName">Name of the tool binary (without path or extension).</param>
    /// <param name="args">Individual command-line arguments. Each element is passed as a
    /// discrete argument via <see cref="ProcessStartInfo.ArgumentList"/>, so paths with
    /// spaces or special characters are handled correctly without manual quoting.</param>
    /// <param name="toolProvider">Resolves tool paths and working directory.</param>
    /// <param name="outputReceived">Optional callback for stdout/stderr chunks.</param>
    public static async Task<int> RunToolAsync(
        string toolName,
        string[] args,
        IFlashToolProvider toolProvider,
        OutputReceivedDelegate? outputReceived)
    {
        outputReceived?.Invoke(FormatCommandLine(toolName, args), MessageType.Command);

        string toolPath = toolProvider.GetToolPath(toolName);
        string workingDir = toolProvider.GetResourceFolder();

        var startInfo = new ProcessStartInfo
        {
            FileName = toolPath,
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (string arg in args)
            startInfo.ArgumentList.Add(arg);

        using var process = new Process { StartInfo = startInfo };

        if (!process.Start())
        {
            outputReceived?.Invoke($"Could not start process: {toolPath}", MessageType.Error);
            return -1;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(FlashTimeoutMinutes));
        try
        {
            Task stdoutTask = PumpAsync(process.StandardOutput, cts.Token,
                chunk => outputReceived?.Invoke(chunk, MessageType.CommandOutput));
            Task stderrTask = PumpAsync(process.StandardError, cts.Token,
                chunk => outputReceived?.Invoke(chunk, MessageType.CommandError));

            await Task.WhenAll(stdoutTask, stderrTask).ConfigureAwait(false);
            await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
            return process.ExitCode;
        }
        catch (OperationCanceledException)
        {
            outputReceived?.Invoke($"Flash tool timed out after {FlashTimeoutMinutes} minutes.", MessageType.Error);
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch { }
            return -1;
        }
    }

    /// <summary>
    /// Formats a tool name and arguments for display in the log (MessageType.Command).
    /// Not used for process invocation — actual execution uses ProcessStartInfo.ArgumentList.
    /// </summary>
    private static string FormatCommandLine(string toolName, string[] args)
    {
        if (args.Length == 0)
            return toolName;
        var sb = new StringBuilder(toolName);
        foreach (string arg in args)
        {
            sb.Append(' ');
            if (arg.Contains(' ') || arg.Length == 0)
            {
                sb.Append('"');
                sb.Append(arg.Replace("\"", "\\\""));
                sb.Append('"');
            }
            else
            {
                sb.Append(arg);
            }
        }
        return sb.ToString();
    }

    private static async Task PumpAsync(StreamReader reader, CancellationToken ct, Action<string> onChunk)
    {
        char[] buf = new char[4096];
        int count;
        while ((count = await reader.ReadAsync(buf.AsMemory(), ct).ConfigureAwait(false)) > 0)
            onChunk(new string(buf, 0, count));
    }
}
