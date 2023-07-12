using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    //class cidrHelper
    public static class CidrHelper
    {
        public static List<string> GetIPAddressesInCIDRRange(string cidrRange)
        {
            List<string> ipAddresses = new List<string>();

            string[] parts = cidrRange.Split('/');
            string ipAddressString = parts[0];
            string subnetMaskString = parts[1];

            IPAddress ipAddress = IPAddress.Parse(ipAddressString);
            byte[] ipAddressBytes = ipAddress.GetAddressBytes();
            byte[] subnetMaskBytes = new byte[4];

            int subnetMaskBits = int.Parse(subnetMaskString);
            int fullBytes = subnetMaskBits / 8;
            int remainderBits = subnetMaskBits % 8;

            for (int i = 0; i < fullBytes; i++)
            {
                subnetMaskBytes[i] = 255;
            }

            if (remainderBits > 0)
            {
                subnetMaskBytes[fullBytes] = (byte)(255 << (8 - remainderBits));
            }

            byte[] startIPAddressBytes = new byte[4];
            byte[] endIPAddressBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                startIPAddressBytes[i] = (byte)(ipAddressBytes[i] & subnetMaskBytes[i]);
                endIPAddressBytes[i] = (byte)((ipAddressBytes[i] & subnetMaskBytes[i]) | (~subnetMaskBytes[i] & 255));
            }

            long startIPAddressValue = BitConverter.ToInt32(startIPAddressBytes, 0);
            long endIPAddressValue = BitConverter.ToInt32(endIPAddressBytes, 0);

            for (long i = startIPAddressValue; i <= endIPAddressValue; i++)
            {
                byte[] currentIPAddressBytes = BitConverter.GetBytes(i);
                Array.Reverse(currentIPAddressBytes); // Convert from little-endian to big-endian

                string currentIPAddressString = new IPAddress(currentIPAddressBytes).ToString();
                ipAddresses.Add(currentIPAddressString);
            }

            return ipAddresses;
        }

        
    }
}
