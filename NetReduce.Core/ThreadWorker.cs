namespace NetReduce.Core
{
    using System;
    using System.Threading;

    using NetReduce.Core.Exceptions;

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
            this.EnsureWorkerThreadIsFree();

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
            this.EnsureWorkerThreadIsFree();

            this.workerThread = new Thread(() =>
            {
                var reduceProvider = Loader.Load<IReduceProvider>(reduceCodeFileName, this.storage);
                var reducer = new Reducer(key, reduceProvider.Reduce, this.storage);
                var reduceResult = reducer.PerformReduce();
                    this.storage.Store(
                           string.Format(Properties.Settings.Default.ReduceOutputFileName, key, this.id, Guid.NewGuid()), reduceResult);
            });

            this.workerThread.Start();
        }

        public void Join()
        {
            this.workerThread.Join();
        }

        private void EnsureWorkerThreadIsFree()
        {
            if (this.workerThread != null && this.workerThread.IsAlive)
            {
                throw new WorkerBusyException();
            }
        }
    }
}
