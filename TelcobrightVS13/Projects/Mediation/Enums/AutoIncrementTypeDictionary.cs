using System;
using System.Collections.Generic;
using System.Linq;

namespace TelcobrightMediation
{
    public static class AutoIncrementTypeDictionary
    {
        public static Dictionary<string, AutoIncrementCounterType> EnumTypes { get; } =
            Enum.GetValues(typeof(AutoIncrementCounterType))
                .Cast<AutoIncrementCounterType>()
                .ToDictionary(t => Enum.GetName(typeof(AutoIncrementCounterType), t), t => t);
    }
}