using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MediationModel;
using Spring.Core;

namespace TelcobrightMediation
{
    public class SummaryCache<TEntity, TKey> : AbstractCache<TEntity, TKey> where TEntity : ICacheble<TEntity>,
        ISummary<TEntity, TKey>
    {
        public override string EntityOrTableName { get; }
        private readonly object _locker=new object();
        private AutoIncrementManager AutoIncrementManager { get; }
        public SummaryCache(string entityName,AutoIncrementManager autoIncrementManager , Func<TEntity, TKey> dictionaryKeyGenerator,
            Func<TEntity, string> insertCommandGenerator,
            Func<TEntity, string> updateCommandGenerator,
            Func<TEntity, string> deleteCommandGenerator)
            : base(dictionaryKeyGenerator, insertCommandGenerator, updateCommandGenerator,deleteCommandGenerator)//Constructor
        {
            this.EntityOrTableName = entityName;
            this.AutoIncrementManager = autoIncrementManager;
        }
        public override void PopulateCache(Func<Dictionary<TKey, TEntity>> methodToPopulate)//define method in Instance
        {
            foreach (var keyValuePair in methodToPopulate.Invoke())
            {
                if(base.Cache.TryAdd(keyValuePair.Key, keyValuePair.Value)==false)
                    throw new Exception("Could not add summary instance to concurrent dictionary while populating summarycache, probably duplicate item.");
            }
        }
        public void Merge(TEntity newSummary, SummaryMergeType mergeType, Func<TEntity, bool> pdValidatationMethodForInsert)
        {
            lock(this._locker)
            {
                TEntity existingSummary = default(TEntity);
                TKey key = newSummary.GetTupleKey();
                this.Cache.TryGetValue(key, out existingSummary);
                if (existingSummary == null)
                {
                    if (mergeType == SummaryMergeType.Substract)
                    {
                        //todo: remove temp code
                        foreach (var cacheKey in this.Cache.Keys)
                        {
                            Debug.Print(cacheKey.ToString());
                        }
                        throw new NotSupportedException("Previous summary instance cannot be null for summary substraction.");
                    }
                    //summaries are merged in Cache, cache.Insert without value copy will have the same reference 
                    //for source summary entity & the mergable version in cache
                    //when a next summary instance to update the merged cache, the first source object will also get changed.
                    //to prevent that, use GetValueCopy();
                    var clonedSummary = (TEntity) newSummary.CloneWithFakeId();
                    clonedSummary.id = this.AutoIncrementManager.GetNewCounter(this.EntityOrTableName);
                    if (mergeType == SummaryMergeType.Add)
                    {
                        InsertWithKey(clonedSummary, key, pdValidatationMethodForInsert);
                    }
                    else 
                    {
                        throw new NotSupportedException("Summary merge type must be add or substract.");
                    }
                }
                else
                {
                    if (mergeType == SummaryMergeType.Substract)
                    {
                        newSummary.Multiply(-1); //negate
                    }
                    this.UpdateThroughCache(this.DictionaryKeyGenerator(existingSummary),
                        e => e.Merge(newSummary));
                }
            }
        }
    }
}