using System.ComponentModel;
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
    /// discrete argument, so paths with spaces or special characters are handled correctly
    /// without manual quoting.</param>
    /// <param name="toolProvider">Resolves tool paths and working directory.</param>
    /// <param name="outputReceived">Optional callback for stdout/stderr chunks.</param>
    /// <param name="runner">Process launcher; defaults to the real <see cref="SystemProcessRunner"/>.
    /// A fake runner lets tests drive scripted output, start failures, and timeouts.</param>
    /// <param name="timeProvider">Clock for the timeout; defaults to <see cref="TimeProvider.System"/>.
    /// A fake provider lets tests trigger the timeout deterministically.</param>
    public static async Task<int> RunToolAsync(
        string toolName,
        string[] args,
        IFlashToolProvider toolProvider,
        OutputReceivedDelegate? outputReceived,
        IProcessRunner? runner = null,
        TimeProvider? timeProvider = null)
    {
        runner ??= SystemProcessRunner.Shared;
        timeProvider ??= TimeProvider.System;

        outputReceived?.Invoke(FormatCommandLine(toolName, args), MessageType.Command);

        string toolPath = toolProvider.GetToolPath(toolName);
        string workingDir = toolProvider.GetResourceFolder();

        IRunningProcess process;
        try
        {
            process = runner.Start(toolPath, workingDir, args);
        }
        catch (Win32Exception)
        {
            outputReceived?.Invoke($"Could not start process: {toolPath}", MessageType.Error);
            return -1;
        }

        using (process)
        using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(FlashTimeoutMinutes), timeProvider))
        {
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
                    process.Kill();
                }
                catch { }
                return -1;
            }
        }
    }

    /// <summary>
    /// Formats a tool name and arguments for display in the log (MessageType.Command).
    /// Not used for process invocation — actual execution uses ProcessStartInfo.ArgumentList.
    /// </summary>
    private static string FormatCommandLine(string toolName, string[] args) =>
        string.Join(' ', [toolName, .. args.Select(a =>
            a.Contains(' ') || a.Length == 0 ? $"\"{a.Replace("\"", "\\\"")}\"" : a)]);

    private static async Task PumpAsync(StreamReader reader, CancellationToken ct, Action<string> onChunk)
    {
        char[] buf = new char[4096];
        int count;
        while ((count = await reader.ReadAsync(buf.AsMemory(), ct).ConfigureAwait(false)) > 0)
            onChunk(new string(buf, 0, count));
    }
}
