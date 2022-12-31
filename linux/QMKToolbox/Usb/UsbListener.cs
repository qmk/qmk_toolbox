using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;
using QMK_Toolbox.Usb.Bootloader;

// ReSharper disable StringLiteralTypo

namespace QMK_Toolbox.Usb;

internal class UsbListener
{
    public delegate void BootloaderDeviceEventDelegate(BootloaderDevice device);

    public delegate void FlashOutputReceivedDelegate(BootloaderDevice device, string data, MessageType type);

    private readonly List<KnownHidDevice> _attachedDevices = new();
    private readonly List<KnownHidDevice> _knownDevices = new();
    private Task _backgroundTask;

    public BootloaderDeviceEventDelegate BootloaderDeviceConnected;
    public BootloaderDeviceEventDelegate BootloaderDeviceDisconnected;

    public FlashOutputReceivedDelegate OutputReceived;

    public UsbListener()
    {
        PopulateKnownDevices();
    }

    public void Start()
    {
        _backgroundTask = new Task(CheckKnownDevicesConnectionState, TaskCreationOptions.LongRunning);
        _backgroundTask.Start();
    }


    private void FlashOutputReceived(BootloaderDevice device, string data, MessageType type)
    {
        OutputReceived?.Invoke(device, data, type);
    }

    private static void GetNewUsbDevices(List<UsbDeviceNameRecord> listNew)
    {
        using UsbContext context = new UsbContext();
        var allDevices = context.List();

        foreach (var dev in allDevices)
        {
            var deviceRevisionBcd = dev.Info.Device;
            if (listNew.Any(x => x.ProductId == dev.ProductId && x.VendorId == dev.VendorId))
                continue;
            {
                var manufacturer = "n/a";

                listNew.Add(new UsbDeviceNameRecord()
                {
                    VendorId = dev.VendorId, ProductId = dev.ProductId,
                    DeviceRevisionBcd = deviceRevisionBcd, ManufacturerName = manufacturer
                });
            }
        }
    }
    
    private void CheckKnownDevicesConnectionState()
    {
        List<UsbDeviceNameRecord> listOld = new();
        List<UsbDeviceNameRecord> listNew = new();
        var helper = new UsbDeviceRecordHelper();
        
        GetNewUsbDevices(listNew);

        while (true)
        {
            listOld = new ();
            listOld.AddRange(listNew);
            listNew = new();
    
            // we are polling 2x per sec for a list of usb devices on this background
            // thread.
            Thread.Sleep(500);

            GetNewUsbDevices(listNew);

            var added = listNew.Except(listOld, new UsbDeviceRecordComparer()).ToList();
            var removed = listOld.Except(listNew, new UsbDeviceRecordComparer()).ToList();
            if (added.Count > 0)
            {
                var item = added.FirstOrDefault();
                Debug.Assert(item!=null);

                var device =_knownDevices.FirstOrDefault(x=>x.VendorId == item.VendorId && 
                                                            x.ProductId == item.ProductId);
                if (device == null)
                {
                   Console.WriteLine(@"Could not find device with VendorId: {0}, ProductId: {1}", 
                       item.VendorId, item.ProductId);
                   continue;
                }
                
                // we take the device bcd from the actual device, not from our table
                // the BootloaderDevice subclasses will handle it.
                device.RevisionBcd = item.DeviceRevisionBcd;
                var bootloaderDevice = CreateDevice(device);
                _attachedDevices.Add(device);
                BootloaderDeviceConnected?.Invoke(bootloaderDevice);
                bootloaderDevice.OutputReceived = FlashOutputReceived;
            }

            if (removed.Count > 0)
            {
                var item = removed.FirstOrDefault();
                Debug.Assert(item != null);
                var device =_knownDevices.FirstOrDefault(x=>x.VendorId == item.VendorId && 
                                                            x.ProductId == item.ProductId);
                if (device == null)
                    continue;
                var bootloaderDevice = CreateDevice(device);
                if (bootloaderDevice != null)
                {
                    BootloaderDeviceDisconnected?.Invoke(bootloaderDevice);
                    bootloaderDevice.OutputReceived = null;
                }
            }

            added.Clear();
            removed.Clear();
        }
    }
    
    
    private static BootloaderDevice CreateDevice(KnownHidDevice device)
    {
        switch (device.BootloaderType)
        {
            case BootloaderType.Apm32Dfu:
                return new Apm32DfuDevice(device);
            case BootloaderType.AtmelDfu:
            case BootloaderType.QmkDfu:
                return new AtmelDfuDevice(device);
            case BootloaderType.AtmelSamBa:
                return new AtmelSamBaDevice(device);
            case BootloaderType.AvrIsp:
                return new AvrIspDevice(device);
            case BootloaderType.BootloadHid:
                return new BootloadHidDevice(device);
            case BootloaderType.Caterina:
                return new CaterinaDevice(device);
            case BootloaderType.Gd32VDfu:
                return new Gd32VDfuDevice(device);
            case BootloaderType.HalfKay:
                return new HalfKayDevice(device);
            case BootloaderType.KiibohdDfu:
                return new KiibohdDfuDevice(device);
            case BootloaderType.LufaHid:
            case BootloaderType.QmkHid:
                return new LufaHidDevice(device);
            case BootloaderType.LufaMs:
                return new LufaMsDevice(device);
            case BootloaderType.Stm32Dfu:
                return new Stm32DfuDevice(device);
            case BootloaderType.Stm32Duino:
                return new Stm32DuinoDevice(device);
            case BootloaderType.UsbAsp:
                return new UsbAspDevice(device);
            case BootloaderType.UsbTinyIsp:
                return new UsbTinyIspDevice(device);
            case BootloaderType.Wb32Dfu:
                return new Wb32DfuDevice(device);
        }
        return null;
    }

    private void PopulateKnownDevices()
    {
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2045,
            VendorName = "Amtel", ProductName = "Bootloader", BootloaderType = BootloaderType.LufaMs
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2067, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "Bootloader QmkHid", BootloaderType = BootloaderType.QmkHid
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2067,
            VendorName = "Amtel", ProductName = "Bootloader QmkHid", BootloaderType = BootloaderType.LufaHid
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2fef,
            VendorName = "Amtel", ProductName = "ATMega16U2", BootloaderType = BootloaderType.AtmelDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ff0,
            VendorName = "Amtel", ProductName = "ATmega32U2", BootloaderType = BootloaderType.AtmelDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ff3,
            VendorName = "Amtel", ProductName = "ATmega16U4", BootloaderType = BootloaderType.AtmelDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ff4,
            VendorName = "Amtel", ProductName = "ATmega32U4", BootloaderType = BootloaderType.AtmelDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ff9,
            VendorName = "Amtel", ProductName = "AT90USB64", BootloaderType = BootloaderType.AtmelDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ffa,
            VendorName = "Amtel", ProductName = "AT90USB64r", BootloaderType = BootloaderType.AtmelDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ffb,
            VendorName = "Amtel", ProductName = "AT90USB128", BootloaderType = BootloaderType.AtmelDfu
        });
        //
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2067, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "Bootloader QmkHid", BootloaderType = BootloaderType.LufaHid
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2fef, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "ATMega16U2", BootloaderType = BootloaderType.QmkDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ff0, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "ATmega32U2", BootloaderType = BootloaderType.QmkDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ff3, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "ATmega16U4", BootloaderType = BootloaderType.QmkDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ff4, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "ATmega32U4", BootloaderType = BootloaderType.QmkDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ff9, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "AT90USB64", BootloaderType = BootloaderType.QmkDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ffa, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "AT90USB64r", BootloaderType = BootloaderType.QmkDfu
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x03eb, ProductId = 0x2ffb, RevisionBcd = 0x0936,
            VendorName = "Amtel", ProductName = "AT90USB128", BootloaderType = BootloaderType.QmkDfu
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x0438, ProductId = 0x0000,
            VendorName = "STMicroelectronics", ProductName = "STM Device in DFU Mode",
            BootloaderType = BootloaderType.Stm32Dfu
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x2045, ProductId = 0x0000,
            VendorName = "Generic", ProductName = "Keyboardio Atreus 2 Bootloader",
            BootloaderType = BootloaderType.Caterina
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1209, ProductId = 0x0478,
            VendorName = "Van Ooijen Technische Informatica", ProductName = "Teensy Halfkay Bootloader",
            BootloaderType = BootloaderType.HalfKay
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1209, ProductId = 0x483,
            VendorName = "Van Ooijen Technische Informatica", ProductName = "Teensyduino Serial",
            BootloaderType = BootloaderType.AvrIsp
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1209, ProductId = 0x05dc,
            VendorName = "Van Ooijen Technische Informatica", ProductName = "Teensy Halfkay Bootloader",
            BootloaderType = BootloaderType.UsbAsp
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1209, ProductId = 0x5df,
            VendorName = "Van Ooijen Technische Informatica", ProductName = "SharedID for USB",
            BootloaderType = BootloaderType.BootloadHid
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 1781, ProductId = 0x0,
            VendorName = "Mechanique", ProductName = "USB Tiny",
            BootloaderType = BootloaderType.UsbTinyIsp
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1b4f, ProductId = 0x9203,
            VendorName = "Spark Fun Electronics", ProductName = "Pro Micro 3V3/8MHz",
            BootloaderType = BootloaderType.Caterina
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1b4f, ProductId = 0x9205,
            VendorName = "Spark Fun Electronics", ProductName = "Pro Micro 5V/16MHz",
            BootloaderType = BootloaderType.Caterina
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1b4f, ProductId = 0x9207,
            VendorName = "Spark Fun Electronics", ProductName = "LilyPad 3V3/8MHz (and some Pro Micro clones)",
            BootloaderType = BootloaderType.Caterina
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1c11, ProductId = 0xb007,
            VendorName = "Input Club Inc.", ProductName = "Kiibohd DFU",
            BootloaderType = BootloaderType.KiibohdDfu
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1eaf, ProductId = 0x0003,
            VendorName = "Leaf Labs", ProductName = "A-Star 32U4",
            BootloaderType = BootloaderType.Stm32Duino
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x1ffb, ProductId = 0x0003,
            VendorName = "Pololu Corporation", ProductName = "STM32Duino",
            BootloaderType = BootloaderType.Caterina
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x2341, ProductId = 0x0036,
            VendorName = "Arduino SA", ProductName = "Arduino Leonardo (bootloader)",
            BootloaderType = BootloaderType.Caterina
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x2341, ProductId = 0x0037,
            VendorName = "Arduino SA", ProductName = "Arduino Micro (bootloader)",
            BootloaderType = BootloaderType.Caterina
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x2a03, ProductId = 0x0036,
            VendorName = "dog hunter AG", ProductName = "Arduino Leonardo (bootloader)",
            BootloaderType = BootloaderType.Caterina
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x2a03, ProductId = 0x0037,
            VendorName = "dog hunter AG", ProductName = "Arduino Micro (bootloader)",
            BootloaderType = BootloaderType.Caterina
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x2398, ProductId = 0x000c,
            VendorName = "AdaFruit", ProductName = "Feather 32U4",
            BootloaderType = BootloaderType.Caterina
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x2398, ProductId = 0x000d,
            VendorName = "AdaFruit", ProductName = "ItsyBitsy 32U4 3V3/8MHz",
            BootloaderType = BootloaderType.Caterina
        });
        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x2398, ProductId = 0x000e,
            VendorName = "AdaFruit", ProductName = "ItsyBitsy 32U4 5V/16MHz",
            BootloaderType = BootloaderType.Caterina
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x28e9, ProductId = 0x0189,
            VendorName = "GDMicroelectronics", ProductName = "GD32 DFU Bootloader (Longan Nano)",
            BootloaderType = BootloaderType.Gd32VDfu
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x314b, ProductId = 0x0106,
            VendorName = "Geehy Semiconductor Co. Ltd.", ProductName = "APM32 DFU Bootloader",
            BootloaderType = BootloaderType.Apm32Dfu
        });

        _knownDevices.Add(new KnownHidDevice
        {
            VendorId = 0x3420, ProductId = 0x0003,
            VendorName = "WestBerryTech", ProductName = "WB32 DFU Bootloader",
            BootloaderType = BootloaderType.Wb32Dfu
        });
    }
}