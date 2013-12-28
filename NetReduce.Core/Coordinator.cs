using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetReduce.Core
{
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
    }
}
