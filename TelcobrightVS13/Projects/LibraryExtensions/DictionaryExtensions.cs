using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Quartz;

namespace LibraryExtensions
{
    public static class DictionaryExtensions
    {
        public static List<TValue> AppendAndGetListIfMissing<TKey, TValue>(this Dictionary<TKey, List<TValue>> dic,
            TKey key)
        {
            List<TValue> tempList;
            dic.TryGetValue(key, out tempList);
            if (tempList != null) return tempList;
            tempList = new List<TValue>();
            dic.Add(key, tempList);
            return tempList;
        }
    }
    public static class IDictionaryExtension
    {
        public static NameValueCollection ToNameValueCollection(this IDictionary<string,string> col)
        {
            NameValueCollection nv=new NameValueCollection();
            foreach (KeyValuePair<string,string> kv in col)
            {
                nv.Add(kv.Key,kv.Value);
            }
            return nv;
        }
    }
}
