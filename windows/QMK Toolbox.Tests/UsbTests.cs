namespace QMK_Toolbox.Tests
{
    using System;
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

        [SetUp]
        public void Setup()
        {
            flasherMock = new Mock<IFlashing>();
            printerMock = new Mock<IPrinting>();
            searcherFactoryMock = new Mock<IManagementObjectSearcherFactory>();
            
            usb = new UsbTester(flasherMock.Object, printerMock.Object, searcherFactoryMock.Object);
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
