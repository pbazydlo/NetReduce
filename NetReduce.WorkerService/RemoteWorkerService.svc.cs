namespace NetReduce.WorkerService
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using NetReduce.Core;
    using NetReduce.Core.Extensions;
    using NetReduce.Remote;

    public class RemoteWorkerService : IRemoteWorkerService
    {
        private static ConcurrentDictionary<int, IWorker> Workers = new ConcurrentDictionary<int, IWorker>();

        private static Uri GetWorkerEndpointUri(Uri uri)
        {
            return new Uri(uri.OriginalString.Replace(uri.Query, string.Empty));
        }

        private static IWorker GetWorker(Uri uri)
        {
            var workerId = GetWorkerId(uri);
            return GetWorker(workerId);
        }

        private static int GetWorkerId(Uri uri)
        {
            var parameters = HttpUtility.ParseQueryString(uri.Query);
            var workerId = int.Parse(parameters["workerId"]);
            return workerId;
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
            return key.Replace(' ', '+'); // replace spaces back with '+'
        }

        public void Init(int workerId)
        {
            Console.Write("Initializing worker {0}... ", workerId);
            try
            {
                Workers.GetOrAdd(
                    workerId,
                    new ThreadWorker(storage: new FileSystemStorage(string.Format(@"c:\temp\netreduce\{0}", workerId), true), id: workerId));
                Console.WriteLine("done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Map(Uri uri, Uri mapFuncUri)
        {
            Console.WriteLine("Got map");
            var worker = GetWorker(uri);
            worker.Map(uri, mapFuncUri);
        }

        public void Reduce(Uri uri, Uri reduceFuncUri)
        {
            Console.WriteLine("Reduce map");
            var worker = GetWorker(uri);
            worker.Reduce(GetKey(uri), reduceFuncUri);
        }

        public string[] TryJoin(int workerId, Uri callbackUri)
        {
            Console.WriteLine("Got join for {0}", workerId);
            // TODO: ignored callbackUri -> wanted to use so that worker would inform coordinator 
            // about finishing the job
            var worker = GetWorker(workerId);
            worker.Join();

            RemoteWorkerService.PushReduceResultsToCoordinatorInSeparateThread(workerId, callbackUri);

            return worker.Storage.GetKeys().ToArray();
        }

        public Uri[] TransferFiles(int workerId, Dictionary<string, Uri> keysAndUris)
        {
            var worker = GetWorker(workerId);
            var workerStorage = worker.Storage;
            var result = new List<Uri>();
            var pushFileTasks = PushFilesToReducers(keysAndUris, workerStorage);
            foreach (var pushFileTask in pushFileTasks)
            {
                var uri = pushFileTask.Result;
                result.Add(uri);
            }

            return result.ToArray();
        }

        public Uri PushFile(int workerId, string fileName, string content)
        {
            Console.Write("PushFile {0}... ", fileName);

            var worker = GetWorker(workerId);
            worker.Storage.Store(fileName, content);

            if (OperationContext.Current != null)
            {
                this.EndpointUri = OperationContext.Current.IncomingMessageHeaders.To;
            }
            else
            {
                Console.WriteLine("OperationContext.Current is null");
            }

            var newUri = string.Format("{0}?workerId={1}&fileName={2}", this.EndpointUri != null ? this.EndpointUri.ToString() : string.Empty, workerId, fileName);
            Console.WriteLine("stored under {0}", newUri);

            return new Uri(newUri);
        }

        public PerformanceMonitor.PerformanceStatistics GetPerformanceStatistics()
        {
            var result = new PerformanceMonitor.PerformanceStatistics();

            result.LoadStatistics = PerformanceMonitor.GetLoadStatistics();
            result.DriveStatistics = PerformanceMonitor.GetHddStatistics("c").ToArray();

            return result;
        }

        public Uri EndpointUri { get; private set; }

        private static List<Task<Uri>> PushFilesToReducers(Dictionary<string, Uri> keysAndUris, IStorage workerStorage)
        {
            var pushFileTasks = new List<Task<Uri>>();
            var regex = new Regex(string.Format("^" + Core.Properties.Settings.Default.MapOutputFileName + "$", @"(?<Key>.+)", "[0-9]+", RegexExtensions.GuidRegexString));
            var uris = workerStorage.ListFiles();
            foreach (var uri in uris)
            {
                var fileName = workerStorage.GetFileName(uri);
                if (regex.IsMatch(fileName))
                {
                    var key = regex.Match(fileName).Groups["Key"].Value;
                    if (keysAndUris.ContainsKey(key))
                    {
                        pushFileTasks.Add(PushFileToReducer(keysAndUris, workerStorage, fileName, key));
                    }
                }
            }

            return pushFileTasks;
        }

        private static Task<Uri> PushFileToReducer(Dictionary<string, Uri> keysAndUris, IStorage workerStorage, string fileName, string key)
        {
            var reducerUri = keysAndUris[key];
            var reducerWorkerId = GetWorkerId(reducerUri);
            var reducerEndpointUri = GetWorkerEndpointUri(reducerUri);
            var binding = new BasicHttpBinding();//new BasicHttpBinding("BasicHttpBinding_IRemoteWorkerService");

            using (var client = new WSClient.RemoteWorkerServiceClient(binding, new EndpointAddress(reducerEndpointUri)))
            {
                return client.PushFileAsync(reducerWorkerId, fileName, workerStorage.Read(fileName));
            }
        }

        private static void PushReduceResultsToCoordinatorInSeparateThread(int workerId, Uri callbackUri)
        {
            if (callbackUri == null)
            {
                return;
            }

            var t = new Thread(() =>
                {
                    var binding = new BasicHttpBinding();
                    try
                    {
                        using (var csclient = new CSClient.CoordinatorServiceClient(binding, new EndpointAddress(callbackUri)))
                        {
                            var storage = RemoteWorkerService.GetWorker(workerId).Storage;

                            var regex =
                                new Regex(
                                    string.Format(
                                        "^" + Core.Properties.Settings.Default.ReduceOutputFileName + "$",
                                        @"(?<Key>.+)",
                                        "[0-9]+",
                                        RegexExtensions.GuidRegexString));
                            var uris = storage.ListFiles();
                            foreach (var uri in uris)
                            {
                                var fileName = storage.GetFileName(uri);
                                if (regex.IsMatch(fileName))
                                {
                                    var value = storage.Read(fileName);
                                    csclient.AddToStorage(fileName, value);
                                }
                            }
                        }
                    }
                    catch(EndpointNotFoundException)
                    {
                        Console.WriteLine("Coordinator not found!");
                    }
                });

            t.Start();
        }
    }
}
