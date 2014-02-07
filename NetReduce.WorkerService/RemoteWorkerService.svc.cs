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
    using System.Web;

    public class RemoteWorkerService : IRemoteWorkerService
    {
        private static ConcurrentDictionary<int, IWorker> Workers = new ConcurrentDictionary<int, IWorker>();

        private static IWorker GetWorker(Uri uri)
        {
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            var workerId = int.Parse(parameters["workerId"]);
            return GetWorker(workerId);
        }

        private static IWorker GetWorker(int workerId)
        {
            IWorker worker;
            if (!Workers.TryGetValue(workerId, out worker))
            {
                throw new ArgumentNullException();
            }
            return worker;
        }

        private static string GetKey(Uri uri)
        {
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            var key = parameters["key"];
            return key;
        }

        public void Init(int workerId)
        {
            Workers.GetOrAdd(workerId,
                new ThreadWorker(storage: new FileSystemStorage(@"c:\temp\netreduce", false),
                                 id:workerId));
        }

        public void Map(Uri uri, Uri mapFuncUri)
        {
            var worker = GetWorker(uri);
            worker.Map(uri, mapFuncUri);
        }

        public void Reduce(Uri uri, Uri reduceFuncUri)
        {
            var worker = GetWorker(uri);
            worker.Reduce(GetKey(uri), reduceFuncUri);
        }

        public string[] TryJoin(int workerId, Uri callbackUri)
        {
            // TODO: ignored callbackUri -> wanted to use so that worker would inform coordinator 
            // about finishing the job
            var worker = GetWorker(workerId);
            worker.Join();

            return worker.Storage.GetKeys().ToArray();
        }

        public Uri[] TransferFiles(int workerId, Dictionary<string, Uri> keysAndUris)
        {
            throw new NotImplementedException();
        }

        public Uri PushFile(int workerId, string fileName, string content)
        {
            throw new NotImplementedException();
        }
    }
}
