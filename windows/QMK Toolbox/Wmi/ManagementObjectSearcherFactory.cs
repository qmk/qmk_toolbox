using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Wmi
{
    public class ManagementObjectSearcherFactory : IManagementObjectSearcherFactory
    {
        public IManagementObjectSearcher Create(string queryString) =>
            new ManagementObjectSearcherWrapper(new ManagementObjectSearcher(queryString));
    }
}
