namespace NetReduce.Core.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    internal class InMemoryStorage : IStorage
    {
        private ConcurrentDictionary<string, string> storage = new ConcurrentDictionary<string, string>();
        private object storeLock = new object();

        public IEnumerable<Uri> ListFiles()
        {
            return this.storage.Keys.Select(k => new Uri(string.Format("memory:///{0}", k)));
        }

        public string Read(string fileName)
        {
            lock (storeLock)
            {
                return this.storage[fileName];
            }
        }

        public string Read(Uri uri)
        {
            return this.Read(this.GetFileName(uri));
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

        public void Store(Uri uri, string value)
        {
            this.Store(this.GetFileName(uri), value);
        }

        public void Clean()
        {
            this.storage.Clear();
        }

        public string GetFileName(Uri uri)
        {
            return uri.Segments.Last();
        }
    }
}
