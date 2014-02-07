using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using NetReduce.Remote;
using NetReduce.Core;

namespace NetReduce.WorkerService
{
    public class RemoteWorkerService : IRemoteWorkerService
    {
        private static ConcurrentDictionary<int, IWorker> Workers = new ConcurrentDictionary<int, IWorker>();

        private static IWorker GetWorker(int workerId)
        {
            IWorker worker;
            if (!Workers.TryGetValue(workerId, out worker))
            {
                throw new ArgumentNullException();
            }
            return worker;
        }

        public void Init(int workerId)
        {
            Workers.GetOrAdd(workerId,
                new ThreadWorker(storage: new FileSystemStorage(@"c:\temp\netreduce", false),
                                 id:workerId));
        }

        public void Map(int workerId, string uri, string mapFuncUri)
        {
            var worker = GetWorker(workerId);
            worker.Map(uri, mapFuncUri);
        }

        public void Reduce(int workerId, string uri, string reduceFuncUri)
        {
            var worker = GetWorker(workerId);
            worker.Reduce(uri, reduceFuncUri);
        }

        public bool TryJoin(int workerId, string callbackUri)
        {
            // TODO: ignored callbackUri -> wanted to use so that worker would inform coordinator 
            // about finishing the job
            var worker = GetWorker(workerId);
            worker.Join();

            return true;
        }
    }
}
