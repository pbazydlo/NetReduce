using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetReduce.Core
{
    using System.Collections;
    using System.IO;
    using System.Text.RegularExpressions;

    public class Coordinator
    {
        private Action _map;
        private Action _reduce;

        public Coordinator(Action map, Action reduce)
        {
            _map = map;
            _reduce = reduce;
        }

        public void Start(int maxMapperNo, int maxReducerNo)
        {
            List<Thread> threadsRunning = new List<Thread>();
            for (int i = 0; i < maxMapperNo; i++)
            {
                var mapper = new Thread(() => this._map.Invoke());
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
                var reducer = new Thread(() => { _reduce.Invoke(); });
                reducer.Start();
                threadsRunning.Add(reducer);
            }

            foreach (var reducer in threadsRunning)
            {
                reducer.Join();
            }
        }

        public IEnumerable<string> GetKeys(string testDirectory)
        {
            var result = new List<string>();
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.MapOutputFileName + "$", @"(?<Key>.+)", "[0-9]+")); //[^<>:""\\/|\?\*]
            var filePaths = Directory.GetFiles(testDirectory);
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
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
