using System;
using System.Collections.Generic;
using System.Linq;

public class TrieHelper
{


    public static Trie findBestMatch(Trie trie, char[] query)
    {
        char head = query[0];
        char[] tail = query.Skip(1).ToArray();
        //013
        //0->0
        //1->01
        //3 -> null

        Trie bestMatch = null;
        if (head == trie.Path)
        {
            bestMatch = trie;
            var children = bestMatch.Children;
            if (children == null || tail.Length == 0)
            {
                return bestMatch;
            }
            Trie nextChild = null;
            children.TryGetValue(tail[0], out nextChild);//tail[0]=1

            if (nextChild == null)
            {
                return bestMatch;
            }
            bestMatch = findBestMatch(nextChild, tail);
            return bestMatch;
        }
        return null;
    }

    public static int findIpType(string s)
    {
        int countDot = 0, countSlash = 0;
        foreach (char c in s)
        {
            if (c == '.')
                countDot++;
            else if (c == '/')
                countSlash++;
        }
        if (countDot == 0) return 1;                     //type1 = point code           15266
        if (countDot == 3 && countSlash == 1) return 2;  //type2 = ip with host bit     10.0.0.8/24
        return 3;                                        //type3 = ip with NO host bit  10.0.0.8

    }

    public static List<string> convertBestMatchtToIpAddr(List<Trie> bestMatchList)
    {
        List<string> convertedIp = new List<string>();

        return convertedIp;
    }

    public static List<string> normalizeAddressList(List<string> data)
    {
        Trie root = new Trie('*', null);
        //data = data.OrderByDescending(s => s.Length).ToList();
        List<string> normalizedIpList = new List<string>();
        foreach (string s in data)
        {
            string str = s;
            int type = findIpType(str);
            if (type == 1)                        //type1 = point code           15266
            {
                str = str.Replace(" ", "").Trim();

            }
            else if (type == 2 || type == 3)      //type2,3 = ip with or without net bit     10.0.0.8/24 or 10.0.0.8
            {
                if (type == 3)
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
            }
            normalizedIpList.Add(str);
        }
        return normalizedIpList;
    }


    public static Trie CreateTrie(List<string> data)
    {
        Trie root = new Trie('*', null);
        data = data.OrderByDescending(s => s.Length).ToList();
        foreach (string s in data)
        {
            string str = s;
            int type = findIpType(str);
            if (type == 1)              //type1 = point code           15266
            {
                str = str.Replace(" ", "").Trim();
            }
            else if (type == 2 || type == 3) //type2,3 = ip with or without net bit     10.0.0.8/24 or 10.0.0.8
            {
                if (type == 3)
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
            }



            //////
            char[] chars = ("*" + str).ToCharArray();
            List<char> fullPath = new List<char>();
            foreach (char path in chars)
            {
                fullPath.Add(path);
                Trie bestMatch = findBestMatch(root, fullPath.ToArray());
                if (bestMatch.FullPath == new string(fullPath.ToArray())) continue;
                Trie child = new Trie(path, bestMatch);
            }
        }
        return root;
        char[] query;
        ;
        //013
        //0->0
        //1->01
        //3 -> null

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