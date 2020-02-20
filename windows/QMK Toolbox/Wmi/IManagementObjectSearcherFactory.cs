using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Wmi
{
    public interface IManagementObjectSearcherFactory
    {
        IManagementObjectSearcher Create(string queryString);
    }
}
