using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine_Performance_Management.Extension
{
    public class CoreException : Exception
    {
        public CoreException(string message) : base(message)
        {

        }
    }
}
