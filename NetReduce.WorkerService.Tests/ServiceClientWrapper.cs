using NetReduce.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetReduce.WorkerService.Tests
{
    using System.ServiceModel;

    // TODO: how to handle disposing of real service client?
    public class ServiceClientWrapper : IRemoteWorkerService
    {
        public static IUriProvider UriProvider { get; set; }

        public ServiceClientWrapper()
        {
            this.EndpointUri = UriProvider.GetNextUri();
            var binding = new BasicHttpBinding("BasicHttpBinding_IRemoteWorkerService");
            this.client = new WSClient.RemoteWorkerServiceClient(binding, new EndpointAddress(this.EndpointUri));
        }

        public Uri EndpointUri { get; private set; }

        public virtual void Init(int workerId)
        {
            this.client.Init(workerId);
        }

        public virtual void Map(Uri uri, Uri mapFuncUri)
        {
            this.client.Map(uri, mapFuncUri);
        }

        public virtual void Reduce(Uri uri, Uri reduceFuncUri)
        {
            this.client.Reduce(uri, reduceFuncUri);
        }

        public virtual string[] TryJoin(int workerId, Uri callbackUri)
        {
            return this.client.TryJoin(workerId, callbackUri);
        }

        public virtual Uri[] TransferFiles(int workerId, Dictionary<string, Uri> keysAndUris)
        {
            return this.client.TransferFiles(workerId, keysAndUris);
        }

        public virtual Uri PushFile(int workerId, string fileName, string content)
        {
            return this.client.PushFile(workerId, fileName, content);
        }

        private WSClient.RemoteWorkerServiceClient client;


        public Core.PerformanceMonitor.PerformanceStatistics GetPerformanceStatistics()
        {
            throw new NotImplementedException();
        }
    }
}
