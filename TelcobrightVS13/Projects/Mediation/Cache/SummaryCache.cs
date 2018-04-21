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
                        //todo remove temp code
                        if (this.EntityOrTableName.Contains("sum_voice"))
                        {
                            //var possMatch = string.Join(Environment.NewLine,
                            //    this.Cache.Values.Select(s => s.GetTupleKey().ToString()));
                            var tuples = this.Cache.Values.Select(s => s.GetTupleKey().ToString()).ToList();
                            var tup = tuples
                                .First(t => t.ToString()
                                    .Contains(
                                        //"1, 584, 1, Ivoco_SBC, Novo1-IOF, 0.01890000, 0.02000000, 114.130.252.171:5060, 203.201.48.36:5060, , 880, 0034, , 23, USD, , , '2017-12-01 00:00:00', USD, USD"));
                                        key.ToString()));
                            //var matchStr = possMatch.ToString() == key.ToString();
                            //Debug.Flush();
                            //Debug.Print(tupleKeys);
                        }
                        //end temp code
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