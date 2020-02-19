//  Created by Jack Humbert on 9/1/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

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
