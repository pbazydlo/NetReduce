namespace NetReduce.Remote
{
    using System;
    using System.Threading;

    using NetReduce.Core;

    // TODO: this.Id should be initialized before calling remoteWorekerService.Init
    public class RemoteWorker<T> : IWorker where T : IRemoteWorkerService, new()
    {
        public static bool NonBlockingMapAndReduce { get; set; }
        
        public RemoteWorker()
        {
            this.nonBlockingMapAndReduce = NonBlockingMapAndReduce;

            // TODO: Uri will be probably required for creating remoteWorkerService
            // or we could move whole endpoint uri management to IRemoteWorkerService
            // (because RemoteWorker gets wrapper which uses actual wcf client - 
            // as wcf client generated code does not implement actual IRemoteWorkerService)
            this.remoteWorkerService = new T();
            this.remoteWorkerService.Init(this.Id);
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

            this.IssueRemoteMap(inputFileName, mapCodeFileName);
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

            this.IssueRemoteReduce(uri, reduceCodeFileName);
        }

        public void Join()
        {
            if (this.joinThread == null)
            {
                this.joinThread = new Thread(() =>
                {
                    var resultKeys = default(string[]);

                    // wait for remote join
                    do
                    {
                        try
                        {
                            resultKeys = this.remoteWorkerService.TryJoin(this.Id, callbackUri: null);
                        }
                        catch { }
                    } while (resultKeys == null);

                    this.joinThread = null;
                });

                this.joinThread.Start();
            }

            this.joinThread.Join();
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
    }
}
