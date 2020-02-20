//  Created by Mike Cooper on 2/20/20.
//  Copyright © 2020 Mike Cooper. This code is licensed under MIT license (see LICENSE.md for details).

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
            baseObjects = new List<IManagementBaseObject>();

            foreach (var item in objectCollection)
            {
                baseObjects.Add(new ManagementBaseObjectWrapper(item));
            }
        }

        public ManagementObjectCollectionWrapper(IEnumerable<IManagementBaseObject> objects)
        {
            baseObjects = new List<IManagementBaseObject>(objects);
        }

        public IEnumerator<IManagementBaseObject> GetEnumerator() => baseObjects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => baseObjects.GetEnumerator();
    }
}
