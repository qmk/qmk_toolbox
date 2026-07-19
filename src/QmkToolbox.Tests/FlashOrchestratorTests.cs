using NSubstitute;
using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

public class FlashOrchestratorTests
{
    private static FlashOrchestrator NewOrchestrator() => new(
        Substitute.For<IFlashToolProvider>(),
        Substitute.For<ISerialPortService>(),
        Substitute.For<IMountPointService>());

    [Fact]
    public async Task RunExclusiveAsync_WhileBusy_SecondCallRefused()
    {
        FlashOrchestrator orch = NewOrchestrator();
        var gate = new TaskCompletionSource();

        // Start an operation that stays in flight until we release the gate.
        Task<bool> first = orch.RunExclusiveAsync(() => gate.Task);
        Assert.True(orch.IsBusy);
        Assert.False(first.IsCompleted);

        // A second request is refused (returns false) and does not disturb the in-flight op.
        bool second = await orch.RunExclusiveAsync(() => Task.CompletedTask);
        Assert.False(second);
        Assert.True(orch.IsBusy);

        gate.SetResult();
        Assert.True(await first);
        Assert.False(orch.IsBusy);
    }

    [Fact]
    public async Task RunExclusiveAsync_IsBusy_TogglesAroundOperation()
    {
        FlashOrchestrator orch = NewOrchestrator();
        Assert.False(orch.IsBusy);

        bool busyDuring = false;
        bool ran = await orch.RunExclusiveAsync(() =>
        {
            busyDuring = orch.IsBusy;
            return Task.CompletedTask;
        });

        Assert.True(ran);
        Assert.True(busyDuring);
        Assert.False(orch.IsBusy);
    }

    [Fact]
    public async Task RunExclusiveAsync_OperationThrows_ResetsBusyAndPropagates()
    {
        FlashOrchestrator orch = NewOrchestrator();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orch.RunExclusiveAsync(() => throw new InvalidOperationException()));

        Assert.False(orch.IsBusy);
    }

    [Fact]
    public async Task RunExclusiveAsync_RaisesStateChangedOnEnterAndExit()
    {
        FlashOrchestrator orch = NewOrchestrator();
        int changes = 0;
        orch.StateChanged += () => changes++;

        var gate = new TaskCompletionSource();
        Task<bool> op = orch.RunExclusiveAsync(() => gate.Task);
        Assert.Equal(1, changes); // entered -> busy

        gate.SetResult();
        await op;
        Assert.Equal(2, changes); // exited -> idle
    }
}
