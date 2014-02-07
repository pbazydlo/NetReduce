using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace NetReduce.Remote
{
    [ServiceContract]
    public interface IRemoteWorkerService
    {
        [OperationContract]
        void Init(int workerId);

        [OperationContract]
        void Map(int workerId, string uri, string mapFuncUri);

        [OperationContract]
        void Reduce(int workerId, string uri, string reduceFuncUri);

        [OperationContract]
        bool TryJoin(int workerId, string callbackUri);
    }
}
