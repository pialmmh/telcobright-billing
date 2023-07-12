using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using LibraryExtensions;

public class Trie
{
    public char Path { get; }
    public string FullPath { get; }
    public Trie Parent { get; set; }
    public Dictionary<char, Trie> Children { get; set; }= new Dictionary<char, Trie>();
    public Trie(char path, Trie parent)
    {
        Path = path;
        Parent = parent;
        if (parent == null)
        {
            this.FullPath = path.ToString();
        }
        else//parent not null
        {
            parent.Children.Add(this.Path, this);
            this.FullPath = parent.FullPath + path;
        }
    }
}

public class Program
{
    public static void Main()
    {
        //List<string> trieData = new List<string>()
        //{
        //    "0",
        //    "01",
        //    "012",
        //    "02",
        //    "03",
        //    "01",
        //    "03",
        //    "04",
        //    "14"
        //};
        List<string> trieData = new List<string>()
        {
            "1613",
            "1613",
            "1327",
            "1071",
            "1327",
            "1071",
            "1613",
            "1071",
            "1613",
            "1613",
            "1613",
            "1613",
            "1613",
            "1613",
            "1613",
            "1613",
            "10.184.199.80 / 32", //addr.replace(" ","").trim().split('/')
            "10.184.199.81 / 32",
            "10.184.199.82 / 32",
            "10.184.199.83 / 32",
            "10.184.199.64 / 32",
            "10.184.199.65 / 32",
            "10.184.199.66 / 32",
            "10.184.199.67 / 32",
            "10.184.199.69 / 32",
            "10.184.199.68 / 32",
            "10.184.199.0 / 24",
            "10.184.0.0 / 22",
            "10.184.199.69 / 32",
            "10.184.199.70 / 32",
            "10.184.199.71 / 32",
            "10.184.199.72 / 32",
            "10.184.199.69",
        };

        Trie trie = CreateTrie(trieData);



        Trie root = new Trie
        (
            path: '0',
            parent:null
        );

        Trie child01 = new Trie('1', root);
        Trie child12 = new Trie('2',child01);

        Trie child02 = new Trie('2',root);
        Trie child03 = new Trie('3', root);

        char[] query = "012".ToCharArray();
        Trie bestMatch = findBestMatch(root, query);
        ;

/*
        //RunTests();
        List<string> srcList = new List<string>
        {
            "0",
            "01",
            "012",
            "02",
            "03"
        };
        List<char[]> srcCharArrays = srcList.Select(s => s.ToCharArray()).ToList();*/


    }

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
            bestMatch= findBestMatch(nextChild, tail);
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
        if (countDot == 0) return 1;                     //type = point code 
        if (countDot == 3 && countSlash == 0) return 2;  //type = ip with host bit
        return 3;                                        //type = ip with NO host bit

    }
    public static Trie CreateTrie(List<string> data)
    {
        Trie root = new Trie('*', null);
        data = data.OrderByDescending(s => s.Length).ToList();
        foreach (string s in data)
        {
            int type = findIpType(s);
            char[] chars = ("*" +s).ToCharArray();
            List<char> fullPath= new List<char>();
            foreach (char path in chars)
            {
                fullPath.Add(path);
                Trie bestMatch = findBestMatch(root, fullPath.ToArray());
                if(bestMatch.FullPath == new string(fullPath.ToArray())) continue;
                Trie child= new Trie(path,bestMatch);
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


    public static void RunTests()
    {
        // Test cases
        List<string> testCases = new List<string>
        {
            "10.0.0.0/24",
            "192.168.1.0/28",
            "172.16.0.0/16",
            "10.10.10.10/32"
        };

        foreach (string testCase in testCases)
        {
            Console.WriteLine($"CIDR Range: {testCase}");
            List<string> ipAddresses = CidrHelper.GetIPAddressesInCIDRRange(testCase);
            Console.WriteLine("Valid IP Addresses:");
            foreach (string ipAddress in ipAddresses)
            {
                Console.WriteLine(ipAddress);
            }
            Console.WriteLine("----------------------------------");
        }
    }
}
