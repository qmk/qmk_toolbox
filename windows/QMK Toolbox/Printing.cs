//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

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

    public class Printing {
        MessageType lastMessage;
        RichTextBox richTextBox;
        char lastChar = '\n';

        public Printing() {

        }

        public Printing(RichTextBox richTextBox) {
            this.richTextBox = richTextBox;
            this.richTextBox.Cursor = Cursors.Arrow; // mouse cursor like in other controls
        }

        private string prepend(string str, string indent, bool newline) {
            string output = "";
            if (newline)
                output = indent + str + "\n";
            else
                output = indent + str;
            return output;
        }

        public Tuple<string, Color> format(string str, MessageType type) {
            Color color = Color.White;
            switch (type) {
                case MessageType.Info:
                    color = Color.White;
                    str = prepend(str, "*** ", true);
                    break;
                case MessageType.Command:
                    color = Color.White;
                    str = prepend(str, ">>> ", true);
                    break;
                case MessageType.Bootloader:
                    color = Color.Yellow;
                    str = prepend(str, "*** ", true);
                    break;
                case MessageType.Error:
                    color = Color.Red;
                    str = prepend(str, "  ! ", true);
                    break;
                case MessageType.HID:
                    color = Color.LightSkyBlue;
                    str = prepend(str, "*** ", true);
                    break;
            }

            if (lastChar != '\n')
                str = "\n" + str;

            lastMessage = type;
            return Tuple.Create(str, color);
        }

        public Tuple<string, Color> formatResponse(string str, MessageType type) {
            bool addBackNewline = false;
            if (str.Last<char>() == '\n') {
                str = str.Substring(0, str.Length - 1);
                addBackNewline = true;
            }
            str = str.Replace("\n", "\n    ");
            if (addBackNewline)
                str = str + "\n";

            Color color = Color.White;
            switch (type) {
                case MessageType.Info:
                    color = Color.LightGray;
                    str = prepend(str, "    ", false);
                    break;
                case MessageType.Command:
                    color = Color.LightGray;
                    str = prepend(str, "    ", false);
                    break;
                case MessageType.Bootloader:
                    color = Color.Yellow;
                    str = prepend(str, "    ", false);
                    break;
                case MessageType.Error:
                    color = Color.DarkRed;
                    str = prepend(str, "    ", false);
                    break;
                case MessageType.HID:
                    color = Color.SkyBlue;
                    if (richTextBox.Text.Last<char>() == '\n')
                        str = prepend(str, "  > ", false);
                    break;
            }

            if (lastMessage != type && lastChar != '\n')
                str = "\n" + str;

            lastMessage = type;
            return Tuple.Create(str, color);
        }

        public void print(string str, MessageType type) {
            if (richTextBox == null || !richTextBox.InvokeRequired) {
                if (string.IsNullOrEmpty(str.Trim('\0')))
                    return;
                addToTextBox(format(str.Trim('\0'), type));
            } else {
                richTextBox.Invoke(new Action<string, MessageType>(print), new object[] { str, type });
            }
        }

        public void printResponse(string str, MessageType type) {
            if (richTextBox == null || !richTextBox.InvokeRequired) {
                if (string.IsNullOrEmpty(str.Trim('\0')))
                    return;
                addToTextBox(formatResponse(str.Trim('\0'), type));
            } else {
                richTextBox.Invoke(new Action<string, MessageType>(printResponse), new object[] { str, type });
            }
        }
            
        private void addToTextBox(Tuple<string, Color>items) {
                string str = items.Item1;
                Color color = items.Item2;
            if (richTextBox != null) {
                richTextBox.SelectionStart = richTextBox.TextLength;
                richTextBox.SelectionLength = str.Length;
                richTextBox.SelectionColor = color;
                richTextBox.SelectedText = str;
            } else {
                Console.Write(str);
            }
            lastChar = str[str.Length-1];
        }
    }
}
