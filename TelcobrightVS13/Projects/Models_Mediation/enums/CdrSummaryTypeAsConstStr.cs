using System;
using System.Collections.Generic;
using MediationModel.enums;

namespace MediationModel
{
    public static class CdrSummaryTypeDictionary
    {
        private static readonly Dictionary<string, CdrSummaryType> Types=new Dictionary<string, CdrSummaryType>();
        private static bool IsInitialized = false;

        public static void Initialize()
        {
            if (IsInitialized) return;
            foreach (var enumValue in Enum.GetValues(typeof(CdrSummaryType)))
            {
               Types.Add(enumValue.ToString(),(CdrSummaryType)enumValue); 
            }
            IsInitialized = true;
        }

        public static Dictionary<string, CdrSummaryType> GetTypes()
        {
            if (IsInitialized==false)
            {
                throw new Exception("Types are not initialized, method Initialize must be called at least once.");
            }
            return Types;
        }
    }
}
