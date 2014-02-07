using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetReduce.Remote.Tests
{
    internal class MockerForRemoteWorkerService : IRemoteWorkerService
    {
        public static IRemoteWorkerService Mock;

        public virtual void Init(int workerId)
        {
            Mock.Init(workerId);
        }

        public virtual void Map(Uri uri, Uri mapFuncUri)
        {
            Mock.Map(uri, mapFuncUri);
        }

        public virtual void Reduce(Uri uri, Uri reduceFuncUri)
        {
            Mock.Reduce(uri, reduceFuncUri);
        }

        public virtual string[] TryJoin(int workerId, Uri callbackUri)
        {
            return Mock.TryJoin(workerId, callbackUri);
        }

        public virtual Uri[] TransferFiles(int workerId, Dictionary<string, Uri> keysAndUris)
        {
            return Mock.TransferFiles(workerId, keysAndUris);
        }

        public virtual Uri PushFile(int workerId, string fileName, string content)
        {
            return Mock.PushFile(workerId, fileName, content);
        }

        public Uri EndpointUri { get; private set; }
    }
}
