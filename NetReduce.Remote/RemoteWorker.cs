namespace NetReduce.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using NetReduce.Core;

    // TODO: this.Id should be initialized before calling remoteWorekerService.Init
    public class RemoteWorker<T> : IWorker where T : IRemoteWorkerService, new()
    {
        public static bool NonBlockingMapAndReduce { get; set; }

        public static Uri CoordinatorCallbackUri { get; set; }

        public RemoteWorker()
        {
            this.nonBlockingMapAndReduce = NonBlockingMapAndReduce;
        }

        public void Map(Uri inputFileName, Uri mapCodeFileName)
        {
            var remoteInputFileName = this.PushFile(inputFileName);
            var remoteMapCodeFileName = this.PushFile(mapCodeFileName);

            if (this.nonBlockingMapAndReduce)
            {
                new Thread(() =>
                {
                    this.IssueRemoteMap(remoteInputFileName, remoteMapCodeFileName);
                }).Start();
                return;
            }

            this.IssueRemoteMap(remoteInputFileName, remoteMapCodeFileName);
        }

        public void Reduce(string key, Uri reduceCodeFileName)
        {
            var remoteReduceCodeFileName = this.PushFile(reduceCodeFileName);

            // TODO: Transfer intermediate results



            var uri = new Uri(string.Format("{0}?workerId={1}&key={2}", this.remoteWorkerService.EndpointUri, this.Id, key));

            if (this.nonBlockingMapAndReduce)
            {
                new Thread(() =>
                    {
                        this.IssueRemoteReduce(uri, remoteReduceCodeFileName);
                    }).Start();
                return;
            }

            this.IssueRemoteReduce(uri, remoteReduceCodeFileName);
        }

        public IEnumerable<string> Join()
        {
            var resultKeys = default(string[]);

            if (this.joinThread == null)
            {
                this.joinThread = new Thread(() =>
                {
                    // wait for remote join
                    do
                    {
                        try
                        {
                            resultKeys = this.remoteWorkerService.TryJoin(this.Id, callbackUri: CoordinatorCallbackUri);
                        }
                        catch { }
                    } while (resultKeys == null);

                    this.joinThread = null;
                });

                this.joinThread.Start();
            }

            this.joinThread.Join();

            return resultKeys;
        }

        public IStorage Storage { get; set; }

        public int Id { get; set; }

        private IRemoteWorkerService remoteWorkerService;

        private bool nonBlockingMapAndReduce { get; set; }

        private Thread joinThread;

        private void IssueRemoteMap(Uri fileUri, Uri mapCodeFileUri)
        {
            this.remoteWorkerService.Map(fileUri, mapCodeFileUri);
        }

        private void IssueRemoteReduce(Uri fileUri, Uri reduceCodeFileUri)
        {
            this.remoteWorkerService.Reduce(fileUri, reduceCodeFileUri);
        }

        private Uri PushFile(Uri uri)
        {
            return this.remoteWorkerService.PushFile(this.Id, this.Storage.GetFileName(uri), this.Storage.Read(uri));
        }

        public Uri[] TransferFiles(int workerId, Dictionary<string, Uri> keysAndUris)
        {
            return this.remoteWorkerService.TransferFiles(workerId, keysAndUris);
        }

        public Uri EndpointUri
        {
            get { return this.remoteWorkerService.EndpointUri; }
        }

        public void Init()
        {
            this.remoteWorkerService = new T();
            this.remoteWorkerService.Init(this.Id);
        }
    }
}
