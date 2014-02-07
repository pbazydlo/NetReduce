using NetReduce.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetReduce.WorkerService.Tests
{
    // TODO: how to handle disposing of real service client?
    public class ServiceClientWrapper : IRemoteWorkerService
    {
        public ServiceClientWrapper()
        {
            this.client = new WSClient.RemoteWorkerServiceClient();
        }

        public void Init(int workerId)
        {
            client.Init(workerId);
        }

        public void Map(int workerId, string uri, string mapFuncUri)
        {
            client.Map(workerId, uri, mapFuncUri);
        }

        public void Reduce(int workerId, string uri, string reduceFuncUri)
        {
            client.Reduce(workerId, uri, reduceFuncUri);
        }

        public bool TryJoin(int workerId, string callbackUri)
        {
            return client.TryJoin(workerId, callbackUri);
        }

        private WSClient.RemoteWorkerServiceClient client;
    }
}
