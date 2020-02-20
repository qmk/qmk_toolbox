//  Created by Mike Cooper on 2/20/20.
//  Copyright © 2020 Mike Cooper. This code is licensed under MIT license (see LICENSE.md for details).

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
