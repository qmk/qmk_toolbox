using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Services;
using QmkToolbox.Desktop.ViewModels;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Drives HidConsoleViewModel through the IHidListener seam with a fake adapter. The ViewModel
/// runs with no UI invoker, so event handling executes synchronously. Devices are keyed by
/// DevicePath, so two identical keyboards (same label) stay distinct.
/// </summary>
public class HidConsoleViewModelTests
{
    private const string AllDevices = "(All connected devices)";

    private sealed class FakeHidListener : IHidListener
    {
        public event Action<IHidDevice>? HidDeviceConnected;
        public event Action<IHidDevice>? HidDeviceDisconnected;
        public event Action<IHidDevice, string>? ConsoleReportReceived;
        public event Action<string>? ErrorOccurred;

        public bool Disposed;

        public void Start() { }
        public void Dispose() => Disposed = true;

        public void RaiseConnected(IHidDevice d) => HidDeviceConnected?.Invoke(d);
        public void RaiseDisconnected(IHidDevice d) => HidDeviceDisconnected?.Invoke(d);
        public void RaiseReport(IHidDevice d, string data) => ConsoleReportReceived?.Invoke(d, data);
        public void RaiseError(string message) => ErrorOccurred?.Invoke(message);
    }

    private sealed class FakeHidDevice(string label, bool isConsole = true, string? path = null) : IHidDevice
    {
        public ushort VendorId => 0xFEED;
        public ushort ProductId => 0x0001;
        public ushort RevisionBcd => 0x0100;
        public ushort UsagePage => 0xFF31;
        public ushort Usage => 0x0074;
        public string ManufacturerString => "QMK";
        public string ProductString => label;
        public string DevicePath => path ?? $"/dev/hidraw-{label}";
        public bool IsConsoleDevice => isConsole;
        public override string ToString() => label;
    }

    private static (HidConsoleViewModel Vm, FakeHidListener Listener) NewConsole()
    {
        var listener = new FakeHidListener();
        var vm = new HidConsoleViewModel(listener);
        return (vm, listener);
    }

    private static IEnumerable<string> Labels(HidConsoleViewModel vm) => vm.Devices.Select(d => d.Label);

    [Fact]
    public void ConsoleDeviceConnected_AddsEntryAndLogs()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();

        listener.RaiseConnected(new FakeHidDevice("Planck"));

        Assert.Equal([AllDevices, "Planck"], Labels(vm));
        Assert.Contains("HID console device connected: Planck", vm.Buffer.ToString());
    }

    [Fact]
    public void NonConsoleDevice_Ignored()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();

        listener.RaiseConnected(new FakeHidDevice("Mouse", isConsole: false));

        Assert.Equal([AllDevices], Labels(vm));
        Assert.Equal("", vm.Buffer.ToString());
    }

    [Fact]
    public void IdenticalDevices_TrackedSeparatelyByPath()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        var left = new FakeHidDevice("Planck", path: "/dev/hidraw0");
        var right = new FakeHidDevice("Planck", path: "/dev/hidraw1");

        listener.RaiseConnected(left);
        listener.RaiseConnected(right);

        Assert.Equal([AllDevices, "Planck", "Planck"], Labels(vm));

        listener.RaiseDisconnected(left);

        Assert.Equal([AllDevices, "Planck"], Labels(vm));
        Assert.Equal("/dev/hidraw1", vm.Devices[1].DevicePath);
    }

    [Fact]
    public void DeviceDisconnected_RemovesEntry_AndResetsSelectionIfSelected()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        var device = new FakeHidDevice("Planck");
        listener.RaiseConnected(device);
        vm.SelectedDevice = vm.Devices[1];

        listener.RaiseDisconnected(device);

        Assert.Equal([AllDevices], Labels(vm));
        Assert.Equal(AllDevices, vm.SelectedDevice?.Label);
    }

    [Fact]
    public void DeviceDisconnected_OtherDeviceSelected_SelectionKept()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        var planck = new FakeHidDevice("Planck");
        var corne = new FakeHidDevice("Corne");
        listener.RaiseConnected(planck);
        listener.RaiseConnected(corne);
        vm.SelectedDevice = vm.Devices.First(d => d.Label == "Corne");

        listener.RaiseDisconnected(planck);

        Assert.Equal("Corne", vm.SelectedDevice?.Label);
    }

    [Fact]
    public void ConsoleReport_AllDevicesSelected_Logged()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        var device = new FakeHidDevice("Planck");
        listener.RaiseConnected(device);

        listener.RaiseReport(device, "dbg: hello\n");

        Assert.Contains("dbg: hello", vm.Buffer.ToString());
    }

    [Fact]
    public void ConsoleReport_OtherDeviceSelected_Filtered()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        var planck = new FakeHidDevice("Planck");
        var corne = new FakeHidDevice("Corne");
        listener.RaiseConnected(planck);
        listener.RaiseConnected(corne);
        vm.SelectedDevice = vm.Devices.First(d => d.Label == "Corne");

        listener.RaiseReport(planck, "from planck\n");
        listener.RaiseReport(corne, "from corne\n");

        Assert.DoesNotContain("from planck", vm.Buffer.ToString());
        Assert.Contains("from corne", vm.Buffer.ToString());
    }

    [Fact]
    public void ConsoleReport_IdenticalLabels_FiltersByPath()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        var left = new FakeHidDevice("Planck", path: "/dev/hidraw0");
        var right = new FakeHidDevice("Planck", path: "/dev/hidraw1");
        listener.RaiseConnected(left);
        listener.RaiseConnected(right);
        vm.SelectedDevice = vm.Devices.First(d => d.DevicePath == "/dev/hidraw1");

        listener.RaiseReport(left, "from left\n");
        listener.RaiseReport(right, "from right\n");

        Assert.DoesNotContain("from left", vm.Buffer.ToString());
        Assert.Contains("from right", vm.Buffer.ToString());
    }

    [Fact]
    public void Error_Logged()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();

        listener.RaiseError("HID polling stopped unexpectedly: boom");

        Assert.Contains("HID polling stopped unexpectedly: boom", vm.Buffer.ToString());
    }

    [Fact]
    public void Dispose_DisposesTheListener()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        vm.Dispose();
        Assert.True(listener.Disposed);
    }
}
