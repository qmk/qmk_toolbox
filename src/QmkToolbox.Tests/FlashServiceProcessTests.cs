using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Exercises <see cref="FlashService.RunToolAsync"/>'s own logic — raw chunk pumping, exit-code
/// return, start-failure, and timeout/kill — through the <see cref="IProcessRunner"/> seam and a
/// fake clock, with no real child process.
/// </summary>
public class FlashServiceProcessTests
{
    private static IFlashToolProvider Provider()
    {
        IFlashToolProvider provider = Substitute.For<IFlashToolProvider>();
        provider.GetToolPath(Arg.Any<string>()).Returns("tool");
        provider.GetResourceFolder().Returns("/tmp");
        return provider;
    }

    [Fact]
    public async Task RunToolAsync_ForwardsStdoutRaw_AndReturnsExitCode()
    {
        const string payload = "\rErase 0%\rErase 100%\nDone\n";
        var process = new FakeRunningProcess(stdout: payload, exitCode: 0);
        var received = new StringBuilder();

        int exit = await FlashService.RunToolAsync("tool", [], Provider(),
            (data, type) => { if (type == MessageType.CommandOutput) received.Append(data); },
            new FakeProcessRunner(process));

        Assert.Equal(0, exit);
        Assert.Equal(payload, received.ToString());
    }

    [Fact]
    public async Task RunToolAsync_RoutesStderr_AsCommandError()
    {
        var process = new FakeRunningProcess(stderr: "boom\n", exitCode: 1);
        var received = new StringBuilder();

        int exit = await FlashService.RunToolAsync("tool", [], Provider(),
            (data, type) => { if (type == MessageType.CommandError) received.Append(data); },
            new FakeProcessRunner(process));

        Assert.Equal(1, exit);
        Assert.Equal("boom\n", received.ToString());
    }

    [Fact]
    public async Task RunToolAsync_NonZeroExit_Propagated()
    {
        var process = new FakeRunningProcess(exitCode: 42);

        int exit = await FlashService.RunToolAsync("tool", [], Provider(), null,
            new FakeProcessRunner(process));

        Assert.Equal(42, exit);
    }

    [Fact]
    public async Task RunToolAsync_StartFailure_EmitsErrorAndReturnsMinusOne()
    {
        var runner = new FakeProcessRunner(new Win32Exception("No such file or directory"));
        MessageType? errorType = null;
        string? errorMessage = null;

        int exit = await FlashService.RunToolAsync("tool", [], Provider(),
            (data, type) => { if (type == MessageType.Error) { errorType = type; errorMessage = data; } },
            runner);

        Assert.Equal(-1, exit);
        Assert.Equal(MessageType.Error, errorType);
        Assert.StartsWith("Could not start process:", errorMessage);
    }

    [Fact]
    public async Task RunToolAsync_OutputPumpFails_KillsProcessAndReturnsMinusOne()
    {
        var process = new FakeRunningProcess(stdoutThrows: true, hang: true);
        var errors = new StringBuilder();

        int exit = await FlashService.RunToolAsync("tool", [], Provider(),
            (data, type) => { if (type == MessageType.Error) errors.Append(data); },
            new FakeProcessRunner(process));

        Assert.Equal(-1, exit);
        Assert.True(process.KillCalled);
        Assert.Contains("Error reading tool output", errors.ToString());
    }

    [Fact]
    public async Task RunToolAsync_Timeout_KillsProcessAndReturnsMinusOne()
    {
        var timeProvider = new FakeTimeProvider();
        var process = new FakeRunningProcess(hang: true);
        var received = new StringBuilder();

        Task<int> run = FlashService.RunToolAsync("tool", [], Provider(),
            (data, type) => { if (type == MessageType.Error) received.Append(data); },
            new FakeProcessRunner(process),
            timeProvider);

        // The process never exits; advancing past the 5-minute limit fires the timeout.
        timeProvider.Advance(TimeSpan.FromMinutes(6));

        int exit = await run;

        Assert.Equal(-1, exit);
        Assert.True(process.KillCalled);
        Assert.Contains("timed out", received.ToString());
    }
}

internal sealed class FakeProcessRunner : IProcessRunner
{
    private readonly IRunningProcess? _process;
    private readonly Exception? _startException;

    public FakeProcessRunner(IRunningProcess process)
    {
        _process = process;
    }

    public FakeProcessRunner(Exception startException)
    {
        _startException = startException;
    }

    public IRunningProcess Start(string fileName, string workingDir, IReadOnlyList<string> args) =>
        _startException != null ? throw _startException : _process!;
}

internal sealed class FakeRunningProcess(
    string stdout = "", string stderr = "", int exitCode = 0, bool hang = false, bool stdoutThrows = false)
    : IRunningProcess
{
    public StreamReader StandardOutput { get; } = new(
        stdoutThrows ? new ThrowingStream() : new MemoryStream(Encoding.UTF8.GetBytes(stdout)));
    public StreamReader StandardError { get; } = new(new MemoryStream(Encoding.UTF8.GetBytes(stderr)));
    public int ExitCode { get; } = exitCode;
    public bool KillCalled { get; private set; }

    // When hanging, the process only "exits" via cancellation, mimicking a stuck flash tool.
    public Task WaitForExitAsync(CancellationToken cancellationToken) =>
        hang ? Task.Delay(Timeout.Infinite, cancellationToken) : Task.CompletedTask;

    public void Kill() => KillCalled = true;

    public void Dispose()
    {
        StandardOutput.Dispose();
        StandardError.Dispose();
    }
}

/// <summary>A read stream that fails mid-pump, mimicking a broken pipe.</summary>
internal sealed class ThrowingStream : Stream
{
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => 0;
    public override long Position { get => 0; set => throw new NotSupportedException(); }

    public override int Read(byte[] buffer, int offset, int count) => throw new IOException("broken pipe");
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        throw new IOException("broken pipe");

    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
