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
        private List<WorkerType> workers;

        public Coordinator(IStorage storage)
        {
            this.storage = storage;
        }

        public void Start(int maxMapperNo, int maxReducerNo, Uri mapFuncFileName, Uri reduceFuncFileName, IEnumerable<Uri> filesToProcess)
        {
            var noOfWorkers = this.InitWorkers(maxMapperNo, maxReducerNo);
            var keys = this.PerformMap(maxMapperNo, mapFuncFileName, filesToProcess, noOfWorkers);
            keys = keys.Distinct().ToList();
            var reducersAssignment = this.TransferIntermediateFiles(keys, noOfWorkers);
            this.PerformReduce(maxReducerNo, reduceFuncFileName, noOfWorkers, keys);
            this.CleanUp();
        }

        public IEnumerable<string> GetKeys()
        {
            // TODO: it is wrong -> we get keys from join
            return this.storage.GetKeys();
        }

        private int InitWorkers(int maxMapperNo, int maxReducerNo)
        {
            this.workers = new List<WorkerType>();
            var noOfWorkers = Math.Max(maxMapperNo, maxReducerNo);
            for (int i = 0; i < noOfWorkers; i++)
            {
                var worker = new WorkerType();
                worker.Storage = this.storage;
                worker.Id = i;
                this.workers.Add(worker);
            }
            return noOfWorkers;
        }

        private void CleanUp()
        {
            Loader.CleanAssemblyCache();
        }

        private void PerformReduce(int maxReducerNo, Uri reduceFuncFileName, int noOfWorkers, List<string> keys)
        {
            var index = 0;
            foreach (var key in keys)
            {
                var worker = this.workers[(index++) % maxReducerNo];
                worker.Reduce(key, reduceFuncFileName);
            }

            for (var i = 0; i < noOfWorkers; i++)
            {
                this.workers[i].Join();
            }
        }

        private List<string> PerformMap(int maxMapperNo, Uri mapFuncFileName, IEnumerable<Uri> filesToProcess, int noOfWorkers)
        {
            var keys = new List<string>();

            var index = 0;
            foreach (var file in filesToProcess)
            {
                var worker = this.workers[(index++) % maxMapperNo];
                worker.Map(file, mapFuncFileName);
            }

            for (var i = 0; i < noOfWorkers; i++)
            {
                keys.AddRange(this.workers[i].Join());
            }

            return keys;
        }

        private Dictionary<string, int> TransferIntermediateFiles(IEnumerable<string> keys, int noOfWorkers)
        {
            var assignmentsInts = new Dictionary<string, int>();
            var assignmentsUris = new Dictionary<string, Uri>();
            var i = 0;
            
            foreach (var key in keys)
            {
                var workerId = (i++) % noOfWorkers;
                var uri = new Uri(string.Format("{0}?workerId={1}", this.workers[workerId].EndpointUri, workerId));
                assignmentsInts.Add(key, workerId);
                assignmentsUris.Add(key, uri);
            }

            foreach (var worker in this.workers)
            {
                // TODO: maybe we want to return these uris?
                worker.TransferFiles(worker.Id, assignmentsUris);
            }

            return assignmentsInts;
        }
    }
}
