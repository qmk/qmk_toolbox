using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Wmi
{
    public class ManagementObjectCollectionWrapper : IManagementObjectCollection
    {
        private readonly List<IManagementBaseObject> baseObjects;

        public ManagementObjectCollectionWrapper(ManagementObjectCollection objectCollection)
        {
            foreach (var item in objectCollection)
            {
                baseObjects.Add(new ManagementBaseObjectWrapper(item));
            }
        }

        public IEnumerator<IManagementBaseObject> GetEnumerator() => baseObjects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => baseObjects.GetEnumerator();
    }
}
