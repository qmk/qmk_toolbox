//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public enum MessageType
    {
        Bootloader,
        Hid,
        Command,
        Info,
        Error
    }

    public class Printing
    {
        private MessageType _lastMessage;
        private readonly RichTextBox _richTextBox;
        private char _lastChar = '\n';

        public Printing()
        {
        }

        public Printing(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
            _richTextBox.Cursor = Cursors.Arrow; // mouse cursor like in other controls
        }

        private static string Prepend(string str, string indent, bool newline) => indent + str + (newline ? "\n" : "");

        public Tuple<string, Color> Format(string str, MessageType type)
        {
            var color = Color.White;
            switch (type)
            {
                case MessageType.Info:
                    color = Color.White;
                    str = Prepend(str, "*** ", true);
                    break;

                case MessageType.Command:
                    color = Color.White;
                    str = Prepend(str, ">>> ", true);
                    break;

                case MessageType.Bootloader:
                    color = Color.Yellow;
                    str = Prepend(str, "*** ", true);
                    break;

                case MessageType.Error:
                    color = Color.Red;
                    str = Prepend(str, "  ! ", true);
                    break;

                case MessageType.Hid:
                    color = Color.LightSkyBlue;
                    str = Prepend(str, "*** ", true);
                    break;
            }

            if (_lastChar != '\n')
                str = "\n" + str;

            _lastMessage = type;
            return Tuple.Create(str, color);
        }

        public Tuple<string, Color> FormatResponse(string str, MessageType type)
        {
            var addBackNewline = false;
            if (str.Last() == '\n')
            {
                str = str.Substring(0, str.Length - 1);
                addBackNewline = true;
            }
            str = str.Replace("\n", "\n    ");
            if (addBackNewline)
                str = str + "\n";

            var color = Color.White;
            switch (type)
            {
                case MessageType.Info:
                    color = Color.LightGray;
                    str = Prepend(str, "    ", false);
                    break;

                case MessageType.Command:
                    color = Color.LightGray;
                    str = Prepend(str, "    ", false);
                    break;

                case MessageType.Bootloader:
                    color = Color.Yellow;
                    str = Prepend(str, "    ", false);
                    break;

                case MessageType.Error:
                    color = Color.DarkRed;
                    str = Prepend(str, "    ", false);
                    break;

                case MessageType.Hid:
                    color = Color.SkyBlue;
                    if (_richTextBox.Text.Last() == '\n')
                        str = Prepend(str, "  > ", false);
                    break;
            }

            if (_lastMessage != type && _lastChar != '\n')
                str = "\n" + str;

            _lastMessage = type;
            return Tuple.Create(str, color);
        }

        public void Print(string str, MessageType type)
        {
            if (_richTextBox == null || !_richTextBox.InvokeRequired)
            {
                if (string.IsNullOrEmpty(str.Trim('\0')))
                    return;
                AddToTextBox(Format(str.Trim('\0'), type));
            }
            else
            {
                _richTextBox.Invoke(new Action<string, MessageType>(Print), str, type);
            }
        }

        public void PrintResponse(string str, MessageType type)
        {
            if (_richTextBox == null || !_richTextBox.InvokeRequired)
            {
                if (string.IsNullOrEmpty(str.Trim('\0')))
                    return;
                AddToTextBox(FormatResponse(str.Trim('\0'), type));
            }
            else
            {
                _richTextBox.Invoke(new Action<string, MessageType>(PrintResponse), str, type);
            }
        }

        private void AddToTextBox(Tuple<string, Color> items)
        {
            var str = items.Item1;
            var color = items.Item2;
            if (_richTextBox != null)
            {
                _richTextBox.SelectionStart = _richTextBox.TextLength;
                _richTextBox.SelectionLength = str.Length;
                _richTextBox.SelectionColor = color;
                _richTextBox.SelectedText = str;
            }
            else
            {
                Console.Write(str);
            }
            _lastChar = str[str.Length - 1];
        }
    }
}