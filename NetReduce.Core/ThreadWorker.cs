using NetReduce.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetReduce.Core
{
    public class ThreadWorker : IWorker
    {
        private IStorage storage;
        private Thread workerThread;
        private int id;

        public ThreadWorker(IStorage storage, int id)
        {
            this.storage = storage;
            this.id = id;
        }

        public void Map(string inputFileName, string mapCodeFileName)
        {
            if (this.workerThread != null && this.workerThread.IsAlive)
                throw new WorkerBusyException();

            this.workerThread = new Thread(() =>
            {
                var mapProvider = Loader.Load<IMapProvider>(mapCodeFileName, this.storage);
                var mapper = new Mapper(inputFileName, mapProvider.Map, this.storage);
                var mapResult = mapper.PerformMap();
                foreach (var res in mapResult)
                {
                    this.storage.Store(
                           string.Format(Properties.Settings.Default.MapOutputFileName, res.Key, this.id, Guid.NewGuid()), res.Value);
                }
            });

            this.workerThread.Start();
        }

        public void Reduce(string key, string reduceCodeFileName)
        {
            throw new NotImplementedException();
        }

        public void Join()
        {
            this.workerThread.Join();
        }
    }
}
