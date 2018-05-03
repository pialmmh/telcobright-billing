using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using LibraryExtensions;
using MySql.Data.MySqlClient;
using MediationModel;
using net.bytebuddy.asm;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class AutoIncrementManager : AbstractCache<autoincrementcounter, long> //int=autoincrementCountertype enum
    {
        private static readonly object Locker = new object();
        public AutoIncrementManager(Func<autoincrementcounter, long> dictionaryKeyGenerator,
            Func<autoincrementcounter, string> insertCommandGenerator,
            Func<autoincrementcounter, string> updateCommandGenerator,
            Func<autoincrementcounter, string> deleteCommandPartGenerator)
            : base(dictionaryKeyGenerator, insertCommandGenerator, updateCommandGenerator,
                deleteCommandPartGenerator) //Constructor
        {} //pass to base

        public override void PopulateCache(Func<Dictionary<long, autoincrementcounter>> methodToPopulate)
        {
            foreach (var keyValuePair in methodToPopulate.Invoke())
            {
                if (base.Cache.TryAdd(keyValuePair.Key, keyValuePair.Value) == false)
                    throw new Exception(
                        "Could not add existing account to concurrent dictionary while populating AccountCache, probably duplicate item.");
            }
        }

        public long GetNewCounter(AutoIncrementCounterType counterType) //don't use .ToLower() for performance
        {
            lock (Locker)
            {
                autoincrementcounter thisCounter = null;
                int counterTypeInt = (int) counterType;
                base.Cache.TryGetValue(counterTypeInt, out thisCounter);
                if (thisCounter == null) //if doesn't exist in cache
                {
                    var newCounter = new autoincrementcounter()
                    {
                        tableName = counterType.ToString(),
                        value = 0
                    };
                    CachedItem<long, autoincrementcounter> boxedItem =
                        base.InsertWithKey(newCounter, counterTypeInt, c => c.value == 0);
                    thisCounter = boxedItem.Entity;
                }
                ++thisCounter.value;
                base.AddExternallyUpdatedEntityToUpdatedItems(thisCounter);
                return thisCounter.value;
            }
        }
    }
}
