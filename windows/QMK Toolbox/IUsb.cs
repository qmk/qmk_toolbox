//  Created by Mike Cooper on 2/20/20.
//  Copyright © 2020 Mike Cooper. This code is licensed under MIT license (see LICENSE.md for details).

using System;
using System.Collections.Generic;
using System.Management;
using QMK_Toolbox.Wmi;

namespace QMK_Toolbox
{
    public interface IUsb
    {
        bool AreDevicesAvailable();

        bool CanFlash(Chipset chipset);

        bool DetectBootloader(IManagementBaseObject instance, bool connected = true);

        bool DetectBootloaderFromCollection(IEnumerable<IManagementBaseObject> collection, bool connected = true);

        string GetComPort(IManagementBaseObject instance);
    }
}
