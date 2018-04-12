using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public class AutoIncrementCounterCache : AbstractCache<autoincrementcounter, string>//string=acc name
    {
        public AutoIncrementCounterCache(Func<autoincrementcounter, string> dictionaryKeyGenerator,
            Func<autoincrementcounter, string> insertCommandGenerator,
            Func<autoincrementcounter, string> updateWherePartGenerator,
            Func<autoincrementcounter, string> deleteWherePartGenerator)
            : base(dictionaryKeyGenerator, insertCommandGenerator, updateWherePartGenerator,deleteWherePartGenerator) { }
        public override void PopulateCache(Func<Dictionary<string, autoincrementcounter>> methodToPopulate)
        {
            foreach (var keyValuePair in methodToPopulate.Invoke())
            {
                if(base.Cache.TryAdd(keyValuePair.Key, keyValuePair.Value)==false)
                    throw new Exception("Could not add autoincrementcounter to concurrent dictionary while populating Cache, probably duplicate item.");
            }
        }
    }
}