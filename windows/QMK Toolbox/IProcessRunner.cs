using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    /// <summary>
    /// Wrapper for a Process
    /// </summary>
    public interface IProcessRunner
    {
        void Run(string command, string args);
    }
}
