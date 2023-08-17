using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using LibraryExtensions;
using TelcobrightInfra;
public class Program
{
    public static void Main()
    {
        List<string> rawData = new List<string>()
        {
            "1011","10","1199","1011" ,"11995","1198"
            // "160.0.0.0/3",
              //"64.0.0.0/3",
            //"1613",
            //"1613",
            //"1327",
            //"1071",
            //"1327",
            //"1071",
            //"1613",
            //"1071",
            //"1613",
            //"1613",
            //"1613",
            //"1613",
            //"1613",
            //"1613",
            //"1613",
            //"1613",
            //"10.184.199.80 / 32", //addr.replace(" ","").trim().split('/')
            //"10.184.199.81 / 32",
            //"10.184.199.82 / 32",
            //"10.184.199.83 / 32",
            //"10.184.199.64 / 32",
            //"10.184.199.65 / 32",
            //"10.184.199.66 / 32",
            //"10.184.199.67 / 32",
            //"10.184.199.69 / 32",
            //"10.184.199.68 / 32",
            //"10.184.199.0 / 24",
            //"10.184.0.0 / 22",
            //"10.184.199.69 / 32",
            //"10.184.199.70 / 32",
            //"10.184.199.71 / 32",
            //"10.184.199.72 / 12",
            //"10.184.199.69",
        };
        //List<string> trieData = TrieHelper.normalizePrefixOrIpAddress(rawData);
        //Trie trie= new Trie(trieData,'^');
        //;
        //trie.print();
        //Trie t = trie.findBestMatch("11995");


        //DaywiseTableManager d = new DaywiseTableManager();

        //TupleIncrementManager tim = new TupleIncrementManager();


       
        //TupleIncrementManager.getIncrementalValue("Abul");


    }
}