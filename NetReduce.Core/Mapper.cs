using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetReduce.Core
{
    public class Mapper
    {
        public Mapper(string filePath)
        {
            this.Source = File.ReadAllText(filePath);
        }

        public string Source { get; set; }
    }
}
