using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    class LogTextBox : RichTextBox
    {
        public void LogBootloader(string message)
        {
            Log(message, MessageType.Bootloader);
        }

        public void LogCommand(string message)
        {
            Log(message, MessageType.Command);
        }

        public void LogCommandError(string message)
        {
            Log(message, MessageType.CommandError);
        }

        public void LogCommandOutput(string message)
        {
            Log(message, MessageType.CommandOutput);
        }

        public void LogError(string message)
        {
            Log(message, MessageType.Error);
        }

        public void LogHid(string message)
        {
            Log(message, MessageType.Hid);
        }

        public void LogHidOutput(string message)
        {
            Log(message, MessageType.HidOutput);
        }

        public void LogInfo(string message)
        {
            Log(message, MessageType.Info);
        }

        public void LogUsb(string message)
        {
            Log(message, MessageType.Usb);
        }

        public void Log(string message, MessageType type)
        {
            if (message.Length > 1 && message.Last() == '\n')
            {
                message = message.Remove(message.Length - 1);
            }
            string[] lines = message.Split('\n');

            foreach (string line in lines)
            {
                switch (type)
                {
                    case MessageType.Bootloader:
                        AppendText($"{line}\n", Color.Yellow);
                        break;
                    case MessageType.Command:
                        AppendText($"> {line}\n", Color.White);
                        break;
                    case MessageType.CommandError:
                        AppendText("> ", Color.LightCoral);
                        AppendText($"{line}\n", Color.Silver);
                        break;
                    case MessageType.CommandOutput:
                        AppendText("> ", Color.White);
                        AppendText($"{line}\n", Color.Silver);
                        break;
                    case MessageType.Error:
                        AppendText($"{line}\n", Color.LightCoral);
                        break;
                    case MessageType.Hid:
                        AppendText($"{line}\n", Color.SkyBlue);
                        break;
                    case MessageType.HidOutput:
                        AppendText("> ", Color.SkyBlue);
                        AppendText($"{line}\n", Color.Azure);
                        break;
                    case MessageType.Info:
                        AppendText("* ", Color.White);
                        AppendText($"{line}\n", Color.Silver);
                        break;
                    case MessageType.Usb:
                        AppendText($"{line}\n", Color.White);
                        break;
                }
            }
        }

        public void AppendText(string text, Color color)
        {
            SelectionStart = TextLength;
            SelectionLength = text.Length;
            SelectionColor = color;
            SelectedText = text;
        }
    }
}
