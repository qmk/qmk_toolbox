using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace QMK_Toolbox.HidConsole
{
    public class HidConsoleListener : IDisposable
    {
        private const ushort ConsoleUsagePage = 0xFF31;
        private const ushort ConsoleUsage = 0x0074;

        public List<HidConsoleDevice> Devices { get; private set; }

        public delegate void HidConsoleDeviceEventDelegate(HidConsoleDevice device);
        public delegate void HidConsoleReportReceivedDelegate(HidConsoleDevice device, string data);

        public HidConsoleDeviceEventDelegate consoleDeviceConnected;
        public HidConsoleDeviceEventDelegate consoleDeviceDisconnected;
        public HidConsoleReportReceivedDelegate consoleReportReceived;

        private void EnumerateHidDevices(bool connected)
        {
            var enumeratedDevices = HidDevices.Enumerate()
                .Where(d => d.IsConnected)
                .Where(d => d.Capabilities.InputReportByteLength > 0)
                .Where(d => (ushort)d.Capabilities.UsagePage == ConsoleUsagePage)
                .Where(d => (ushort)d.Capabilities.Usage == ConsoleUsage);

            if (connected)
            {
                foreach (var device in enumeratedDevices)
                {
                    var listed = Devices.Aggregate(false, (curr, d) => curr | d.HidDevice.DevicePath.Equals(device.DevicePath));

                    if (device != null && !listed)
                    {
                        HidConsoleDevice consoleDevice = new HidConsoleDevice(device)
                        {
                            consoleReportReceived = HidConsoleReportReceived
                        };
                        Devices.Add(consoleDevice);
                        consoleDeviceConnected?.Invoke(consoleDevice);
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
                        device.consoleReportReceived = null;
                        consoleDeviceDisconnected?.Invoke(device);
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

        private ManagementEventWatcher CreateManagementEventWatcher(string eventType)
        {
            return new ManagementEventWatcher($"SELECT * FROM {eventType} WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.DeviceID LIKE 'HID%'");
        }

        private void HidDeviceWmiEvent(object sender, EventArrivedEventArgs e)
        {
            if (!(e.NewEvent["TargetInstance"] is ManagementBaseObject _))
            {
                return;
            }

            (sender as ManagementEventWatcher)?.Stop();
            EnumerateHidDevices(e.NewEvent.ClassPath.ClassName.Equals("__InstanceCreationEvent"));
            (sender as ManagementEventWatcher)?.Start();
        }

        public void Start()
        {
            if (Devices == null)
            {
                Devices = new List<HidConsoleDevice>();
            }
            EnumerateHidDevices(true);

            if (deviceConnectedWatcher == null)
            {
                deviceConnectedWatcher = CreateManagementEventWatcher("__InstanceCreationEvent");
            }
            deviceConnectedWatcher.EventArrived += HidDeviceWmiEvent;
            deviceConnectedWatcher.Start();

            if (deviceDisconnectedWatcher == null)
            {
                deviceDisconnectedWatcher = CreateManagementEventWatcher("__InstanceDeletionEvent");
            }
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
        }
    }
}
