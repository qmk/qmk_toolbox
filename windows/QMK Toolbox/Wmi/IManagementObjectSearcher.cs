﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Wmi
{
    public interface IManagementObjectSearcher : IDisposable
    {
        IManagementObjectCollection Get();
    }
}
