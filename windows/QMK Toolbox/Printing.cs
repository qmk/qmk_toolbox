using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox {
    public enum MessageType {
        Bootloader,
        HID,
        Command,
        Info,
        Error
    }

    public static class Printing {
        static MessageType lastMessage;

        public static void color(RichTextBox rtb, string str, MessageType type) {
            Color color = Color.White;
            switch (type) {
                case MessageType.Bootloader:
                    color = Color.Yellow;
                    break;
                case MessageType.Command:
                case MessageType.Info:
                    color = Color.White;
                    break;
                case MessageType.HID:
                    color = Color.LightSkyBlue;
                    break;
                case MessageType.Error:
                    color = Color.Red;
                    break;
            }
            lastMessage = type;
            Print(rtb, str, color);
        }

        public static void colorResponse(RichTextBox rtb, string str, MessageType type) {
            Color color = Color.White;
            switch (type) {
                case MessageType.Bootloader:
                    color = Color.Yellow;
                    break;
                case MessageType.Command:
                case MessageType.Info:
                    color = Color.LightGray;
                    break;
                case MessageType.HID:
                    color = Color.SkyBlue;
                    break;
                case MessageType.Error:
                    color = Color.DarkRed;
                    break;
            }
            lastMessage = type;
            Print(rtb, str, color);
        }

        public static void Print(RichTextBox rtb, string str, Color color) {
            Print(rtb, str, false, color);
        }

        public static void Print(RichTextBox rtb, string str, bool bold = false, Color? color = null, HorizontalAlignment align = HorizontalAlignment.Left) {
            if (string.IsNullOrEmpty(str))
                return;
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = str.Length;
            if (bold)
                rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
            rtb.SelectionColor = color ?? Color.White;
            rtb.SelectionAlignment = align;
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = str.Length;
            // This might be a better idea that prefixing each line
            // richTextBox1.SelectionIndent = 20;
            rtb.SelectedText = str;
        }

        public static string formatType(string output, string prefix, bool newline = true) {
            output = prefix + output;
            if (newline)
                output += "\n";
            return output;
        }

        public static string format(RichTextBox rtb, string str, MessageType type) {
            if (rtb.Text.Length > 0 && rtb.Text[rtb.Text.Length - 1] != '\n')
                Print(rtb, "\n");
            switch (type) {
                case MessageType.Bootloader:
                case MessageType.HID:
                case MessageType.Info:
                    str = formatType(str, "*** ");
                    break;
                case MessageType.Command:
                    str = formatType(str, ">>> ");
                    break;
                case MessageType.Error:
                    str = formatType(str, "  ! ");
                    break;
            }
            return str;
        } 

        public static string formatResponse(RichTextBox rtb, string output, MessageType type) {
            output = output.Trim('\0');
            if (output.Equals("\n") || output.Equals("\r") || String.IsNullOrEmpty(output))
                return output;
            bool endsWithNewline = (output[output.Length - 1] == '\n' || output[output.Length - 1] == '\r');
            string indent = new String(' ', 4);
            switch (type) {
                case MessageType.Bootloader:
                case MessageType.HID:
                    indent = "  > ";
                    break;
                case MessageType.Info:
                case MessageType.Command:
                    break;
            }
            if (rtb.Text[rtb.Text.Length - 1] == '\n' || rtb.Text[rtb.Text.Length - 1] == '\r')
                output = indent + output;
            output = output.Replace("\n", "\n" + indent);
            if (endsWithNewline)
                output = output.Substring(0, output.Length - indent.Length);
            return output;
        }
    }
}
