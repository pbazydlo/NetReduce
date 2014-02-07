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

        public virtual void Map(int workerId, string uri, string mapFuncUri)
        {
            Mock.Map(workerId, uri, mapFuncUri);
        }

        public void Reduce(int workerId, string uri, string reduceFuncUri)
        {
            Mock.Reduce(workerId, uri, reduceFuncUri);
        }

        public bool TryJoin(int workerId, string callbackUri)
        {
            return Mock.TryJoin(workerId, callbackUri);
        }
    }
}
