//  Created by Mike Cooper on 2/20/20.
//  Copyright © 2020 Mike Cooper. This code is licensed under MIT license (see LICENSE.md for details).

namespace QMK_Toolbox.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using QMK_Toolbox.Wmi;

    [TestFixture]
    public class UsbTests
    {
        private UsbTester usb;
        private Mock<IFlashing> flasherMock;
        private Mock<IPrinting> printerMock;
        private Mock<IManagementObjectSearcherFactory> searcherFactoryMock;
        private Mock<IManagementObjectSearcher> objectSearcherMock;

        [SetUp]
        public void Setup()
        {
            flasherMock = new Mock<IFlashing>();
            printerMock = new Mock<IPrinting>();
            searcherFactoryMock = new Mock<IManagementObjectSearcherFactory>();
            objectSearcherMock = new Mock<IManagementObjectSearcher>();

            var deviceMock = new Mock<IManagementBaseObject>();
            deviceMock
                .Setup(m => m.GetPropertyValue("PNPDeviceID"))
                .Returns("Id1");
            deviceMock
                .Setup(m => m.GetPropertyValue("DeviceID"))
                .Returns("ComPort");

            SetSearcherMockReturn(new List<IManagementBaseObject> { deviceMock.Object });

            usb = new UsbTester(flasherMock.Object, printerMock.Object, searcherFactoryMock.Object);

            searcherFactoryMock
                .Setup(sf => sf.Create(It.IsAny<string>()))
                .Returns(objectSearcherMock.Object);
        }

        [Test]
        public void AreDevicesAvailable_NoDevices_ReturnsFalse()
        {
            usb.AreDevicesAvailable().Should().BeFalse();
        }

        [Test]
        public void AreDevicesAvailable_DeviceAvailableAtFirstIndex_ReturnsTrue()
        {
            usb.DevicesAvailable[0] = 5;

            usb.AreDevicesAvailable().Should().BeTrue();
        }

        [Test]
        public void AreDevicesAvailable_DeviceAvailableAtLastIndex_ReturnsTrue()
        {
            usb.DevicesAvailable[(int)Chipset.NumberOfChipsets - 1] = 22;

            usb.AreDevicesAvailable().Should().BeTrue();
        }

        [Test]
        public void CanFlash_NoDevicesAvailable_ReturnsFalse()
        {
            usb.DevicesAvailable[(int)Chipset.Dfu] = 0;

            usb.CanFlash(Chipset.Dfu).Should().BeFalse();
        }

        [Test]
        public void CanFlash_DevicesAvailable_ReturnsTrue()
        {
            usb.DevicesAvailable[(int)Chipset.Dfu] = 5;

            usb.CanFlash(Chipset.Dfu).Should().BeTrue();
        }

        [Test]
        public void MatchVid_GivenMatch_ReturnsTrue()
        {
            var result = usb.MatchVid("USB1234+-VID_00FFabcdefghiklmnop", 255);

            result.Should().BeTrue();
        }

        [Test]
        public void MatchVid_GivenNonMatch_ReturnsFalse()
        {
            var result = usb.MatchVid("asdf123400FF", 255);

            result.Should().BeFalse();
        }

        [Test]
        public void MatchPid_GivenMatch_ReturnsTrue()
        {
            var result = usb.MatchPid("USB1234+-PID_00FFabcdefghiklmnop", 255);

            result.Should().BeTrue();
        }

        [Test]
        public void MatchPid_GivenNonMatch_ReturnsFalse()
        {
            var result = usb.MatchPid("asdf123400FF", 255);

            result.Should().BeFalse();
        }

        [Test]
        public void MatchRev_GivenMatch_ReturnsTrue()
        {
            var result = usb.MatchRev("USB1234+-REV_00FFabcdefghiklmnop", 255);

            result.Should().BeTrue();
        }

        [Test]
        public void MatchRev_GivenNonMatch_ReturnsFalse()
        {
            var result = usb.MatchRev("asdf123400FF", 255);

            result.Should().BeFalse();
        }

        [Test]
        public void GetComPort_NoPortsFound_ReturnsEmptyString()
        {
            SetSearcherMockReturn(new List<IManagementBaseObject>());

            var result = usb.GetComPort(new BaseObjectMock(string.Empty));

            result.Should().BeEmpty();

            searcherFactoryMock.Verify(s => s.Create("SELECT * FROM Win32_SerialPort"));
        }

        [Test]

        public void GetComPort_PortFoundNoMatch_ReturnsEmptyString()
        {
            var instanceMock = new Mock<IManagementBaseObject>();
            instanceMock
                .Setup(m => m.GetPropertyValue("PNPDeviceID"))
                .Returns("Id2");

            var result = usb.GetComPort(instanceMock.Object);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetComPort_PortFoundMatch_ReturnsCorrectValue()
        {
            var instanceMock = GetInstanceMock(pnpHardwareId: "Id1");

            var result = usb.GetComPort(instanceMock.Object);

            result.Should().Be("ComPort");
        }

        [Test]
        public void DetectBootloader_HardwareIdIsNull_ReturnsFalse()
        {
            var instanceMock = new Mock<IManagementBaseObject>();
            instanceMock
                .Setup(i => i.GetPropertyValue("HardwareID"))
                .Returns(null);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeFalse();
        }

        [Test]
        public void DetectBootloader_HardwareIdIsEmpty_ReturnsFalse()
        {
            var instanceMock = GetInstanceMock();

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeFalse();
        }

        [Test]
        public void DetectBootloader_NoMatch_ReturnsFalse()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x0000, productId: 0x0000);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeFalse();
        }

        [Test]
        public void DetectBootloader_AtmelSamBaConnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x03EB, productId: 0x6124, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.AtmelSamBa].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("Atmel SAM-BA device connected: Manufacturer Name (03EB:6124:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_AtmelSamBaDisconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x03EB, productId: 0x6124, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.AtmelSamBa].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("Atmel SAM-BA device disconnected: Manufacturer Name (03EB:6124:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_DfuConnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x03EB, productId: 0x6155, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Dfu].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("DFU device connected: Manufacturer Name (03EB:6155:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_DfuDisconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x03EB, productId: 0x6155, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Dfu].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("DFU device disconnected: Manufacturer Name (03EB:6155:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_CaterinaConnected_CorrectValuesAreSet()
        {
            var result = usb.DetectBootloader(GetInstanceMock(vendorId: 0x2341, productId: 0x6155, rev: 0x1234).Object, true);

            result &= usb.DetectBootloader(GetInstanceMock(vendorId: 0x1B4F, productId: 0x6155, rev: 0x1234).Object, true);

            result &= usb.DetectBootloader(GetInstanceMock(vendorId: 0x239A, productId: 0x6155, rev: 0x1234).Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Caterina].Should().Be(3);
            usb.DevicesAvailable.Sum().Should().Be(3);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("Caterina device connected: Manufacturer Name (2341:6155:1234)", MessageType.Bootloader));
        }

        [Test]
        public void DetectBootloader_CaterinaDisconnected_CorrectValuesAreSet()
        {
            var result = usb.DetectBootloader(GetInstanceMock(vendorId: 0x2341, productId: 0x6155, rev: 0x1234).Object, false);

            result &= usb.DetectBootloader(GetInstanceMock(vendorId: 0x1B4F, productId: 0x6155, rev: 0x1234).Object, false);

            result &= usb.DetectBootloader(GetInstanceMock(vendorId: 0x239A, productId: 0x6155, rev: 0x1234).Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Caterina].Should().Be(-3);
            usb.DevicesAvailable.Sum().Should().Be(-3);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("Caterina device disconnected: Manufacturer Name (2341:6155:1234)", MessageType.Bootloader));
        }

        [Test]
        public void DetectBootloader_HalfkayConnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x16C0, productId: 0x0478, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Halfkay].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("Halfkay device connected: Manufacturer Name (16C0:0478:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_HalfkayDisconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x16C0, productId: 0x0478, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Halfkay].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("Halfkay device disconnected: Manufacturer Name (16C0:0478:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_Stm32Connected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x0483, productId: 0xDF11, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Stm32].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("STM32 device connected: Manufacturer Name (0483:DF11:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_Stm32Disconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x0483, productId: 0xDF11, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Stm32].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("STM32 device disconnected: Manufacturer Name (0483:DF11:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_KiibohdConnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x1C11, productId: 0xB007, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Kiibohd].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("Kiibohd device connected: Manufacturer Name (1C11:B007:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_KiibohdDisconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x1C11, productId: 0xB007, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.Kiibohd].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("Kiibohd device disconnected: Manufacturer Name (1C11:B007:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_AvrIspConnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x16C0, productId: 0x0483, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.AvrIsp].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("AVRISP device connected: Manufacturer Name (16C0:0483:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_AvrIspDisconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x16C0, productId: 0x0483, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.AvrIsp].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("AVRISP device disconnected: Manufacturer Name (16C0:0483:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_UsbAspConnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x16C0, productId: 0x05DC, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.UsbAsp].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("USBAsp device connected: Manufacturer Name (16C0:05DC:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_UsbAspDisconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x16C0, productId: 0x05DC, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.UsbAsp].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("USBAsp device disconnected: Manufacturer Name (16C0:05DC:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_UsbTinyConnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x1781, productId: 0x0C9F, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.UsbTiny].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("USB Tiny device connected: Manufacturer Name (1781:0C9F:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_UsbTinyDisconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x1781, productId: 0x0C9F, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.UsbTiny].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = "ComPort");

            printerMock.Verify(p =>
                p.Print("USB Tiny device disconnected: Manufacturer Name (1781:0C9F:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_BootloadHidConnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x16C0, productId: 0x05DF, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, true);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.BootloadHid].Should().Be(1);
            usb.DevicesAvailable.Sum().Should().Be(1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("BootloadHID device connected: Manufacturer Name (16C0:05DF:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloader_BootloadHidDisconnected_CorrectValuesAreSet()
        {
            var instanceMock = GetInstanceMock(vendorId: 0x16C0, productId: 0x05DF, rev: 0x1234);

            var result = usb.DetectBootloader(instanceMock.Object, false);

            result.Should().BeTrue();

            usb.DevicesAvailable[(int)Chipset.BootloadHid].Should().Be(-1);
            usb.DevicesAvailable.Sum().Should().Be(-1);

            flasherMock.VerifySet(f => f.CaterinaPort = It.IsAny<string>(), Times.Never());

            printerMock.Verify(p =>
                p.Print("BootloadHID device disconnected: Manufacturer Name (16C0:05DF:1234)", MessageType.Bootloader), Times.Once());
        }

        [Test]
        public void DetectBootloaderFromCollection_NoMatches_ReturnsFalse()
        {
            var managementObjects = new List<IManagementBaseObject>
            {
                GetInstanceMock(vendorId: 0x0000, productId: 0x0000).Object,
                GetInstanceMock(vendorId: 0x0001, productId: 0x0001).Object,
                GetInstanceMock(vendorId: 0x0002, productId: 0x0002).Object,
            };

            var result = usb.DetectBootloaderFromCollection(managementObjects);

            result.Should().BeFalse();
        }

        [Test]
        public void DetectBootloaderFromCollection_BootloadHidFound_ReturnsTrue()
        {
            var managementObjects = new List<IManagementBaseObject>
            {
                GetInstanceMock(vendorId: 0x0000, productId: 0x0000).Object,
                GetInstanceMock(vendorId: 0x0001, productId: 0x0001).Object,
                GetInstanceMock(vendorId: 0x0002, productId: 0x0002).Object,
                GetInstanceMock(vendorId: 0x16C0, productId: 0x05DF, rev: 0x1234).Object,
            };

            var result = usb.DetectBootloaderFromCollection(managementObjects);

            result.Should().BeTrue();
        }

        private Mock<IManagementBaseObject> GetInstanceMock(string pnpHardwareId = "Id1", ushort? vendorId = null, ushort? productId = null, ushort? rev = null)
        {
            var instanceMock = new Mock<IManagementBaseObject>();
            instanceMock
                .Setup(i => i.GetPropertyValue("PNPDeviceID"))
                .Returns(pnpHardwareId);

            instanceMock
                .Setup(i => i.GetPropertyValue("Manufacturer"))
                .Returns("Manufacturer");

            instanceMock
                .Setup(i => i.GetPropertyValue("Name"))
                .Returns("Name");

            if (vendorId != null
                || productId != null
                || rev != null)
            {
                instanceMock
                    .Setup(i => i.GetPropertyValue("HardwareID"))
                    .Returns(new string[] { GetDeviceId(vendorId: vendorId ?? 0, productId: productId ?? 0, rev: rev ??
                        0) });
            }
            else
            {
                instanceMock
                    .Setup(i => i.GetPropertyValue("HardwareID"))
                    .Returns(Array.Empty<string>());
            }

            return instanceMock;
        }

        private void SetSearcherMockReturn(List<IManagementBaseObject> list)
        {
            var objectCollectionMock = new Mock<IManagementObjectCollection>();

            objectCollectionMock
                .Setup(c => c.GetEnumerator())
                .Returns(list.GetEnumerator());

            objectSearcherMock
                .Setup(s => s.Get())
                .Returns(objectCollectionMock.Object);
        }

        private string GetDeviceId(ushort vendorId, ushort productId, ushort rev = 0) =>
            $"USB_VID_{vendorId:X4}_PID_{productId:X4}_REV_{rev:X4}_";

        private class BaseObjectMock : IManagementBaseObject
        {
            private readonly string propertyValue;

            public BaseObjectMock(string propertyValue)
            {
                this.propertyValue = propertyValue;
            }

            public object GetPropertyValue(string propertyName) => propertyValue;
        }

        public class UsbTester : Usb
        {
            public UsbTester(IFlashing flasher, IPrinting printer, IManagementObjectSearcherFactory searcherFactory)
                : base(flasher, printer, searcherFactory)
            {
            }

            public int[] DevicesAvailable => _devicesAvailable;

            public new bool MatchVid(string did, ushort vid) => Usb.MatchVid(did, vid);

            public new bool MatchPid(string did, ushort pid) => Usb.MatchPid(did, pid);

            public new bool MatchRev(string did, ushort rev) => Usb.MatchRev(did, rev);
        }
    }
}
