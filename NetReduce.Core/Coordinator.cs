namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using NetReduce.Core.Extensions;

    public class Coordinator<WorkerType> where WorkerType : IWorker, new()
    {
        private IStorage storage;
        private List<WorkerType> mapWorkers = new List<WorkerType>();
        private List<WorkerType> reduceWorkers = new List<WorkerType>();

        private List<string> keys;

        public Coordinator(IStorage storage)
        {
            this.storage = storage;
        }

        public void Start(int maxMapperNo, int maxReducerNo, Uri mapFuncFileName, Uri reduceFuncFileName, IEnumerable<Uri> filesToProcess)
        {
            this.mapWorkers = this.InitWorkers(1, maxMapperNo);
            this.keys = this.PerformMap(mapFuncFileName, filesToProcess);
            this.keys = this.keys.Distinct().ToList();
            this.reduceWorkers = this.InitWorkers(maxMapperNo + 1, maxReducerNo);
            var reducersAssignment = this.TransferIntermediateFiles(this.keys);
            this.PerformReduce(reduceFuncFileName, reducersAssignment);
            this.CleanUp();
        }

        public IEnumerable<string> GetKeys()
        {
            return this.keys;
        }

        private List<WorkerType> InitWorkers(int startWorkerNo, int workersCount)
        {
            var workers = new List<WorkerType>();
            for (int i = 0; i < workersCount; i++)
            {
                var worker = new WorkerType();
                worker.Storage = this.storage;
                worker.Id = startWorkerNo + i;
                worker.Init();
                workers.Add(worker);
            }

            return workers;
        }

        private void CleanUp()
        {
            Loader.CleanAssemblyCache();
        }

        private void PerformReduce(Uri reduceFuncFileName, Dictionary<string, int> assignments)
        {
            var index = 0;
            foreach (var assignment in assignments)
            {
                var worker = this.reduceWorkers[assignment.Value];
                worker.Reduce(assignment.Key, reduceFuncFileName);
            }

            for (var i = 0; i < this.reduceWorkers.Count; i++)
            {
                this.reduceWorkers[i].Join();
            }
        }

        private List<string> PerformMap(Uri mapFuncFileName, IEnumerable<Uri> filesToProcess)
        {
            var keys = new List<string>();

            var index = 0;
            foreach (var file in filesToProcess)
            {
                var worker = this.mapWorkers[(index++) % this.mapWorkers.Count];
                worker.Map(file, mapFuncFileName);
            }

            for (var i = 0; i < this.mapWorkers.Count; i++)
            {
                keys.AddRange(this.mapWorkers[i].Join());
            }

            return keys;
        }

        private Dictionary<string, int> TransferIntermediateFiles(IEnumerable<string> keys)
        {
            var assignmentsInts = new Dictionary<string, int>();
            var assignmentsUris = new Dictionary<string, Uri>();
            var i = 0;

            foreach (var key in keys)
            {
                var index = (i++) % this.reduceWorkers.Count;
                var workerId = this.reduceWorkers[index].Id;
                var uri = new Uri(string.Format("{0}?workerId={1}", this.reduceWorkers[index].EndpointUri, workerId));
                assignmentsInts.Add(key, index);
                assignmentsUris.Add(key, uri);
            }

            foreach (var worker in this.mapWorkers)
            {
                // TODO: maybe we want to return these uris?
                worker.TransferFiles(worker.Id, assignmentsUris);
            }

            return assignmentsInts;
        }
    }
}
