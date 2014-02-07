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
            this.remoteWorkerService = new T();
            this.remoteWorkerService.Init(this.Id);
        }

        public void Map(Uri inputFileName, Uri mapCodeFileName)
        {
            if (this.nonBlockingMapAndReduce)
            {
                new Thread(() =>
                {
                    this.IssueRemoteMap(inputFileName, mapCodeFileName);
                }).Start();
                return;
            }

            this.IssueRemoteMap(inputFileName, mapCodeFileName);
        }

        public void Reduce(string key, Uri reduceCodeFileName)
        {
            var uri = new Uri(string.Format("{0}?workerId={1}&key={2}", this.EndpointUri, this.Id, key));

            if (this.nonBlockingMapAndReduce)
            {
                new Thread(() =>
                    {
                        this.IssueRemoteReduce(uri, reduceCodeFileName);
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

        public Uri EndpointUri { get; private set; }

        private void IssueRemoteMap(Uri fileUri, Uri mapCodeFileUri)
        {
            this.remoteWorkerService.Map(fileUri, mapCodeFileUri);
        }

        private void IssueRemoteReduce(Uri fileUri, Uri reduceCodeFileUri)
        {
            this.remoteWorkerService.Reduce(fileUri, reduceCodeFileUri);
        }
    }
}
