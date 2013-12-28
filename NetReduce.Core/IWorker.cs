using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetReduce.Core
{
    public interface IWorker
    {
        void BeMapper(string inputFileName);
        void BeReducer(string key);
        void Join();
    }
}
