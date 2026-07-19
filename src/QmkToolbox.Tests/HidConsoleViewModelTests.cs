using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Services;
using QmkToolbox.Desktop.ViewModels;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Drives HidConsoleViewModel through the IHidListener seam with a fake adapter. The ViewModel
/// runs with no UI invoker, so event handling executes synchronously.
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

        public bool Started;
        public bool Disposed;

        public void Start() => Started = true;
        public void Dispose() => Disposed = true;

        public void RaiseConnected(IHidDevice d) => HidDeviceConnected?.Invoke(d);
        public void RaiseDisconnected(IHidDevice d) => HidDeviceDisconnected?.Invoke(d);
        public void RaiseReport(IHidDevice d, string data) => ConsoleReportReceived?.Invoke(d, data);
        public void RaiseError(string message) => ErrorOccurred?.Invoke(message);
    }

    private sealed class FakeHidDevice(string label, bool isConsole = true) : IHidDevice
    {
        public ushort VendorId => 0xFEED;
        public ushort ProductId => 0x0001;
        public ushort RevisionBcd => 0x0100;
        public ushort UsagePage => 0xFF31;
        public ushort Usage => 0x0074;
        public string ManufacturerString => "QMK";
        public string ProductString => label;
        public string DevicePath => $"/dev/hidraw-{label}";
        public bool IsConsoleDevice => isConsole;
        public override string ToString() => label;
    }

    private static (HidConsoleViewModel Vm, FakeHidListener Listener) NewConsole()
    {
        var listener = new FakeHidListener();
        var vm = new HidConsoleViewModel(listener);
        return (vm, listener);
    }

    [Fact]
    public void Start_StartsTheListener()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        vm.Start();
        Assert.True(listener.Started);
    }

    [Fact]
    public void ConsoleDeviceConnected_AddsLabelAndLogs()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();

        listener.RaiseConnected(new FakeHidDevice("Planck"));

        Assert.Equal(new[] { AllDevices, "Planck" }, vm.Devices);
        Assert.Contains("HID console device connected: Planck", vm.Buffer.ToString());
    }

    [Fact]
    public void NonConsoleDevice_Ignored()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();

        listener.RaiseConnected(new FakeHidDevice("Mouse", isConsole: false));

        Assert.Equal(new[] { AllDevices }, vm.Devices);
        Assert.Equal("", vm.Buffer.ToString());
    }

    [Fact]
    public void DeviceDisconnected_RemovesLabel_AndResetsSelectionIfSelected()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        var device = new FakeHidDevice("Planck");
        listener.RaiseConnected(device);
        vm.SelectedDevice = "Planck";

        listener.RaiseDisconnected(device);

        Assert.Equal(new[] { AllDevices }, vm.Devices);
        Assert.Equal(AllDevices, vm.SelectedDevice);
    }

    [Fact]
    public void DeviceDisconnected_OtherDeviceSelected_SelectionKept()
    {
        (HidConsoleViewModel vm, FakeHidListener listener) = NewConsole();
        var planck = new FakeHidDevice("Planck");
        var corne = new FakeHidDevice("Corne");
        listener.RaiseConnected(planck);
        listener.RaiseConnected(corne);
        vm.SelectedDevice = "Corne";

        listener.RaiseDisconnected(planck);

        Assert.Equal("Corne", vm.SelectedDevice);
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
        vm.SelectedDevice = "Corne";

        listener.RaiseReport(planck, "from planck\n");
        listener.RaiseReport(corne, "from corne\n");

        Assert.DoesNotContain("from planck", vm.Buffer.ToString());
        Assert.Contains("from corne", vm.Buffer.ToString());
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
