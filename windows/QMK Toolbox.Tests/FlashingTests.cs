namespace QMK_Toolbox.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FlashingTests
    {
        private readonly string caterinaPort = "CaterinaPort";
        private IFlashing flasher;
        private Mock<IProcessRunner> processRunnerMock;
        private Mock<IPrinting> printerMock;
        private Mock<IUsb> usbMock;

        [SetUp]
        public void Setup()
        {
            processRunnerMock = new Mock<IProcessRunner>();
            printerMock = new Mock<IPrinting>();
            usbMock = new Mock<IUsb>();
            flasher = new Flashing(printerMock.Object, processRunnerMock.Object, false);
            flasher.Usb = usbMock.Object;
            flasher.CaterinaPort = caterinaPort;
        }

        [Test]
        public void Flash_CanFlashDfuTrue_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Dfu))
                .Returns(true);

            var orderedCalls = new List<ValueTuple<string, string>>();

            processRunnerMock
                .Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((command, args) => orderedCalls.Add((command, args)));

            flasher.Flash("Garbage", "Junk.txt");

            orderedCalls.Should().HaveCount(3);
            orderedCalls[0].Should().Be(("dfu-programmer.exe", "Garbage erase --force"));
            orderedCalls[1].Should().Be(("dfu-programmer.exe", "Garbage flash \"Junk.txt\""));
            orderedCalls[2].Should().Be(("dfu-programmer.exe", "Garbage reset"));
        }

        [Test]
        public void Flash_CanFlashDfuFalse_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Dfu))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p => p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Flash_CanFlashCaterinaTrue_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Caterina))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.bin");

            processRunnerMock.Verify(p =>
                p.Run("avrdude.exe", "-p Garbage -c avr109 -U flash:w:\"Junk.bin\":i -P CaterinaPort"), Times.Once());
        }

        [Test]
        public void Flash_CanFlashCaterinaFalse_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Caterina))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.bin");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Flash_CanFlashHalfkayTrue_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Halfkay))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.bin");

            processRunnerMock.Verify(p =>
                p.Run("teensy_loader_cli.exe", "-mmcu=Garbage \"Junk.bin\" -v"), Times.Once());
        }

        [Test]
        public void Flash_CanFlashHalfkayFalse_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Halfkay))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.bin");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Flash_CanFlashStm32TrueValidFile_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Stm32))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.BIN");

            processRunnerMock.Verify(p =>
                p.Run("dfu-util.exe", "-a 0 -d 0483:df11 -s 0x08000000:leave -D \"Junk.BIN\""), Times.Once());
        }

        [Test]
        public void Flash_CanFlashStm32TrueInvalidFile_ErrorMessageIsWritten()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Stm32))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            printerMock.Verify(p =>
                p.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error), Times.Once());
        }
        
        [Test]
        public void Flash_CanFlashStm32FalseValidFile_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Stm32))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.BIN");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Flash_CanFlashKiibohdTrueValidFile_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Kiibohd))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.BIN");

            processRunnerMock.Verify(p =>
                p.Run("dfu-util.exe", "-D \"Junk.BIN\""), Times.Once());
        }

        [Test]
        public void Flash_CanFlashKiibohdTrueInvalidFile_ErrorMessageIsWritten()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Kiibohd))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

            printerMock.Verify(p =>
                p.Print("Only firmware files in .bin format can be flashed with dfu-util!", MessageType.Error), Times.Once());
        }

        [Test]
        public void Flash_CanFlashKiibohdFalseValidFile_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Kiibohd))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.BIN");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Flash_CanFlashAvrIspTrue_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.AvrIsp))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run("avrdude.exe", "-p Garbage -c avrisp -U flash:w:\"Junk.txt\":i -P CaterinaPort"), Times.Once());
        }

        [Test]
        public void Flash_CanFlashAvrIspFalse_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.AvrIsp))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Flash_CanFlashUsbAspTrue_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.UsbAsp))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run("avrdude.exe", "-p Garbage -c usbasp -U flash:w:\"Junk.txt\":i"), Times.Once());
        }

        [Test]
        public void Flash_CanFlashUsbAspFalse_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.UsbAsp))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Flash_CanFlashUsbTinyTrue_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.UsbTiny))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run("avrdude.exe", "-p Garbage -c usbtiny -U flash:w:\"Junk.txt\":i -P CaterinaPort"), Times.Once());
        }

        [Test]
        public void Flash_CanFlashUsbTinyFalse_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.UsbTiny))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Flash_CanFlashBootloadHidTrue_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.BootloadHid))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run("bootloadHID.exe", "-r \"Junk.txt\""), Times.Once());
        }

        [Test]
        public void Flash_CanFlashBootloadHidFalse_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.BootloadHid))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Flash_CanFlashAtmelSamBaTrue_FlashIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.AtmelSamBa))
                .Returns(true);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run("mdloader_windows.exe", "-p CaterinaPort -D \"Junk.txt\" --restart"), Times.Once());
        }

        [Test]
        public void Flash_CanFlashAtmelSamBaFalse_FlashIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.AtmelSamBa))
                .Returns(false);

            flasher.Flash("Garbage", "Junk.txt");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Reset_CanFlashDfuTrue_ResetIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Dfu))
                .Returns(true);

            flasher.Reset("Garbage");

            processRunnerMock.Verify(p =>
                p.Run("dfu-programmer.exe", "Garbage reset"), Times.Once());
        }

        [Test]
        public void Reset_CanFlashDfuFalse_ResetIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Dfu))
                .Returns(false);

            flasher.Reset("Garbage");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Reset_CanFlashHalfkayTrue_ResetIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Halfkay))
                .Returns(true);

            flasher.Reset("Garbage");

            processRunnerMock.Verify(p =>
                p.Run("teensy_loader_cli.exe", "-mmcu=Garbage -bv"), Times.Once());
        }

        [Test]
        public void Reset_CanFlashHalfkayFalse_ResetIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Halfkay))
                .Returns(false);

            flasher.Reset("Garbage");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Reset_CanFlashBootloadHidTrue_ResetIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.BootloadHid))
                .Returns(true);

            flasher.Reset("Garbage");

            processRunnerMock.Verify(p =>
                p.Run("bootloadHID.exe", "-r"), Times.Once());
        }

        [Test]
        public void Reset_CanFlashBootloadHidFalse_ResetIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.BootloadHid))
                .Returns(false);

            flasher.Reset("Garbage");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Reset_CanFlashAtmelSamBaTrue_ResetIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.AtmelSamBa))
                .Returns(true);

            flasher.Reset("Garbage");

            processRunnerMock.Verify(p =>
                p.Run("mdloader_windows.exe", "-p CaterinaPort --restart"), Times.Once());
        }

        [Test]
        public void Reset_CanFlashAtmelSamBaFalse_ResetIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.AtmelSamBa))
                .Returns(false);

            flasher.Reset("Garbage");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ClearEeprom_CanFlashDfuTrue_ClearIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Dfu))
                .Returns(true);

            flasher.ClearEeprom("Garbage");

            processRunnerMock.Verify(p =>
                p.Run("dfu-programmer.exe", "Garbage flash --force --eeprom \"reset.eep\""), Times.Once());
        }

        [Test]
        public void ClearEeprom_CanFlashDfuFalse_ClearIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Dfu))
                .Returns(false);

            flasher.ClearEeprom("Garbage");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ClearEeprom_CanFlashCaterinaTrue_ClearIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Caterina))
                .Returns(true);

            flasher.ClearEeprom("Garbage");

            processRunnerMock.Verify(p =>
                p.Run("avrdude.exe", "-p Garbage -c avr109 -U eeprom:w:\"reset.eep\":i -P CaterinaPort"), Times.Once());
        }

        [Test]
        public void ClearEeprom_CanFlashCaterinaFalse_ClearIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.Caterina))
                .Returns(false);

            flasher.ClearEeprom("Garbage");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ClearEeprom_CanFlashUsbAspTrue_ClearIsPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.UsbAsp))
                .Returns(true);

            flasher.ClearEeprom("Garbage");

            processRunnerMock.Verify(p =>
                p.Run("avrdude.exe", "-p Garbage -c usbasp -U eeprom:w:\"reset.eep\":i -P CaterinaPort"), Times.Once());
        }

        [Test]
        public void ClearEeprom_CanFlashUsbAspFalse_ClearIsNotPerformed()
        {
            usbMock
                .Setup(u => u.CanFlash(Chipset.UsbAsp))
                .Returns(false);

            flasher.ClearEeprom("Garbage");

            processRunnerMock.Verify(p =>
                p.Run(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }
}
