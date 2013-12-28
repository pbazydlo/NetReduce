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
        private string sourceDirectory;
        private Func<string, IEnumerable<string>, string> reduce;
        
        public int LoadedFileCount { get; set; }
        public Regex FileFilter { get; private set; }

        public Reducer(string key, string sourceDirectory, Func<string, IEnumerable<string>, string> reduce)
        {
            this.key = key;
            this.sourceDirectory = sourceDirectory;
            this.reduce = reduce;
            this.FileFilter = new Regex(string.Format("^" + Core.Properties.Settings.Default.MapOutputFileName + "$", this.key, "[0-9]+"));
            this.Load();
        }

        public string PerformReduce()
        {
            return this.reduce.Invoke(key, values);
        }

        private void Load()
        {
            this.values = new List<string>();
            var filePaths = Directory.GetFiles(this.sourceDirectory);
            foreach (var filePath in filePaths)
            {
                if (this.FileFilter.IsMatch(Path.GetFileName(filePath)))
                {
                    (this.values as List<string>).AddRange(File.ReadAllLines(filePath));
                    this.LoadedFileCount++;
                }
            }
        }
    }
}
