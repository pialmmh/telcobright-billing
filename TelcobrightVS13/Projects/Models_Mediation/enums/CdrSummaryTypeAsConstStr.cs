using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MediationModel.enums;

namespace MediationModel
{
    public static class CdrSummaryTypeDictionary
    {
        private static readonly ConcurrentDictionary<string, CdrSummaryType> Types=new ConcurrentDictionary<string, CdrSummaryType>();
        private static bool IsInitialized = false;

        public static void Initialize()
        {
            if (IsInitialized) return;
            foreach (var enumValue in Enum.GetValues(typeof(CdrSummaryType)))
            {
               Types.TryAdd(enumValue.ToString(),(CdrSummaryType)enumValue); 
            }
            IsInitialized = true;
        }

        public static Dictionary<string, CdrSummaryType> GetTypes()
        {
            if (IsInitialized==false)
            {
                throw new Exception("Types are not initialized, method Initialize must be called at least once.");
            }
            return Types.Select(kv=>new
            {
                Key=kv.Key,
                Value=kv.Value
            }).ToDictionary(a=>a.Key, a=>a.Value);
        }
    }
}
