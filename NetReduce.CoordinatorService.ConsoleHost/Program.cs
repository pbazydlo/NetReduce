namespace NetReduce.CoordinatorService.ConsoleHost
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading;

    using NetReduce.CoordinatorService.ConsoleHost.Properties;
    using NetReduce.Core;
    using NetReduce.Remote;

    class Program
    {
        static void Main(string[] args)
        {
            var listenUri = Settings.Default.ListenUri;
            var callbackUri = listenUri;

            if (callbackUri.Contains("0.0.0.0"))
            {
                callbackUri = callbackUri.Replace("0.0.0.0", NetworkInfo.GetIpAddresses().First().Address.ToString());
            }

            RemoteWorker<ServiceClientWrapper>.CoordinatorCallbackUri = new Uri(callbackUri);

            var baseAddress = new Uri(listenUri);
            // Create the ServiceHost.
            using (var host = new ServiceHost(typeof(CoordinatorService), baseAddress))
            {
                // Enable metadata publishing.
                var smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);
                host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                Console.WriteLine("Coordinator service is ready at {0}", baseAddress);
                Console.WriteLine("Callback Uri is {0}", RemoteWorker<ServiceClientWrapper>.CoordinatorCallbackUri);
                Console.WriteLine("Press <Enter> to stop the service.");

                /*var terminate = false;

                var t = new Thread(
                    () =>
                    {
                        while (!terminate)
                        {
                            var perf = PerformanceMonitor.GetPerformanceStatistics();
                            Console.Clear();
                            Console.WriteLine(perf.TotalProcessorTimeCounterPercent);
                            Console.WriteLine(perf.FreeRamCounterMB);
                            Console.WriteLine(perf.UsedRamCounterPercent);
                            var drvs = PerformanceMonitor.GetHddStatistics("c");
                            foreach (var drv in drvs)
                            {
                                Console.WriteLine("{0} {1} {2}", drv.Name, drv.FreeSpace, drv.TotalSize);
                            }
                        }
                    });
                t.Start();*/
                
                Console.ReadLine();
                /*terminate = true;
                t.Join();*/
                // Close the ServiceHost.
                host.Close();
            }
        }
    }
}
