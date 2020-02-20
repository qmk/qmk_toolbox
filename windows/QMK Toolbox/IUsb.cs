//  Created by Jack Humbert on 11/2/17.
//  Copyright © 2017 Jack Humbert. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Management;
using QMK_Toolbox.Wmi;

namespace QMK_Toolbox
{
    public interface IUsb
    {
        bool AreDevicesAvailable();

        bool CanFlash(Chipset chipset);

        bool DetectBootloader(IManagementBaseObject instance, bool connected = true);

        bool DetectBootloaderFromCollection(IManagementObjectCollection collection, bool connected = true);

        string GetComPort(IManagementBaseObject instance);
    }
}
