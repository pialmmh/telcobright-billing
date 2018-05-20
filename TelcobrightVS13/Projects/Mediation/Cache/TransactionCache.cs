using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using MediationModel;

namespace TelcobrightMediation
{
    public class TransactionCache : AbstractCache<acc_transaction, string>//string=acc id
    {
        public TransactionCache(Func<acc_transaction, string> dictionaryKeyGenerator,
            Func<acc_transaction, StringBuilder> insertCommandGenerator,
            Func<acc_transaction, StringBuilder> updateWherePartGenerator,
            Func<acc_transaction, StringBuilder> deleteWherePartGenerator)
            : base(dictionaryKeyGenerator,insertCommandGenerator,updateWherePartGenerator,deleteWherePartGenerator)//Constructor
        { }//pass to base
        public override void PopulateCache(Func<Dictionary<string, acc_transaction>> methodToPopulate)//define method in Instance
        {
            foreach (var keyValuePair in methodToPopulate.Invoke())
            {
                if(base.Cache.TryAdd(keyValuePair.Key, keyValuePair.Value)==false)
                    throw new Exception("Could not add transaction to concurrent dictionary while populating cache, probably duplicate item.");
            }
        }
    }
}