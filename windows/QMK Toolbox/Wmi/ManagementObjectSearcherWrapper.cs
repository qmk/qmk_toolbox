using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace QMK_Toolbox.Wmi
{
    public class ManagementObjectSearcherWrapper : IManagementObjectSearcher
    {
        private readonly ManagementObjectSearcher searcher;
        private bool disposedValue = false; // To detect redundant calls

        public ManagementObjectSearcherWrapper(ManagementObjectSearcher searcher)
        {
            this.searcher = searcher;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        public IManagementObjectCollection Get() => new ManagementObjectCollectionWrapper(searcher.Get());

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                searcher?.Dispose();

                disposedValue = true;
            }
        }
    }
}
