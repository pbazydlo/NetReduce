namespace NetReduce.Core.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

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
                while (!this.storage.TryAdd(fileName, value)) 
                {
                    if (this.storage.ContainsKey(fileName))
                    {
                        throw new Exception("File already exists in storage!");
                    }
                }
            }
        }

        public void Clean()
        {
            this.storage.Clear();
        }
    }
}
