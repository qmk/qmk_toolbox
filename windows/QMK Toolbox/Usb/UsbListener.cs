using QMK_Toolbox.Usb.Bootloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace QMK_Toolbox.Usb
{
    public class UsbListener: IDisposable
    {
        private static readonly Regex UsbIdRegex = new(@"USB\\VID_([0-9A-F]{4})&PID_([0-9A-F]{4})&REV_([0-9A-F]{4})");

        public List<IUsbDevice> Devices { get; private set; }

        public delegate void UsbDeviceEventDelegate(UsbDevice device);

        public delegate void BootloaderDeviceEventDelegate(BootloaderDevice device);
        public delegate void FlashOutputReceivedDelegate(BootloaderDevice device, string data, MessageType type);

        public UsbDeviceEventDelegate usbDeviceConnected;
        public UsbDeviceEventDelegate usbDeviceDisconnected;

        public BootloaderDeviceEventDelegate bootloaderDeviceConnected;
        public BootloaderDeviceEventDelegate bootloaderDeviceDisconnected;
        public FlashOutputReceivedDelegate outputReceived;

        private void EnumerateUsbDevices(bool connected)
        {
            var enumeratedDevices = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE 'USB%'").Get()
                .Cast<ManagementBaseObject>().ToList()
                .Where(d => (d.GetPropertyValue("HardwareID") as string[])?.Any(s => UsbIdRegex.Match(s).Success) ?? false);

            if (connected)
            {
                foreach (var device in enumeratedDevices)
                {
                    var listed = Devices.ToList().Aggregate(false, (curr, d) => curr | d.WmiDevice.Equals(device));

                    if (device != null && !listed)
                    {
                        IUsbDevice usbDevice = CreateDevice(device);
                        Devices.Add(usbDevice);

                        if (usbDevice is BootloaderDevice)
                        {
                            bootloaderDeviceConnected?.Invoke(usbDevice as BootloaderDevice);
                            (usbDevice as BootloaderDevice).outputReceived = FlashOutputReceived;
                        }
                        else
                        {
                            usbDeviceConnected?.Invoke(usbDevice as UsbDevice);
                        }
                    }
                }
            }
            else
            {
                foreach (var device in Devices.ToList())
                {
                    var listed = enumeratedDevices.Aggregate(false, (curr, d) => curr | device.WmiDevice.Equals(d));

                    if (!listed)
                    {
                        Devices.Remove(device);

                        if (device is BootloaderDevice)
                        {
                            bootloaderDeviceDisconnected?.Invoke(device as BootloaderDevice);
                            (device as BootloaderDevice).outputReceived = null;
                        }
                        else
                        {
                            usbDeviceDisconnected?.Invoke(device as UsbDevice);
                        }
                    }
                }
            }
        }

        private void FlashOutputReceived(BootloaderDevice device, string data, MessageType type)
        {
            outputReceived?.Invoke(device, data, type);
        }

        private ManagementEventWatcher deviceConnectedWatcher;
        private ManagementEventWatcher deviceDisconnectedWatcher;

        private static ManagementEventWatcher CreateManagementEventWatcher(string eventType)
        {
            return new ManagementEventWatcher($"SELECT * FROM {eventType} WITHIN 2 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.DeviceID LIKE 'USB%'");
        }

        private void UsbDeviceWmiEvent(object sender, EventArrivedEventArgs e)
        {
            if (e.NewEvent["TargetInstance"] is not ManagementBaseObject _)
            {
                return;
            }

            (sender as ManagementEventWatcher)?.Stop();
            EnumerateUsbDevices(e.NewEvent.ClassPath.ClassName.Equals("__InstanceCreationEvent"));
            (sender as ManagementEventWatcher)?.Start();
        }

        public void Start()
        {
            Devices ??= new List<IUsbDevice>();
            EnumerateUsbDevices(true);

            deviceConnectedWatcher ??= CreateManagementEventWatcher("__InstanceCreationEvent");
            deviceConnectedWatcher.EventArrived += UsbDeviceWmiEvent;
            deviceConnectedWatcher.Start();

            deviceDisconnectedWatcher ??= CreateManagementEventWatcher("__InstanceDeletionEvent");
            deviceDisconnectedWatcher.EventArrived += UsbDeviceWmiEvent;
            deviceDisconnectedWatcher.Start();
        }

        public void Stop()
        {
            if (deviceConnectedWatcher != null)
            {
                deviceConnectedWatcher.Stop();
                deviceConnectedWatcher.EventArrived -= UsbDeviceWmiEvent;
            }

            if (deviceDisconnectedWatcher != null)
            {
                deviceDisconnectedWatcher.Stop();
                deviceDisconnectedWatcher.EventArrived -= UsbDeviceWmiEvent;
            }
        }

        public void Dispose()
        {
            Stop();
            deviceConnectedWatcher?.Dispose();
            deviceDisconnectedWatcher?.Dispose();
            GC.SuppressFinalize(this);
        }

        private static IUsbDevice CreateDevice(ManagementBaseObject d)
        {
            UsbDevice usbDevice = new(d);

            switch (GetDeviceType(usbDevice.VendorId, usbDevice.ProductId, usbDevice.RevisionBcd))
            {
                case BootloaderType.Apm32Dfu:
                    return new Apm32DfuDevice(usbDevice);
                case BootloaderType.AtmelDfu:
                case BootloaderType.QmkDfu:
                    return new AtmelDfuDevice(usbDevice);
                case BootloaderType.AtmelSamBa:
                    return new AtmelSamBaDevice(usbDevice);
                case BootloaderType.AvrIsp:
                    return new AvrIspDevice(usbDevice);
                case BootloaderType.BootloadHid:
                    return new BootloadHidDevice(usbDevice);
                case BootloaderType.Caterina:
                    return new CaterinaDevice(usbDevice);
                case BootloaderType.Gd32VDfu:
                    return new Gd32VDfuDevice(usbDevice);
                case BootloaderType.HalfKay:
                    return new HalfKayDevice(usbDevice);
                case BootloaderType.KiibohdDfu:
                    return new KiibohdDfuDevice(usbDevice);
                case BootloaderType.LufaHid:
                case BootloaderType.QmkHid:
                    return new LufaHidDevice(usbDevice);
                case BootloaderType.LufaMs:
                    return new LufaMsDevice(usbDevice);
                case BootloaderType.Stm32Dfu:
                    return new Stm32DfuDevice(usbDevice);
                case BootloaderType.Stm32Duino:
                    return new Stm32DuinoDevice(usbDevice);
                case BootloaderType.UsbAsp:
                    return new UsbAspDevice(usbDevice);
                case BootloaderType.UsbTinyIsp:
                    return new UsbTinyIspDevice(usbDevice);
                case BootloaderType.Wb32Dfu:
                    return new Wb32DfuDevice(usbDevice);
                default:
                    break;
            }

            return usbDevice;
        }

        private static BootloaderType GetDeviceType(ushort vendorId, ushort productId, ushort revisionBcd)
        {
            switch (vendorId)
            {
                case 0x03EB: // Atmel Corporation
                    switch (productId)
                    {
                        case 0x2045:
                            return BootloaderType.LufaMs;
                        case 0x2067:
                            if (revisionBcd == 0x0936) // Unicode Ψ
                            {
                                return BootloaderType.QmkHid;
                            }

                            return BootloaderType.LufaHid;
                        case 0x2FEF: // ATmega16U2
                        case 0x2FF0: // ATmega32U2
                        case 0x2FF3: // ATmega16U4
                        case 0x2FF4: // ATmega32U4
                        case 0x2FF9: // AT90USB64
                        case 0x2FFA: // AT90USB162
                        case 0x2FFB: // AT90USB128
                            if (revisionBcd == 0x0936) // Unicode Ψ
                            {
                                return BootloaderType.QmkDfu;
                            }

                            return BootloaderType.AtmelDfu;
                        case 0x6124:
                            return BootloaderType.AtmelSamBa;
                    }
                    break;
                case 0x0483: // STMicroelectronics
                    if (productId == 0xDF11)
                    {
                        return BootloaderType.Stm32Dfu;
                    }
                    break;
                case 0x1209: // pid.codes
                    if (productId == 0x2302) // Keyboardio Atreus 2 Bootloader
                    {
                        return BootloaderType.Caterina;
                    }
                    break;
                case 0x16C0: // Van Ooijen Technische Informatica
                    switch (productId)
                    {
                        case 0x0478:
                            return BootloaderType.HalfKay;
                        case 0x0483:
                            return BootloaderType.AvrIsp;
                        case 0x05DC:
                            return BootloaderType.UsbAsp;
                        case 0x05DF:
                            return BootloaderType.BootloadHid;
                    }
                    break;
                case 0x1781: // MECANIQUE
                    if (productId == 0x0C9F)
                    {
                        return BootloaderType.UsbTinyIsp;
                    }
                    break;
                case 0x1B4F: // Spark Fun Electronics
                    switch (productId)
                    {
                        case 0x9203: // Pro Micro 3V3/8MHz
                        case 0x9205: // Pro Micro 5V/16MHz
                        case 0x9207: // LilyPad 3V3/8MHz (and some Pro Micro clones)
                            return BootloaderType.Caterina;
                    }
                    break;
                case 0x1C11: // Input Club Inc.
                    if (productId == 0xB007)
                    {
                        return BootloaderType.KiibohdDfu;
                    }
                    break;
                case 0x1EAF: // Leaflabs
                    if (productId == 0x0003)
                    {
                        return BootloaderType.Stm32Duino;
                    }
                    break;
                case 0x1FFB: // Pololu Corporation
                    if (productId == 0x0101) // A-Star 32U4
                    {
                        return BootloaderType.Caterina;
                    }
                    break;
                case 0x2341: // Arduino SA
                case 0x2A03: // dog hunter AG
                    switch (productId)
                    {
                        case 0x0036: // Leonardo
                        case 0x0037: // Micro
                            return BootloaderType.Caterina;
                    }
                    break;
                case 0x239A: // Adafruit
                    switch (productId)
                    {
                        case 0x000C: // Feather 32U4
                        case 0x000D: // ItsyBitsy 32U4 3V3/8MHz
                        case 0x000E: // ItsyBitsy 32U4 5V/16MHz
                            return BootloaderType.Caterina;
                    }
                    break;
                case 0x28E9: // GigaDevice Semiconductor (Beijing) Inc.
                    switch (productId)
                    {
                        case 0x0189: // GD32VF103 DFU
                            return BootloaderType.Gd32VDfu;
                    }
                    break;
                case 0x314B: // Geehy Semiconductor Co. Ltd.
                    if (productId == 0x0106)
                    {
                        return BootloaderType.Apm32Dfu;
                    }
                    break;
                case 0x342D: // WestBerryTech
                    if (productId == 0xDFA0)
                    {
                        return BootloaderType.Wb32Dfu;
                    }
                    break;
            }

            return BootloaderType.None;
        }
    }
}
