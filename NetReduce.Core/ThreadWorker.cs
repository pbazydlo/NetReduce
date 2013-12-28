namespace NetReduce.Core
{
    using System;
    using System.Threading;

    using NetReduce.Core.Exceptions;

    public class ThreadWorker : IWorker
    {
        private Thread workerThread;

        public ThreadWorker()
        {
            this.workerThread = new Thread(() => { });
        }

        public ThreadWorker(IStorage storage, int id)
            : this()
        {
            this.Storage = storage;
            this.Id = id;
        }

        public IStorage Storage { get; set; }
        public int Id { get; set; }

        public void Map(string inputFileName, string mapCodeFileName)
        {
            this.EnsureWorkerThreadIsFree();

            this.workerThread = new Thread(() =>
            {
                var mapProvider = Loader.Load<IMapProvider>(mapCodeFileName, this.Storage);
                var mapper = new Mapper(inputFileName, mapProvider.Map, this.Storage);
                var mapResult = mapper.PerformMap();
                foreach (var res in mapResult)
                {
                    this.Storage.Store(
                           string.Format(Properties.Settings.Default.MapOutputFileName, res.Key, this.Id, Guid.NewGuid()), res.Value);
                }
            });

            this.workerThread.Start();
        }

        public void Reduce(string key, string reduceCodeFileName)
        {
            this.EnsureWorkerThreadIsFree();

            this.workerThread = new Thread(() =>
            {
                var reduceProvider = Loader.Load<IReduceProvider>(reduceCodeFileName, this.Storage);
                var reducer = new Reducer(key, reduceProvider.Reduce, this.Storage);
                var reduceResult = reducer.PerformReduce();
                this.Storage.Store(
                       string.Format(Properties.Settings.Default.ReduceOutputFileName, key, this.Id, Guid.NewGuid()), reduceResult);
            });

            this.workerThread.Start();
        }

        public void Join()
        {
            this.workerThread.Join();
        }

        private void EnsureWorkerThreadIsFree()
        {
            if (this.workerThread.IsAlive)
            {
                throw new WorkerBusyException();
            }
        }
    }
}
