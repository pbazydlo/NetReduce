using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetReduce.Core
{
    using NetReduce.Core.Extensions;
    using System.Collections;
    using System.IO;
    using System.Text.RegularExpressions;

    public class Coordinator
    {
        private Action map;
        private Action reduce;
        private IStorage storage;

        public Coordinator(Action map, Action reduce, IStorage storage)
        {
            this.map = map;
            this.reduce = reduce;
            this.storage = storage;
        }

        public void Start(int maxMapperNo, int maxReducerNo)
        {
            List<Thread> threadsRunning = new List<Thread>();
            for (int i = 0; i < maxMapperNo; i++)
            {
                var mapper = new Thread(() => this.map.Invoke());
                mapper.Start();
                threadsRunning.Add(mapper);
            }

            foreach (var mapper in threadsRunning)
            {
                mapper.Join();
            }

            threadsRunning.Clear();
            for (int i = 0; i < maxReducerNo; i++)
            {
                var reducer = new Thread(() => { reduce.Invoke(); });
                reducer.Start();
                threadsRunning.Add(reducer);
            }

            foreach (var reducer in threadsRunning)
            {
                reducer.Join();
            }
        }

        public IEnumerable<string> GetKeys()
        {
            var result = new List<string>();
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.MapOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString)); //[^<>:""\\/|\?\*]
            var fileNames = this.storage.ListFiles();
            foreach (var fileName in fileNames)
            {
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
    }
}
