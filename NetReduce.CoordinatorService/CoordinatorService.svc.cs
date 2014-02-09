namespace NetReduce.CoordinatorService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text.RegularExpressions;
    using System.Threading;

    using NetReduce.Core;
    using NetReduce.Core.Extensions;
    using NetReduce.Remote;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CoordinatorService" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CoordinatorService : ICoordinatorService
    {
        private IStorage storage;

        private Coordinator<RemoteWorker<ServiceClientWrapper>> coordinator;

        private UriProvider uriProvider;

        private Thread coordinatorThread;

        public CoordinatorService()
        {
            this.storage = new FileSystemStorage(@"c:\temp\netreduce\coordinator", true);
            this.coordinator = new Coordinator<RemoteWorker<ServiceClientWrapper>>(this.storage);
            this.uriProvider = new UriProvider();
            ServiceClientWrapper.UriProvider = this.uriProvider;
        }

        public static Uri EndpointUri { get; set; }

        public void AddWorker(Uri uri)
        {
            if (!this.uriProvider.Uris.Contains(uri))
            {
                Console.WriteLine("Worker {0} registered", uri);
                this.uriProvider.Uris.Add(uri);
            }
        }

        public Uri[] GetWorkers()
        {
            return this.uriProvider.Uris.ToArray();
        }

        public void RemoveWorker(Uri uri)
        {
            if (this.uriProvider.Uris.Contains(uri))
            {
                Console.WriteLine("Worker {0} unregistered", uri);
                this.uriProvider.Uris.Remove(uri);
            }
        }

        public bool RunJob(int numberOfMappers, int numberOfReducers, Uri mapCodeFile, Uri reduceCodeFile, Uri[] filesToProcess)
        {
            if (!this.uriProvider.Uris.Any())
            {
                return false;
            }
            
            if (this.coordinatorThread != null && this.coordinatorThread.IsAlive)
            {
                return false;
            }

            this.coordinatorThread = new Thread(() =>
            {
                this.coordinator.Start(numberOfMappers, numberOfReducers, mapCodeFile, reduceCodeFile, filesToProcess);
            });

            this.coordinatorThread.Start();

            return true;
        }

        public Uri AddToStorage(string fileName, string content)
        {
            this.storage.Store(fileName, content);
            if (EndpointUri == null && OperationContext.Current != null)
            {
                EndpointUri = OperationContext.Current.IncomingMessageHeaders.To;
            }

            var newUri = string.Format("{0}?fileName={1}", EndpointUri != null ? EndpointUri.ToString() : string.Empty, fileName);

            return new Uri(newUri);
        }

        public Uri[] ListStorageFiles()
        {
            return this.storage.ListFiles().ToArray();
        }

        public void RemoveFromStorage(Uri uri)
        {
            this.storage.Remove(uri);
        }

        public void CleanStorage()
        {
            this.storage.Clean();
        }

        public MapReduceResult GetResults()
        {
            var results = new MapReduceResult();
            var keysAndValues = new List<Tuple<string, string>>();

            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.ReduceOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString));
            var uris = this.storage.ListFiles();
            foreach (var uri in uris)
            {
                var fileName = this.storage.GetFileName(uri);
                if (regex.IsMatch(fileName))
                {
                    var key = regex.Match(fileName).Groups["Key"].Value;
                    var value = this.storage.Read(fileName);
                    keysAndValues.Add(new Tuple<string, string>(key, value));
                }
            }

            results.IsRunning = this.coordinator.IsRunning;
            results.KeysAndValues = keysAndValues.ToArray();

            return results;
        }
    }
}
