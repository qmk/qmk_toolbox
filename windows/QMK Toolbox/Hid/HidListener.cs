using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace QMK_Toolbox.Hid
{
    public class HidListener : IDisposable
    {
        private const ushort ConsoleUsagePage = 0xFF31;
        private const ushort ConsoleUsage = 0x0074;

        private const ushort RawUsagePage = 0xFF60;
        private const ushort RawUsage = 0x0061;

        public List<BaseHidDevice> Devices { get; private set; }

        public delegate void HidDeviceEventDelegate(BaseHidDevice device);
        public delegate void HidConsoleReportReceivedDelegate(HidConsoleDevice device, string data);

        public HidDeviceEventDelegate hidDeviceConnected;
        public HidDeviceEventDelegate hidDeviceDisconnected;
        public HidConsoleReportReceivedDelegate consoleReportReceived;

        private void EnumerateHidDevices(bool connected)
        {
            var enumeratedDevices = HidDevices.Enumerate()
                .Where(d => d.IsConnected)
                .Where(d => d.Capabilities.InputReportByteLength > 0);

            if (connected)
            {
                foreach (var device in enumeratedDevices)
                {
                    var listed = Devices.Aggregate(false, (curr, d) => curr | d.HidDevice.DevicePath.Equals(device.DevicePath));

                    if (device != null && !listed)
                    {
                        BaseHidDevice hidDevice = CreateDevice(device);

                        if (hidDevice != null)
                        {
                            Devices.Add(hidDevice);

                            if (hidDevice is HidConsoleDevice)
                            {
                                (hidDevice as HidConsoleDevice).consoleReportReceived = HidConsoleReportReceived;
                            }
                            hidDeviceConnected?.Invoke(hidDevice);
                        }
                    }
                }
            }
            else
            {
                foreach (var device in Devices.ToList())
                {
                    var listed = enumeratedDevices.Aggregate(false, (curr, d) => curr | device.HidDevice.DevicePath.Equals(d.DevicePath));

                    if (!listed)
                    {
                        if (device.HidDevice.IsOpen)
                        {
                            device.HidDevice.CloseDevice();
                        }
                        Devices.Remove(device);

                        if (device is HidConsoleDevice)
                        {
                            (device as HidConsoleDevice).consoleReportReceived = null;
                        }
                        hidDeviceDisconnected?.Invoke(device);
                    }
                }
            }
        }

        public void HidConsoleReportReceived(HidConsoleDevice device, string data)
        {
            consoleReportReceived?.Invoke(device, data);
        }

        private ManagementEventWatcher deviceConnectedWatcher;
        private ManagementEventWatcher deviceDisconnectedWatcher;

        private static ManagementEventWatcher CreateManagementEventWatcher(string eventType)
        {
            return new ManagementEventWatcher($"SELECT * FROM {eventType} WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.DeviceID LIKE 'HID%'");
        }

        private void HidDeviceWmiEvent(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent["TargetInstance"] is not ManagementBaseObject _)
            {
                return;
            }

            (sender as ManagementEventWatcher)?.Stop();
            EnumerateHidDevices(e.NewEvent.ClassPath.ClassName.Equals("__InstanceCreationEvent"));
            (sender as ManagementEventWatcher)?.Start();
        }

        public void Start()
        {
            Devices ??= new List<BaseHidDevice>();
            EnumerateHidDevices(true);

            deviceConnectedWatcher ??= CreateManagementEventWatcher("__InstanceCreationEvent");
            deviceConnectedWatcher.EventArrived += HidDeviceWmiEvent;
            deviceConnectedWatcher.Start();

            deviceDisconnectedWatcher ??= CreateManagementEventWatcher("__InstanceDeletionEvent");
            deviceDisconnectedWatcher.EventArrived += HidDeviceWmiEvent;
            deviceDisconnectedWatcher.Start();
        }

        public void Stop()
        {
            if (deviceConnectedWatcher != null)
            {
                deviceConnectedWatcher.Stop();
                deviceConnectedWatcher.EventArrived -= HidDeviceWmiEvent;
            }

            if (deviceDisconnectedWatcher != null)
            {
                deviceDisconnectedWatcher.Stop();
                deviceDisconnectedWatcher.EventArrived -= HidDeviceWmiEvent;
            }
        }

        public void Dispose()
        {
            Stop();
            deviceConnectedWatcher?.Dispose();
            deviceDisconnectedWatcher?.Dispose();
            GC.SuppressFinalize(this);
        }

        private static BaseHidDevice CreateDevice(HidDevice d)
        {
            if ((ushort)d.Capabilities.UsagePage == ConsoleUsagePage && (ushort)d.Capabilities.Usage == ConsoleUsage)
            {
                return new HidConsoleDevice(d);
            }
            else if ((ushort)d.Capabilities.UsagePage == RawUsagePage && (ushort)d.Capabilities.Usage == RawUsage)
            {
                return new RawDevice(d);
            }

            return null;
        }
    }
}
