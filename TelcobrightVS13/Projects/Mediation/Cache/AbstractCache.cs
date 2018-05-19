using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MediationModel;
using Spring.Caching;

namespace TelcobrightMediation
{
    public abstract class AbstractCache<TEntity, TKey> : ICache where TEntity : ICacheble<TEntity>
    {
        private readonly object locker = new object();
        protected AbstractCache(Func<TEntity, TKey> dictionaryKeyGenerator,
            Func<TEntity, StringBuilder> insertCommandGenerator,
            Func<TEntity, StringBuilder> updateCommandGenerator, Func<TEntity, StringBuilder> deleteCommandGenerator)
        {//constructor
            this.DictionaryKeyGenerator = dictionaryKeyGenerator;
            this.UpdateCommandGenerator = updateCommandGenerator;
            this.InsertCommandGenerator = insertCommandGenerator;
            this.DeleteCommandGenerator = deleteCommandGenerator;
        }
        public Func<TEntity, TKey> DictionaryKeyGenerator { get; protected set; }
        public Func<TEntity, StringBuilder> InsertCommandGenerator { get; protected set; }
        public Func<TEntity, StringBuilder> UpdateCommandGenerator { get; protected set; }
        public Func<TEntity, StringBuilder> DeleteCommandGenerator { get; protected set; }
        public virtual string EntityOrTableName => typeof(TEntity).Name;
        protected ConcurrentDictionary<TKey, TEntity> Cache { get; } = new ConcurrentDictionary<TKey, TEntity>();
        protected readonly ConcurrentDictionary<TKey, TEntity> InsertedItems = new ConcurrentDictionary<TKey, TEntity>();
        protected readonly ConcurrentDictionary<TKey, TEntity> UpdatedItems = new ConcurrentDictionary<TKey, TEntity>();
        protected readonly ConcurrentDictionary<TKey, TEntity> DeletedItems = new ConcurrentDictionary<TKey, TEntity>();
        //keep populate method outside of the cache object for more control in data filtering, pass a method
        //separate instance of uomcache can contain uoms e.g. currency, time units etc.
        public abstract void PopulateCache(Func<Dictionary<TKey, TEntity>> methodToPopulate);
        //public Func<TEntity, string> methodForWhereSql { get; set; }
        public bool Exists(TKey key)
        {
            return this.Cache.ContainsKey(key);
        }

        public virtual CachedItem<TKey, TEntity> GetItemByKey(TKey key)
        {
            //it's best to return the item with already generated key, because the same key will be used to update the entity
            //will have good performance impact

            TEntity selectedItem = default(TEntity);
            this.Cache.TryGetValue(key, out selectedItem);
            if (selectedItem == null)
            {
                return null;
            }
            return new CachedItem<TKey, TEntity>(key, selectedItem);
        }

        public virtual List<TEntity> GetItems()
        {
            return this.Cache.Values.ToList();
        }

        public virtual List<TEntity> GetInsertedItems()
        {
            return this.InsertedItems.Values.ToList();
        }

        public virtual List<TEntity> GetUpdatedItems()
        {
            return this.UpdatedItems.Values.ToList();
        }

        public virtual List<TEntity> GetDeletedItems()
        {
            return this.DeletedItems.Values.ToList();
        }

        public int NumberOfInsertedItems => this.InsertedItems.Count;
        public int NumberOfUpdatedItems => this.UpdatedItems.Count;
        public int NumberOfDeletedItems => this.DeletedItems.Count;
        public virtual void Delete(TKey key)
        {
            lock (this.locker)
            {
                TEntity toBeDeleted = default(TEntity);
                this.Cache.TryGetValue(key, out toBeDeleted);
                if (toBeDeleted == null) throw new Exception("Item does not exist in cache.");
                if (this.DeletedItems.ContainsKey(key) == false) //could've already be marked as deleted
                {
                    if (this.DeletedItems.TryAdd(key, toBeDeleted) == false)
                        throw new Exception("Could not add item to deleted items, probably duplicate item.");
                }
                TEntity ignoredItem = default(TEntity);
                this.Cache.TryRemove(this.DictionaryKeyGenerator(toBeDeleted), out ignoredItem);
            }
        }
        public virtual void DeleteAll()
        {
            lock (this.locker)
            {
                List<TKey> keys = this.Cache.Keys.ToList();
                keys.ForEach(key => this.Delete(key));
            }
        }
        public virtual void UpdateThroughCache(TKey key, Action<TEntity> entityUpdater)
        {
            lock (this.locker)
            {
                if (this.DeletedItems.ContainsKey(key) == true)
                    throw new Exception("Item to be updated already exists in deleted items which is not allowed");
                TEntity entityToBeUpdated = default(TEntity);
                this.Cache.TryGetValue(key, out entityToBeUpdated);
                if (entityToBeUpdated == null) throw new Exception("Item does not exist in cache.");
                entityUpdater.Invoke(entityToBeUpdated);
                if (this.InsertedItems.ContainsKey(key) == false && this.UpdatedItems.ContainsKey(key) == false)
                {
                    if (this.UpdatedItems.TryAdd(key, entityToBeUpdated)==false)
                        throw new Exception("Could not add item to updated items, probably duplicate item.");
                }
            }
        }
        public virtual void AddExternallyUpdatedEntityToUpdatedItems(TEntity updatedEntity)
        {
            lock (this.locker)
            {
                TKey key = this.DictionaryKeyGenerator(updatedEntity);
                if (this.DeletedItems.ContainsKey(key) == true)
                    throw new Exception("Item to be updated already exists in deleted items which is not allowed");
                if (this.InsertedItems.ContainsKey(key) == false && this.UpdatedItems.ContainsKey(key) == false)
                {
                    if (this.UpdatedItems.TryAdd(key, updatedEntity) == false)
                        throw new Exception("Could not add item to updated items, probably duplicate item.");
                }
            }
        }
        public virtual CachedItem<TKey, TEntity> Insert(TEntity newItem, Func<TEntity, bool> pkValidationMethod)
        {
            lock (this.locker)
            {
                TKey key = this.DictionaryKeyGenerator(newItem);
                return InsertWithKey(newItem, key, pkValidationMethod);
            }
        }

        public virtual CachedItem<TKey, TEntity> InsertWithKey(TEntity newItem, TKey key, Func<TEntity, bool> pkValidationMethod)
        {
            lock (this.locker)
            {
                if (pkValidationMethod.Invoke(newItem) == false)
                    throw new Exception("Primary key is either missing or invalid.");
                if (this.UpdatedItems.ContainsKey(key) == true)
                    throw new Exception("Item to be inserted already exists in updated items which is not allowed.");
                if (this.DeletedItems.ContainsKey(key) == true)
                    throw new Exception("Item to be inserted already exists in deleted items which is not allowed.");
                if (this.Cache.TryAdd(key, newItem) == false)
                    throw new Exception("Could not add item to cache, probably duplicate item.");
                if (this.InsertedItems.TryAdd(key, newItem) == false)
                    throw new Exception("Could not add item to inserted items, probably duplicate item.");
                return new CachedItem<TKey, TEntity>(key, newItem);
            }
        }

        public virtual void WriteAllChanges(DbCommand cmd, int segmentSize)
        {
            lock (this.locker)
            {
                this.WriteDeletes(cmd, segmentSize);
                this.WriteInserts(cmd, segmentSize);
                this.WriteUpdates(cmd, segmentSize);
            }
        }

        public virtual int WriteInserts(DbCommand cmd, int segmentSize)
        {
            lock (this.locker)
            {
                int affectedRecordCount = 0;
                if (this.InsertedItems.Any() == false) return affectedRecordCount;
                if (this.InsertCommandGenerator == null)
                {
                    throw new Exception("InsertCommandGenerator is not set and null.");
                }
                string extInsertHeader = StaticExtInsertColumnParsedDic.GetParsedDic()[this.EntityOrTableName];
                int startAt = 0;
                CollectionSegmenter<TEntity> segments =
                    new CollectionSegmenter<TEntity>(this.GetInsertedItems().AsParallel(), startAt);
                segments.ExecuteMethodInSegments(segmentSize,
                    segment =>
                    {
                        var sqlsAsStringBuilders = segment.AsParallel().Select(c => this.InsertCommandGenerator(c));
                        cmd.CommandText = new StringBuilder(extInsertHeader)
                            .Append(StringBuilderJoiner.Join(",",sqlsAsStringBuilders)).ToString();

                        affectedRecordCount += cmd.ExecuteNonQuery();
                    });
                if (affectedRecordCount != this.InsertedItems.Count)
                    throw new Exception("Inserted records count does not match the same number in cache.");
                this.InsertedItems.Clear();
                return affectedRecordCount;
            }
        }

        public virtual void WriteUpdates(DbCommand cmd, int segmentSize)
        {
            lock (this.locker)
            {
                if (this.UpdatedItems.Any() == false) return;
                int updateWriteCount = 0;
                if (this.UpdateCommandGenerator == null)
                {
                    throw new Exception("UpdateCommandGenerator is null/not defined.");
                }
                int startAt = 0;
                CollectionSegmenter<TEntity> segments = new CollectionSegmenter<TEntity>(this.GetUpdatedItems(), startAt);
                segments.ExecuteMethodInSegments(segmentSize,
                    segment =>
                    {
                        var sqlsAsStringBuilders = segment.AsParallel().Select(c => this.UpdateCommandGenerator(c));
                        cmd.CommandText = StringBuilderJoiner.Join(";", sqlsAsStringBuilders).ToString();
                        updateWriteCount += cmd.ExecuteNonQuery();
                    });
                if (updateWriteCount != this.UpdatedItems.Count)
                    throw new Exception("Updated records count in database does not match UpdateCache count.");
                this.UpdatedItems.Clear();
            }
        }

        public virtual void WriteDeletes(DbCommand cmd, int segmentSize)
        {
            lock (this.locker)
            {
                if (this.DeletedItems.Any() == false) return;
                int deleteWriteCount = 0;
                if (this.DeleteCommandGenerator == null)
                {
                    throw new Exception("DeleteCommandGenerator is null/not defined.");
                }
                int startAt = 0;
                CollectionSegmenter<TEntity> segments = new CollectionSegmenter<TEntity>(this.GetDeletedItems(), startAt);
                segments.ExecuteMethodInSegments(segmentSize,
                    segment =>
                    {
                        var sqlsAsStringBuilders = segment.AsParallel().Select(c => this.DeleteCommandGenerator(c));
                        cmd.CommandText = StringBuilderJoiner.Join(";", sqlsAsStringBuilders).ToString();
                        deleteWriteCount+= cmd.ExecuteNonQuery();
                    });
                if (deleteWriteCount != this.DeletedItems.Count)
                    throw new Exception("Deleted records count in database does not match DeleteCache count.");
                this.DeletedItems.Clear();
            }
        }
    }
}
