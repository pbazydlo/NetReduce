namespace NetReduce.WorkerService.ConsoleHost
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading;

    using NetReduce.Core;
    using NetReduce.WorkerService.ConsoleHost.Properties;

    class Program
    {
        static void Main(string[] args)
        {
            var random = new Random();
            var ip = "0.0.0.0";
            var port = random.Next(49152, 65535);

            if (args.Length == 1)
            {
                port = int.Parse(args[0]);
            } 
            else if (args.Length == 2)
            {
                ip = args[0];
                port = int.Parse(args[1]);
            }

            var listenUri = string.Format("http://{0}:{1}/RemoteWorkerService.svc", ip, port);
            var callbackUri = listenUri;

            if (callbackUri.Contains("0.0.0.0"))
            {
                callbackUri = callbackUri.Replace("0.0.0.0", NetworkInfo.GetIpAddresses().First().Address.ToString());
            }

            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(RemoteWorkerService), new Uri(listenUri)))
            {
                Console.WriteLine("Worker URI is {0}", callbackUri);

                // register at coordinator
                RegisterAtCoordinator(new Uri(callbackUri), true);

                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);
                host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                Console.WriteLine("The service is ready at {0}", listenUri);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // unregister from coordinator
                RegisterAtCoordinator(new Uri(callbackUri), false);

                // Close the ServiceHost.
                host.Close();
            }

        }

        private static void RegisterAtCoordinator(Uri uri, bool register)
        {
            var t = new Thread(() =>
            {
                if (!string.IsNullOrEmpty(Settings.Default.CoordinatorUri))
                {
                    using (var coordinator = new CSClient.CoordinatorServiceClient(new BasicHttpBinding(), new EndpointAddress(Settings.Default.CoordinatorUri)))
                    {
                        try
                        {
                            Console.WriteLine("{1} at coordinator {0}... ", Settings.Default.CoordinatorUri, register ? "Registering" : "Unregistering");
                            if (register)
                            {
                                coordinator.AddWorkerAsync(uri).Wait();
                            }
                            else
                            {
                                coordinator.RemoveWorkerAsync(uri).Wait();
                            }
                        }
                        catch
                        {
                            Console.WriteLine("{1} at coordinator {0} failed ", Settings.Default.CoordinatorUri, register ? "Registering" : "Unregistering");
                        }
                    }
                }
            });

            t.Start();
        }
    }
}
