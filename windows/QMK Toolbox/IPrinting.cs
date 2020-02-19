//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public interface IPrinting
    {
        Tuple<string, Color> Format(string str, MessageType type);

        Tuple<string, Color> FormatResponse(string str, MessageType type);

        void Print(string str, MessageType type);

        void PrintResponse(string str, MessageType type);
    }
}
