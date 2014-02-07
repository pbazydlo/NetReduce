using NetReduce.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetReduce.Remote
{
    public class RemoteWorker<T> : IWorker where T : IRemoteWorkerService, new()
    {
        public static bool NonBlockingMapAndReduce { get; set; }

        public RemoteWorker()
        {
            this.nonBlockingMapAndReduce = NonBlockingMapAndReduce;
            this.remoteWorkerService = new T();
            this.remoteWorkerService.Init(this.Id);
        }

        public void Map(string inputFileName, string mapCodeFileName)
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

        public void Reduce(string key, string reduceCodeFileName)
        {
            if (this.nonBlockingMapAndReduce)
            {
                new Thread(() =>
                {
                    this.IssueRemoteReduce(key, reduceCodeFileName);
                }).Start();
                return;
            }

            this.IssueRemoteReduce(key, reduceCodeFileName);
        }

        public void Join()
        {
            if (this.joinThread == null)
            {
                this.joinThread = new Thread(() =>
                {
                    bool remoteJoinSucceded = false;

                    // wait for remote join
                    do
                    {
                        try
                        {
                            remoteJoinSucceded = this.remoteWorkerService.TryJoin(this.Id, callbackUri: null);
                        }
                        catch { }
                    } while (!remoteJoinSucceded);

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

        private void IssueRemoteMap(string uri, string mapCodeFileUri)
        {
            this.remoteWorkerService.Map(this.Id, uri, mapCodeFileUri);
        }

        private void IssueRemoteReduce(string uri, string reduceCodeFileUri)
        {
            this.remoteWorkerService.Reduce(this.Id, uri, reduceCodeFileUri);
        }
    }
}
