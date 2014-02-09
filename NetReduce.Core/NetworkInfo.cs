namespace NetReduce.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    public class NetworkInfo
    {
        public static IEnumerable<UnicastIPAddressInformation> GetIpAddresses()
        {
            var result = new List<UnicastIPAddressInformation>();

            var nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in nics)
            {
                var ipProperties = nic.GetIPProperties();

                result.AddRange(ipProperties.UnicastAddresses.Where(addr => 
                    addr.Address.AddressFamily == AddressFamily.InterNetwork 
                    && !addr.Address.ToString().StartsWith("169.254.")
                    && !addr.Address.ToString().StartsWith("127.0.0.")));
            }

            return result;
        }
    }
}
