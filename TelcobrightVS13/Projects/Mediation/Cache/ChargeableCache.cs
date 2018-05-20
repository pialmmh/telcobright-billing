using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using MediationModel;

namespace TelcobrightMediation
{
    public class ChargeableCache : AbstractCache<acc_chargeable, string>//string=acc id
    {
        public ChargeableCache(Func<acc_chargeable, string> dictionaryKeyGenerator,
            Func<acc_chargeable, StringBuilder> insertCommandGenerator,
            Func<acc_chargeable, StringBuilder> updateWherePartGenerator,
            Func<acc_chargeable, StringBuilder> deleteWherePartGenerator)
            : base(dictionaryKeyGenerator,insertCommandGenerator,updateWherePartGenerator,deleteWherePartGenerator)//Constructor
        { }//pass to base
        public override void PopulateCache(Func<Dictionary<string, acc_chargeable>> methodToPopulate)//define method in Instance
        {
            foreach (var keyValuePair in methodToPopulate.Invoke())
            {
                if(base.Cache.TryAdd(keyValuePair.Key, keyValuePair.Value)==false)
                    throw new Exception("Could not add chargeable to concurrent dictionary while populating chargeable cache, probably duplicate item.");
            }
        }
    }
}