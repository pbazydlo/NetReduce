using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetReduce.Core
{
    public interface IStorage
    {
        IEnumerable<string> ListFiles();
        string Read(string fileName);
        void Store(string fileName, string value);
    }
}
