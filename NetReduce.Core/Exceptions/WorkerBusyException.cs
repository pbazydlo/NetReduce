using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetReduce.Core.Exceptions
{
    public class WorkerBusyException : Exception
    {
        public WorkerBusyException()
            : base("Worker is still working! Do Join!") { }
    }
}
