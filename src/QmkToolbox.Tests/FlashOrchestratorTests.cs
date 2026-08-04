using NSubstitute;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

public class FlashOrchestratorTests
{
    private static FlashOrchestrator NewOrchestrator(IMountPointService? mount = null) => new(
        Substitute.For<IFlashToolProvider>(),
        Substitute.For<ISerialPortService>(),
        mount ?? Substitute.For<IMountPointService>())
    {
        VolumeProbeDelayMs = 1,
    };

    private static IUsbDevice MassStorage(ushort pid = 0x00FF, string path = "path0") =>
        new UsbDeviceInfo(0x239A, pid, 0, "Test", "Board", "", path, IsMassStorage: true);

    private static IUsbDevice Unknown() =>
        new UsbDeviceInfo(0x1234, 0x5678, 0, "", "", "", "path1");

    // ── UF2 volume probe ──────────────────────────────────────────────────────

    [Fact]
    public async Task OnDeviceConnectedAsync_MassStorageWithUf2Volume_RegistersBootloader()
    {
        string mountDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(mountDir);
        File.WriteAllText(Path.Combine(mountDir, "INFO_UF2.TXT"),
            "UF2 Bootloader v3.0\nModel: Test Board\nBoard-ID: TEST-V1\n");

        try
        {
            IMountPointService mount = Substitute.For<IMountPointService>();
            mount.FindMountPoint(Arg.Any<IUsbDevice>(), "INFO_UF2.TXT").Returns(mountDir);
            FlashOrchestrator orch = NewOrchestrator(mount);

            var connected = new TaskCompletionSource<string>();
            orch.OutputReceived += (msg, type) =>
            {
                if (type == MessageType.Bootloader && msg.Contains("device connected"))
                    connected.TrySetResult(msg);
            };

            Assert.True(await orch.OnDeviceConnectedAsync(MassStorage(), false));
            Assert.True(orch.HasBootloaders);

            // The connected message is emitted from a WhenReadyAsync continuation on the
            // thread pool; wait for it rather than asserting immediately.
            string msg = await connected.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.StartsWith("UF2 (TEST-V1) device connected", msg);
        }
        finally
        {
            Directory.Delete(mountDir, true);
        }
    }

    [Fact]
    public async Task OnDeviceConnectedAsync_VolumeMountedLate_RegistersOnMount()
    {
        string mountDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(mountDir);
        File.WriteAllText(Path.Combine(mountDir, "INFO_UF2.TXT"), "UF2 Bootloader v3.0\n");

        try
        {
            // Desktops that don't automount (e.g. KDE) surface the volume only when the
            // user clicks mount — several polls after arrival here stands in for that.
            IMountPointService mount = Substitute.For<IMountPointService>();
            mount.FindMountPoint(Arg.Any<IUsbDevice>(), "INFO_UF2.TXT")
                .Returns(null, null, null, null, mountDir);
            FlashOrchestrator orch = NewOrchestrator(mount);

            Assert.True(await orch.OnDeviceConnectedAsync(MassStorage(), false).WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.True(orch.HasBootloaders);
        }
        finally
        {
            Directory.Delete(mountDir, true);
        }
    }

    [Fact]
    public async Task OnDeviceConnectedAsync_VolumeNeverMounts_ProbesUntilRemoval()
    {
        // NSubstitute auto-returns "" for strings; the probe must see null ("not mounted").
        IMountPointService mount = Substitute.For<IMountPointService>();
        mount.FindMountPoint(Arg.Any<IUsbDevice>(), Arg.Any<string>()).Returns((string?)null);
        FlashOrchestrator orch = NewOrchestrator(mount);
        IUsbDevice device = MassStorage();

        Task<bool> connect = orch.OnDeviceConnectedAsync(device, false);
        await Task.Delay(50);
        Assert.False(connect.IsCompleted);

        orch.OnDeviceDisconnected(device, false);

        Assert.False(await connect.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.False(orch.HasBootloaders);
    }

    [Fact]
    public async Task OnDeviceConnectedAsync_VolumeAlreadyClaimed_SecondDeviceKeepsProbing()
    {
        string mountDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(mountDir);
        File.WriteAllText(Path.Combine(mountDir, "INFO_UF2.TXT"), "UF2 Bootloader v3.0\n");

        try
        {
            IMountPointService mount = Substitute.For<IMountPointService>();
            mount.FindMountPoint(Arg.Any<IUsbDevice>(), "INFO_UF2.TXT").Returns(mountDir);
            FlashOrchestrator orch = NewOrchestrator(mount);
            IUsbDevice second = MassStorage(pid: 0x0002, path: "path2");

            Assert.True(await orch.OnDeviceConnectedAsync(MassStorage(pid: 0x0001, path: "path1"), false));

            // The volume backs the first device now, so the second must not claim it too.
            Task<bool> connect = orch.OnDeviceConnectedAsync(second, false);
            await Task.Delay(50);
            Assert.False(connect.IsCompleted);

            orch.OnDeviceDisconnected(second, false);

            Assert.False(await connect.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.True(orch.HasBootloaders);
        }
        finally
        {
            Directory.Delete(mountDir, true);
        }
    }

    [Fact]
    public async Task OnDeviceConnectedAsync_UnknownNonMassStorage_NotProbed()
    {
        IMountPointService mount = Substitute.For<IMountPointService>();
        FlashOrchestrator orch = NewOrchestrator(mount);

        Assert.False(await orch.OnDeviceConnectedAsync(Unknown(), false));

        Assert.False(orch.HasBootloaders);
        mount.DidNotReceive().FindMountPoint(Arg.Any<IUsbDevice>(), Arg.Any<string>());
    }

    [Fact]
    public async Task OnDeviceDisconnected_RemovesUf2Bootloader()
    {
        string mountDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(mountDir);
        File.WriteAllText(Path.Combine(mountDir, "INFO_UF2.TXT"), "UF2 Bootloader v3.0\n");

        try
        {
            IMountPointService mount = Substitute.For<IMountPointService>();
            mount.FindMountPoint(Arg.Any<IUsbDevice>(), "INFO_UF2.TXT").Returns(mountDir);
            FlashOrchestrator orch = NewOrchestrator(mount);
            IUsbDevice device = MassStorage();
            await orch.OnDeviceConnectedAsync(device, false);
            Assert.True(orch.HasBootloaders);

            orch.OnDeviceDisconnected(device, false);

            Assert.False(orch.HasBootloaders);
        }
        finally
        {
            Directory.Delete(mountDir, true);
        }
    }

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
    public async Task RunExclusiveAsync_OperationThrows_ResetsBusyAndPropagates()
    {
        FlashOrchestrator orch = NewOrchestrator();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orch.RunExclusiveAsync(() => throw new InvalidOperationException()));

        Assert.False(orch.IsBusy);
    }
}
