//  Created by Mike Cooper on 2/20/20.
//  Copyright © 2020 Mike Cooper. This code is licensed under MIT license (see LICENSE.md for details).

using System;

namespace QMK_Toolbox
{
    public interface IFlashing
    {
        string CaterinaPort { get; set; }

        IUsb Usb { get; set; }

        void ClearEeprom(string mcu);

        void Flash(string mcu, string file);

        string[] GetMcuList();

        void Reset(string mcu);
    }
}
