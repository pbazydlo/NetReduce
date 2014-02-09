namespace NetReduce.Core.Tests
{
    using NetReduce.Core.Extensions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

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

        public IEnumerable<string> GetKeys()
        {
            var result = new List<string>();
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.MapOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString));
            var uris = this.ListFiles();
            foreach (var uri in uris)
            {
                var fileName = this.GetFileName(uri);
                if (regex.IsMatch(fileName))
                {
                    var key = regex.Match(fileName).Groups["Key"].Value;
                    if (!result.Contains(key))
                    {
                        result.Add(key);
                    }
                }
            }

            return result;
        }


        public void Remove(Uri uri)
        {
            var fileName = this.GetFileName(uri);
            string tmp;
            while(!this.storage.TryRemove(fileName, out tmp))
            {

            }
        }
    }
}
