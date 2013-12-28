using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetReduce.Core.Tests
{
    internal class InMemoryStorage : IStorage
    {
        private ConcurrentDictionary<string, string> storage = new ConcurrentDictionary<string, string>();

        public IEnumerable<string> ListFiles()
        {
            return this.storage.Keys;
        }

        public string Read(string fileName)
        {
            return this.storage[fileName];
        }

        public string[] ReadLines(string fileName)
        {
            return this.Read(fileName).Split('\n');
        }

        public void Store(string fileName, string value)
        {
            while (!this.storage.TryAdd(fileName, value))
            {
                // ?
                if (this.storage.ContainsKey(fileName))
                {
                    throw new Exception();
                }
            }
        }
    }
}
