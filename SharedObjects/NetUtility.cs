using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SharedObjects
{
    public static class NetUtility
    {
        public static IPAddress GetHostAddressIPv4(string hostname)
        {
            foreach (var address in Dns.GetHostAddresses(hostname))
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address;
            return null;
        }

        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
