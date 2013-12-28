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
        private object storeLock = new object();

        public IEnumerable<string> ListFiles()
        {
            return this.storage.Keys;
        }

        public string Read(string fileName)
        {
            lock (storeLock)
            {
                return this.storage[fileName];
            }
        }

        public string[] ReadLines(string fileName)
        {
            return this.Read(fileName).Split('\n');
        }

        public void Store(string fileName, string value)
        {
            lock (this.storeLock)
            {
                /*if (this.storage.ContainsKey(fileName))
                {
                    this.storage[fileName] += string.Format("\n{0}", value);
                }
                else
                {
                    this.storage[fileName] = value;
                }*/
                while (!this.storage.TryAdd(fileName, value)) 
                {
                    if (this.storage.ContainsKey(fileName))
                    {
                        throw new Exception("File already exists in storage!");
                    }
                }
            }
        }
    }
}
