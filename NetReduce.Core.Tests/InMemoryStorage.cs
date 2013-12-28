using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetReduce.Core.Tests
{
    internal class InMemoryStorage : IStorage
    {
        private Dictionary<string, string> storage = new Dictionary<string, string>();

        public IEnumerable<string> ListFiles()
        {
            return this.storage.Keys;
        }

        public string Read(string fileName)
        {
            return this.storage[fileName];
        }

        public void Store(string fileName, string value)
        {
            this.storage.Add(fileName, value);
        }
    }
}
