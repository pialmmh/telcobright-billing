using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading;
using tempCdr;
using WsTest;
using Wintellect.PowerCollections;

public static class Program
{
    //private static Timer _timer = null;
    private static readonly IEqualityComparer<int> equalityComparer;
    public static void Main(string[] args)
    {
        //decimal[] duration =
        //{
        //    1.09m, 1.99m, 2.10m, 19.09m,
        //    31.87m, 15.09m, 14.99m, 15.1m,
        //    0.5m, 2.3m, 4.7m, 6.2m, 8.9m,
        //    10.5m, 15.8m, 20.2m, 25.6m, 30.1m,
        //    3.2m, 7.5m, 11.8m, 16.2m, 20.6m,
        //    2.1m, 4.5m, 7.8m, 11.3m, 15.6m,
        //    6.5m, 9.7m, 12.4m, 15.8m, 19.3m,
        //    0.8m, 3.4m, 6.9m, 9.2m, 12.7m,
        //    5.3m, 8.6m, 13.2m, 17.5m, 21.9m,
        //    1.5m, 4.9m, 8.2m, 11.6m, 15.3m,
        //    12.5m, 15.9m, 20.3m, 25.7m, 30.1m,
        //    35.4m, 40.8m, 46.2m, 51.6m, 57.0m,
        //    62.4m, 67.8m, 73.2m, 78.6m, 84.0m,
        //    89.4m, 94.8m, 100.2m, 105.6m, 111.0m,
        //    0.09m,0.1m,120.0m

        //};
        //Console.WriteLine($"{"actual",-10}{"domestic",-10}{"intOut",-10}");

        //foreach (decimal d in duration)
        //{
        //    CasDurationHelper newCasDurationHelper = new CasDurationHelper();

        //    Console.WriteLine($"{d,-10}{newCasDurationHelper.getDomesticDur(d),-10}{newCasDurationHelper.getIntlOutDur(d),-10}");
        //}
        //Console.ReadLine();

        //Hash<KeyValuePair<string, Cdr>> hash = new Hash<KeyValuePair<string, Cdr>>(equalityComparer);

        //KeyValuePair<string, Cdr> previous = new KeyValuePair<string, Cdr>();
        //for (int i = 0; i < 100000000; i++)
        //{
        //    Cdr cdr = new Cdr();

        //    cdr.ServiceGroup = i;
        //    cdr.AnsIdOrig = i;
        //    cdr.CallForwardOrRoamingType = i;
        //    cdr.CalledPartyNOA = i;

        //    hash.Insert(new KeyValuePair<string, Cdr>(i.ToString(), cdr),false,out previous);

        //}
        //MultiDictionary<string, Cdr> multiDictionary = new MultiDictionary<string, Cdr>(true);
        Dictionary<string, Cdr> dictionary = new Dictionary<string, Cdr>();

        //BigList<Cdr> bigList = new BigList<Cdr>();
        for (int i = 0; i < 100000000; i++)
        {
            Cdr cdr = new Cdr();

            cdr.ServiceGroup = i;
            cdr.AnsIdOrig = i;
            cdr.CallForwardOrRoamingType = i;
            cdr.CalledPartyNOA = i;

            //multiDictionary.Add(i.ToString(), cdr);
            dictionary.Add(i.ToString(), cdr);
            //bigList.Add(cdr);
            if ((i+1) % 100000 == 0)
            {
                Console.WriteLine(i+1);
            }

        }

        //foreach (KeyValuePair<string, Cdr> item in multiDictionary.KeyValuePairs)
        //{
        //    Console.WriteLine("Key : {0} , Value : {1} ", item.Key, item.Value);
        //}

        Console.WriteLine(System.Environment.NewLine);
        Console.WriteLine(dictionary.Count);
        Console.WriteLine("-------- End Of MultiDictionary<TKey,TValue> ----------");


        Console.ReadKey(true);
    }

    //private static void TimerCallback(Object o)
    //{
    //    // Display the date/time when this method got called.
    //    string appName = (string)o;
    //    Console.Title = appName;
    //    Console.WriteLine($"In app={appName}: " + DateTime.Now);
    //}
}