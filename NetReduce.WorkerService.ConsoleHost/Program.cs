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
            var listenUri = new Uri("http://0.0.0.0:28756/RemoteWorkerService.svc");
            var callbackUri = listenUri.ToString();

            if (callbackUri.Contains("0.0.0.0"))
            {
                callbackUri = callbackUri.Replace("0.0.0.0", NetworkInfo.GetIpAddresses().First().Address.ToString());
            }

            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(RemoteWorkerService), listenUri))
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
