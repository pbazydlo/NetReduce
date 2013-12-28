namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;

    public class Mapper
    {
        private Func<string, string, IEnumerable<KeyValuePair<string, string>>> map;
        private IStorage storage;

        public string Key { get; private set; }
        public string Value { get; private set; }

        public Mapper(string filePath, Func<string, string, IEnumerable<KeyValuePair<string, string>>> map, IStorage storage)
        {
            this.Key = filePath;
            this.Value = storage.Read(filePath);
            this.map = map;
            this.storage = storage;
        }

        public IEnumerable<KeyValuePair<string, string>> PerformMap()
        {
            return this.map.Invoke(this.Key, this.Value);
        }
    }
}
