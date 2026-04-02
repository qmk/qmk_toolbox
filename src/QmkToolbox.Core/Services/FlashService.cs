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
    /// Lines terminated by a bare CR (carriage return without line feed) are passed
    /// back with a trailing '\r' so callers can overwrite the previous line rather
    /// than appending a new one (typical for progress-bar style output).
    /// </summary>
    /// <param name="toolName">Name of the tool binary (without path or extension).</param>
    /// <param name="args">Individual command-line arguments. Each element is passed as a
    /// discrete argument via <see cref="ProcessStartInfo.ArgumentList"/>, so paths with
    /// spaces or special characters are handled correctly without manual quoting.</param>
    /// <param name="toolProvider">Resolves tool paths and working directory.</param>
    /// <param name="outputReceived">Optional callback for stdout/stderr lines.</param>
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
            Task stdoutTask = ReadLinesAsync(process.StandardOutput, cts.Token,
                (line, overwrite) => outputReceived?.Invoke(overwrite ? line + '\r' : line, MessageType.CommandOutput));
            Task stderrTask = ReadLinesAsync(process.StandardError, cts.Token,
                (line, overwrite) => outputReceived?.Invoke(overwrite ? line + '\r' : line, MessageType.CommandError));

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

    /// <summary>
    /// Reads characters from <paramref name="reader"/>, splitting on LF, CRLF, and bare CR.
    /// Bare CR-terminated lines are reported with <c>overwrite = true</c>.
    /// </summary>
    internal static async Task ReadLinesAsync(StreamReader reader, CancellationToken ct, Action<string, bool> onLine)
    {
        var sb = new StringBuilder();
        // CR handling requires a one-character lookahead: we cannot emit on CR immediately
        // because we don't yet know whether the next character is LF (making it CRLF → append)
        // or something else (bare CR → overwrite). This flag carries that state across
        // iterations of the inner loop and across buffer boundaries.
        bool pendingCr = false;
        char[] buf = new char[4096];

        void emit(bool overwrite)
        {
            onLine(sb.ToString(), overwrite);
            sb.Clear();
        }

        while (!ct.IsCancellationRequested)
        {
            int count;
            try
            {
                count = await reader.ReadAsync(buf, 0, buf.Length).WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (count == 0)
                break;

            for (int i = 0; i < count; i++)
            {
                char c = buf[i];

                if (pendingCr)
                {
                    pendingCr = false;
                    // CRLF: emit as a normal (append) line and skip the LF.
                    // Bare CR: emit as an overwrite line and fall through to process c normally.
                    emit(overwrite: c != '\n');
                    if (c == '\n')
                        continue;
                }

                if (c == '\r')
                    pendingCr = true;   // defer until we see the next character
                else if (c == '\n')
                    emit(overwrite: false);
                else
                    sb.Append(c);
            }
        }

        // Flush any buffered text. If a CR was the last character, emit as overwrite
        // (no LF followed, so it was a bare CR). Otherwise emit as a normal trailing line.
        if (pendingCr || sb.Length > 0)
            emit(overwrite: pendingCr);
    }
}
