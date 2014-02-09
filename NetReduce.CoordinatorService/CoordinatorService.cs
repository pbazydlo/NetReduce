namespace NetReduce.CoordinatorService
{
    using System;
    using System.Linq;
    using System.ServiceModel;

    using NetReduce.Core;
    using NetReduce.Remote;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CoordinatorService" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CoordinatorService : ICoordinatorService
    {
        private IStorage storage;

        private Coordinator<RemoteWorker<>> coordinator;

        public CoordinatorService()
        {
            this.storage = new FileSystemStorage(@"c:\temp\netreduce\coordinator", true);
            this.coordinator = new Coordinator<RemoteWorker<>>(this.storage);
        }

        public static Uri EndpointUri { get; set; }

        public void AddWorker(Uri uri)
        {
            throw new NotImplementedException();
        }

        public Uri[] GetWorkers()
        {
            throw new NotImplementedException();
        }

        public void RemoveWorker(Uri uri)
        {
            throw new NotImplementedException();
        }

        public void RunJob(Uri[] filesToProcess, Uri mapCodeFile, Uri reduceCodeFile)
        {
            throw new NotImplementedException();
        }

        public Uri AddToStorage(string fileName, string content)
        {
            this.storage.Store(fileName, content);
            if (EndpointUri == null && OperationContext.Current != null)
            {
                EndpointUri = OperationContext.Current.IncomingMessageHeaders.To;
            }

            var newUri = string.Format("{0}?fileName={2}", EndpointUri != null ? EndpointUri.ToString() : string.Empty, fileName);

            return new Uri(newUri);
        }

        public Uri[] ListStorageFiles()
        {
            return this.storage.ListFiles().ToArray();
        }

        public void RemoveFromStorage(Uri uri)
        {
            throw new NotImplementedException();
        }

        public void CleanStorage()
        {
            this.storage.Clean();
        }

        public MapReduceResult GetResults()
        {
            throw new NotImplementedException();
        }
    }
}
