namespace NetReduce.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using NetReduce.Core.Exceptions;

    public class ThreadWorker : IWorker
    {
        private Thread workerThread;
        private ConcurrentQueue<Tuple<string, string>> taskQueue;

        public ThreadWorker()
        {
            this.workerThread = new Thread(() => { });
            this.taskQueue = new ConcurrentQueue<Tuple<string, string>>();
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
            this.taskQueue.Enqueue(new Tuple<string, string>(inputFileName, mapCodeFileName));

            if (this.workerThread.IsAlive)
            {
                return;
            }

            this.workerThread = new Thread(() =>
            {
                while (this.taskQueue.Count > 0)
                {
                    Tuple<string, string> task;
                    if (!this.taskQueue.TryDequeue(out task))
                    {
                        continue;
                    }

                    var mapProvider = Loader.Load<IMapProvider>(task.Item2, this.Storage);
                    var mapper = new Mapper(task.Item1, mapProvider.Map, this.Storage);
                    var mapResult = mapper.PerformMap();
                    foreach (var res in mapResult)
                    {
                        this.Storage.Store(
                            string.Format(Properties.Settings.Default.MapOutputFileName, res.Key, this.Id, Guid.NewGuid()),
                            res.Value);
                    }
                }
            });

            this.workerThread.Start();
        }

        public void Reduce(string key, string reduceCodeFileName)
        {
            this.taskQueue.Enqueue(new Tuple<string, string>(key, reduceCodeFileName));

            if (this.workerThread.IsAlive) 
            {
                return;
            }

            this.workerThread = new Thread(() =>
            {
                while (this.taskQueue.Count > 0)
                {
                    Tuple<string, string> task;
                    if (!this.taskQueue.TryDequeue(out task))
                    {
                        continue;
                    }

                    var reduceProvider = Loader.Load<IReduceProvider>(task.Item2, this.Storage);
                    var reducer = new Reducer(task.Item1, reduceProvider.Reduce, this.Storage);
                    var reduceResult = reducer.PerformReduce();
                    this.Storage.Store(
                        string.Format(Properties.Settings.Default.ReduceOutputFileName, task.Item1, this.Id, Guid.NewGuid()),
                        reduceResult);
                }
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
