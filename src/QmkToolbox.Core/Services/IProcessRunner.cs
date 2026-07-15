namespace QmkToolbox.Core.Services;

/// <summary>
/// Starts an external process. This is the single seam that lets <see cref="FlashService"/> run
/// against a scripted process in tests instead of a real child process.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Starts <paramref name="fileName"/> with the given arguments (each passed as a discrete
    /// argument). Throws <see cref="System.ComponentModel.Win32Exception"/> if it cannot start.
    /// </summary>
    IRunningProcess Start(string fileName, string workingDir, IReadOnlyList<string> args);
}

/// <summary>A started process: its output streams, completion, exit code, and kill switch.</summary>
public interface IRunningProcess : IDisposable
{
    StreamReader StandardOutput { get; }
    StreamReader StandardError { get; }
    Task WaitForExitAsync(CancellationToken cancellationToken);
    int ExitCode { get; }
    void Kill();
}
