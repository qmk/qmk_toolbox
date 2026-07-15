using System.Diagnostics;

namespace QmkToolbox.Core.Services;

/// <summary>
/// The production <see cref="IProcessRunner"/>, backed by <see cref="Process"/>. Stateless —
/// use the single <see cref="Shared"/> instance.
/// </summary>
public sealed class SystemProcessRunner : IProcessRunner
{
    public static SystemProcessRunner Shared { get; } = new();

    public IRunningProcess Start(string fileName, string workingDir, IReadOnlyList<string> args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (string arg in args)
            startInfo.ArgumentList.Add(arg);

        var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch
        {
            process.Dispose();
            throw;
        }
        return new SystemRunningProcess(process);
    }

    private sealed class SystemRunningProcess(Process process) : IRunningProcess
    {
        public StreamReader StandardOutput => process.StandardOutput;
        public StreamReader StandardError => process.StandardError;
        public Task WaitForExitAsync(CancellationToken cancellationToken) => process.WaitForExitAsync(cancellationToken);
        public int ExitCode => process.ExitCode;
        public void Kill() => process.Kill(entireProcessTree: true);
        public void Dispose() => process.Dispose();
    }
}
