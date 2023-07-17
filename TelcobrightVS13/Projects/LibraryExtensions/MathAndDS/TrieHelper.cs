using System;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;

namespace LibraryExtensions
{
    public class TrieHelper
    {
        public static List<string> normalizePrefixOrIpAddress(List<string> data)
        {
            List<string> normalizeData = new List<string>();

            foreach (string s in data)
            {
                string str = s;
                TrieDataType type = findIpType(str);
                if (type == TrieDataType.Prefix) //type1 = point code           15266
                {
                    str = str.Replace(" ", "").Trim();
                }
                else //type2,3 = ip with or without net bit     10.0.0.8/24 or 10.0.0.8
                {
                    str = normalizeIpAddress(str, type);
                }
                normalizeData.Add(str);
            }
            return normalizeData;
        }

        private static string normalizeIpAddress(string str, TrieDataType type)
        {
            if (type == TrieDataType.IpAddressWithoutSlash)
            {
                str = str.Replace(" ", "").Trim();
                str = str + "/32";
            }
            string maskBits = str.Split('/')[0]; //10.0.0.8/32
            string netBits = str.Split('/')[1];

            //converting ip addr into bin str
            byte[] byteArr = maskBits.Split('.').Select(x => Convert.ToByte(x)).ToArray();
            string binStr = string.Concat(byteArr.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            int netBit = Convert.ToInt32(netBits);

            str = binStr.Substring(0, netBit);
            return str;
        }

        static TrieDataType findIpType(string s)
        {
            int countDot = 0, countSlash = 0;
            foreach (char c in s)
            {
                if (c == '.')
                    countDot++;
                else if (c == '/')
                    countSlash++;
            }
            if (countDot == 0) return TrieDataType.Prefix; //type1 = point code           15266
            if (countDot == 3 && countSlash == 1)
                return TrieDataType.CidrWithSlash; //type2 = ip with host bit     10.0.0.8/24
            return TrieDataType.IpAddressWithoutSlash; //type3 = ip with NO host bit  10.0.0.8

        }

        


        //public static void RunTests()
        //{
        //    // Test cases
        //    List<string> testCases = new List<string>
        //    {
        //        "10.0.0.0/24",
        //        "192.168.1.0/28",
        //        "172.16.0.0/16",
        //        "10.10.10.10/32"
        //    };

        //    foreach (string testCase in testCases)
        //    {
        //        Console.WriteLine($"CIDR Range: {testCase}");
        //        List<string> ipAddresses = CidrHelper.GetIPAddressesInCIDRRange(testCase);
        //        Console.WriteLine("Valid IP Addresses:");
        //        foreach (string ipAddress in ipAddresses)
        //        {
        //            Console.WriteLine(ipAddress);
        //        }
        //        Console.WriteLine("----------------------------------");
        //    }
        //}
    }
}