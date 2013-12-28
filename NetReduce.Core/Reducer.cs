using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetReduce.Core
{
    public class Reducer
    {
        private string key;
        private IEnumerable<string> values;
        private Func<string, IEnumerable<string>, string> reduce;
        private IStorage storage;
        
        public int LoadedFileCount { get; set; }
        public Regex FileFilter { get; private set; }

        public Reducer(string key, Func<string, IEnumerable<string>, string> reduce, IStorage storage)
        {
            this.key = key;
            this.reduce = reduce;
            this.FileFilter = new Regex(string.Format("^" + Core.Properties.Settings.Default.MapOutputFileName + "$", this.key, "[0-9]+"));
            this.storage = storage;
            this.Load();
        }

        public string PerformReduce()
        {
            return this.reduce.Invoke(key, values);
        }

        private void Load()
        {
            this.values = new List<string>();
            var fileNames = this.storage.ListFiles();
            foreach (var fileName in fileNames)
            {
                if (this.FileFilter.IsMatch(fileName))
                {
                    (this.values as List<string>).AddRange(this.storage.ReadLines(fileName));
                    this.LoadedFileCount++;
                }
            }
        }
    }
}
