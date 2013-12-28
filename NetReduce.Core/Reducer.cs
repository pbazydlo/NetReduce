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
        private string sourceDirectory;

        public int LoadedFileCount { get; set; }
        public Regex FileFilter { get; private set; }

        public Reducer(string key, string sourceDirectory)
        {
            this.key = key;
            this.sourceDirectory = sourceDirectory;
            this.FileFilter = new Regex(string.Format("^{0}_MAP_NO_[0-9]+$", this.key));
            this.Load();
        }

        private void Load()
        {
            var filePaths = Directory.GetFiles(this.sourceDirectory);
            foreach (var filePath in filePaths)
            {
                if (this.FileFilter.IsMatch(Path.GetFileName(filePath)))
                {
                    this.LoadedFileCount++;
                }
            }
        }
    }
}
