using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Wmi
{
    public class ManagementBaseObjectWrapper : IManagementBaseObject
    {
        private readonly ManagementBaseObject baseObject;

        public ManagementBaseObjectWrapper(ManagementBaseObject baseObject)
        {
            this.baseObject = baseObject;
        }

        public object GetPropertyValue(string propertyName) => baseObject.GetPropertyValue(propertyName);
    }
}
