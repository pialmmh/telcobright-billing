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
            List<TValue> innerList;
            dic.TryGetValue(key, out innerList);
            if (innerList != null) return innerList;
            innerList = new List<TValue>();
            dic.Add(key, innerList);
            return innerList;
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
