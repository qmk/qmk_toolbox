//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using QMK_Toolbox.Helpers;

namespace QMK_Toolbox
{
    public interface IFlashing
    {
        void ClearEeprom(string mcu);

        void Flash(string mcu, string file);

        string[] GetMcuList();

        void Reset(string mcu);
    }
}
