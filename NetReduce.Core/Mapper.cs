namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class Mapper
    {
        private Func<string, string, IEnumerable<KeyValuePair<string, string>>> map;

        public string Key { get; private set; }
        public string Value { get; private set; }

        public Mapper(string filePath, Func<string, string, IEnumerable<KeyValuePair<string, string>>> map)
        {
            this.Key = filePath;
            this.Value = File.ReadAllText(filePath);
            this.map = map;
        }

        public IEnumerable<KeyValuePair<string, string>> PerformMap()
        {
            return this.map.Invoke(this.Key, this.Value);
        }
    }
}
