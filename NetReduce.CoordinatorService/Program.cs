namespace NetReduce.CoordinatorService
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    class Program
    {
        static void Main(string[] args)
        {
            var baseAddress = new Uri("http://0.0.0.0:7777/CoordinatorService.svc");
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
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
        }
    }
}
