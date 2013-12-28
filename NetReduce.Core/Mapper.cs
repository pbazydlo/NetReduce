using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetReduce.Core
{
    public class Mapper
    {
        private Func<string, string, SortedList<string, string>> map;

        public string Key { get; private set; }
        public string Value { get; private set; }

        public Mapper(string filePath, Func<string, string, SortedList<string, string>> map)
        {
            this.Key = filePath;
            this.Value = File.ReadAllText(filePath);
            this.map = map;
        }

        public SortedList<string, string> PerformMap()
        {
            return this.map.Invoke(this.Key, this.Value);
        }
    }
}
