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
            this.PerformReduce(maxReducerNo, reduceFuncFileName, noOfWorkers, keys);
            this.CleanUp();
        }

        public IEnumerable<string> GetKeys()
        {
            var result = new List<string>();
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.MapOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString));
            var uris = this.storage.ListFiles();
            foreach (var uri in uris)
            {
                var fileName = this.storage.GetFileName(uri);
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
            int index = 0;
            foreach (var file in filesToProcess)
            {
                var worker = this.workers[(index++) % maxMapperNo];
                worker.Map(file, mapFuncFileName);
            }

            for (var i = 0; i < noOfWorkers; i++)
            {
                this.workers[i].Join();
            }

            var keys = this.GetKeys().ToList();
            return keys;
        }
    }
}
