using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MediationModel;

namespace TelcobrightMediation
{
    public class TempTransactionCache : AbstractCache<acc_temp_transaction, string>//string=acc id
    {
        public TempTransactionCache(Func<acc_temp_transaction, string> dictionaryKeyGenerator,
            Func<acc_temp_transaction, string> insertCommandGenerator,
            Func<acc_temp_transaction, string> updateWherePartGenerator,
            Func<acc_temp_transaction, string> deleteWherePartGenerator)
            : base(dictionaryKeyGenerator,insertCommandGenerator,updateWherePartGenerator,deleteWherePartGenerator)//Constructor
        { }//pass to base
        public override void PopulateCache(Func<Dictionary<string, acc_temp_transaction>> methodToPopulate)//define method in Instance
        {
            foreach (var keyValuePair in methodToPopulate.Invoke())
            {
                if(base.Cache.TryAdd(keyValuePair.Key, keyValuePair.Value)==false)
                    throw new Exception("Could not add temp_transaction to concurrent dictionary while populating tempTransCache, probably duplicate item.");
            }
        }
    }
}