using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MediationModel;

namespace TelcobrightMediation
{
    public class AccountCache : AbstractCache<account, string> //string=acc id
    {
        public ConcurrentDictionary<string, string> IndexById { get; } = new ConcurrentDictionary<string, string>();
        private readonly object _locker = new object();

        public AccountCache(Func<account, string> dictionaryKeyGenerator,
            Func<account, string> insertCommandGenerator,
            Func<account, string> updateCommandGenerator,
            Func<account, string> deleteCommandPartGenerator)
            : base(dictionaryKeyGenerator, insertCommandGenerator, updateCommandGenerator,
                deleteCommandPartGenerator) //Constructor
        {
        } //pass to base

        public override void
            PopulateCache(Func<Dictionary<string, account>> methodToPopulate) //define method in Instance
        {
            foreach (var keyValuePair in methodToPopulate.Invoke())
            {
                if (base.Cache.TryAdd(keyValuePair.Key, keyValuePair.Value) == false)
                    throw new Exception(
                        "Could not add existing account to concurrent dictionary while populating AccountCache, probably duplicate item.");
            }
            base.GetItems().ForEach(acc => UpdateIndexWithNewEntity(acc));
        }

        public override CachedItem<string, account> Insert(account newItem, Func<account, bool> pkValidationMethod)
        {
            throw new Exception("Use of Method 'Insert' is prohibited for AccountCache, use 'InsertWithKey' instead.");
        }

        public override CachedItem<string, account> InsertWithKey(account newItem, string key,
            Func<account, bool> pkValidationMethod)
        {
            lock (this._locker)
            {
                CachedItem<string, account> insertedItem = base.InsertWithKey(newItem, key, pkValidationMethod);
                account insertedAccount = insertedItem.Entity;
                UpdateIndexWithNewEntity(insertedAccount);
                return insertedItem;
            }
        }

        private void UpdateIndexWithNewEntity(account acc)
        {
            lock (this._locker)
            {
                if (this.IndexById.TryAdd(acc.id.ToString(), acc.accountName) == false)
                {
                    throw new Exception("Could not update concurrent dictionary IndexByid in AccountCache.");
                }
            }
        }

        public override void Delete(string key)
        {
            throw new NotImplementedException("Delete is prohibited for account cache.");
        }

        public override void DeleteAll()
        {
            throw new NotImplementedException("Delete is prohibited for account cache.");
        }
    }
}