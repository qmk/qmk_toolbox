namespace QMK_Toolbox.Tests
{
    using System;
    using System.Drawing;
    using System.Linq;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class PrintingTests
    {
        private PrintingTester printing;

        [SetUp]
        public void Setup()
        {
            printing = new PrintingTester();
        }

        [Test]
        public void Format_InfoMessage_ReturnsCorrectResult()
        {
            var result = printing.Format("Test string", MessageType.Info);

            result.Item1.Should().Be("*** Test string\n");
            result.Item2.Should().Be(Color.White);
        }

        [Test]
        public void Format_CommandMessage_ReturnsCorrectResult()
        {
            var result = printing.Format("Test ABCDEFG", MessageType.Command);

            result.Item1.Should().Be(">>> Test ABCDEFG\n");
            result.Item2.Should().Be(Color.White);
        }

        [Test]
        public void Format_BootloaderMessage_ReturnsCorrectResult()
        {
            var result = printing.Format("Test 1234", MessageType.Bootloader);

            result.Item1.Should().Be("*** Test 1234\n");
            result.Item2.Should().Be(Color.Yellow);
        }

        [Test]
        public void Format_ErrorMessage_ReturnsCorrectResult()
        {
            var result = printing.Format("ABCDEFG 1234", MessageType.Error);

            result.Item1.Should().Be("  ! ABCDEFG 1234\n");
            result.Item2.Should().Be(Color.Red);
        }

        [Test]
        public void Format_HidMessage_ReturnsCorrectResult()
        {
            var result = printing.Format("1234 1234 56", MessageType.Hid);

            result.Item1.Should().Be("*** 1234 1234 56\n");
            result.Item2.Should().Be(Color.LightSkyBlue);
        }

        [Test]
        public void Format_LastCharacterNotNewline_NewlineIsPrepended()
        {
            printing.LastCharacter = 'a';

            var result = printing.Format("1234", MessageType.Command);

            result.Item1.Should().Be("\n>>> 1234\n");
        }

        [Test]
        public void Format_WhenCalled_LastMessageTypeIsSet()
        {
            printing.Format("1234", MessageType.Command);

            printing.LastMessageType.Should().Be(MessageType.Command);
        }

        [Test]
        public void FormatResponse_InfoMessage_ReturnsCorrectResult()
        {
            var result = printing.FormatResponse("1234 5678", MessageType.Info);

            result.Item1.Should().Be("    1234 5678");
            result.Item2.Should().Be(Color.LightGray);
        }

        [Test]
        public void FormatResponse_CommandMessage_ReturnsCorrectResult()
        {
            var result = printing.FormatResponse("1111 2222", MessageType.Command);

            result.Item1.Should().Be("    1111 2222");
            result.Item2.Should().Be(Color.LightGray);
        }

        [Test]
        public void FormatResponse_BootloaderMessage_ReturnsCorrectResult()
        {
            var result = printing.FormatResponse("1234 1234 1234", MessageType.Bootloader);

            result.Item1.Should().Be("    1234 1234 1234");
            result.Item2.Should().Be(Color.Yellow);
        }

        [Test]
        public void FormatResponse_ErrorMessage_ReturnsCorrectResult()
        {
            var result = printing.FormatResponse("aaa bbb ccc", MessageType.Error);

            result.Item1.Should().Be("    aaa bbb ccc");
            result.Item2.Should().Be(Color.DarkRed);
        }

        [Test]
        public void FormatResponse_HidMessage_ReturnsCorrectResult()
        {
            var result = printing.FormatResponse("asdf", MessageType.Hid);

            result.Item1.Should().Be("  > asdf");
            result.Item2.Should().Be(Color.SkyBlue);
        }

        [Test]
        public void FormatResponse_GivenMessageWithNewlines_NewlinesAreReformatted()
        {
            var result = printing.FormatResponse("\n1234\n5678", MessageType.Command);

            result.Item1.Should().Be("    \n    1234\n    5678");
        }

        [Test]
        public void FormatResponse_GivenMessageWithNewlineAtEnd_NewlinesAreReformatted()
        {
            var result = printing.FormatResponse("\n1234\n5678\n", MessageType.Command);

            result.Item1.Should().Be("    \n    1234\n    5678\n");
        }

        [Test]
        public void FormatResponse_WhenCalled_LastMessageTypeIsSet()
        {
            printing.FormatResponse("1234", MessageType.Error);

            printing.LastMessageType.Should().Be(MessageType.Error);
        }

        [Test]
        public void FormatResponse_LastMessageTypeIsDifferentLastCharacterNotNewline_NewlineIsPrepended()
        {
            printing.LastMessageType = MessageType.Error;
            printing.LastCharacter = 'b';

            var result = printing.FormatResponse("1234", MessageType.Info);

            result.Item1.Should().Be("\n    1234");
        }

        public class PrintingTester : Printing
        {
            public MessageType LastMessageType
            {
                get => _lastMessage;
                set => _lastMessage = value;
            }

            public char LastCharacter
            {
                get => _lastChar;
                set => _lastChar = value;
            }
        }
    }
}
