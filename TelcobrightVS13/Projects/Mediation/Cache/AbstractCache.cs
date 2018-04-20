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
        private readonly object _locker = new object();
        protected AbstractCache(Func<TEntity, TKey> dictionaryKeyGenerator,
            Func<TEntity, string> insertCommandGenerator,
            Func<TEntity, string> updateCommandGenerator, Func<TEntity, string> deleteCommandGenerator)
        {//constructor
            this.DictionaryKeyGenerator = dictionaryKeyGenerator;
            this.UpdateCommandGenerator = updateCommandGenerator;
            this.InsertCommandGenerator = insertCommandGenerator;
            this.DeleteCommandGenerator = deleteCommandGenerator;
        }
        public Func<TEntity, TKey> DictionaryKeyGenerator { get; protected set; }
        public Func<TEntity, string> InsertCommandGenerator { get; protected set; }
        public Func<TEntity, string> UpdateCommandGenerator { get; protected set; }
        public Func<TEntity, string> DeleteCommandGenerator { get; protected set; }
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
            lock (this._locker)
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
            lock (this._locker)
            {
                List<TKey> keys = this.Cache.Keys.ToList();
                keys.ForEach(key => this.Delete(key));
            }
        }
        public virtual void UpdateThroughCache(TKey key, Action<TEntity> entityUpdater)
        {
            lock (this._locker)
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
            lock (this._locker)
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
            lock (this._locker)
            {
                TKey key = this.DictionaryKeyGenerator(newItem);
                return InsertWithKey(newItem, key, pkValidationMethod);
            }
        }

        public virtual CachedItem<TKey, TEntity> InsertWithKey(TEntity newItem, TKey key, Func<TEntity, bool> pkValidationMethod)
        {
            lock (this._locker)
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
            lock (this._locker)
            {
                this.WriteDeletes(cmd, segmentSize);
                this.WriteInserts(cmd, segmentSize);
                this.WriteUpdates(cmd, segmentSize);
            }
        }

        public virtual int WriteInserts(DbCommand cmd, int segmentSize)
        {
            lock (this._locker)
            {
                int affectedRecordCount = 0;
                if (this.InsertedItems.Any() == false) return affectedRecordCount;
                if (this.InsertCommandGenerator == null)
                {
                    throw new Exception("InsertCommandGenerator is not set and null.");
                }
                string extInsertHeader = StaticExtInsertColumnParsedDic.GetParsedDic()[this.EntityOrTableName];
                var sqls = this.GetInsertedItems().Select(c => this.InsertCommandGenerator(c)).ToList();
                int startAt = 0;
                CollectionSegmenter<string> segments = new CollectionSegmenter<string>(sqls, startAt);
                segments.ExecuteMethodInSegments(segmentSize,
                    segment =>
                    {
                        cmd.CommandText = (new StringBuilder(extInsertHeader).Append(string.Join(",", segment))).ToString();
                        affectedRecordCount += cmd.ExecuteNonQuery();
                    });
                this.InsertedItems.Clear();
                if (affectedRecordCount != sqls.Count)
                    throw new Exception("Inserted records count does not match the same number in cache.");
                return affectedRecordCount;
            }
        }

        public virtual void WriteUpdates(DbCommand cmd, int segmentSize)
        {
            lock (this._locker)
            {
                if (this.UpdatedItems.Any() == false) return;
                if (this.UpdateCommandGenerator == null)
                {
                    throw new Exception("UpdateCommandGenerator is not set and null.");
                }
                var sqls = this.GetUpdatedItems().Select(c => this.UpdateCommandGenerator(c)).ToList();
                int startAt = 0;
                CollectionSegmenter<string> segments = new CollectionSegmenter<string>(sqls, startAt);
                segments.ExecuteMethodInSegments(segmentSize,
                    segment =>
                    {
                        cmd.CommandText = (string.Join(String.Empty, segment)).ToString();
                        cmd.ExecuteNonQuery();
                    });
                this.UpdatedItems.Clear();
            }
        }

        public virtual void WriteDeletes(DbCommand cmd, int segmentSize)
        {
            lock (this._locker)
            {
                if (this.DeletedItems.Any() == false) return;
                if (this.DeleteCommandGenerator == null)
                {
                    throw new Exception("DeleteCommandGenerator is not set and null.");
                }
                var sqls = this.GetDeletedItems().Select(c => this.DeleteCommandGenerator(c)).ToList();
                int startAt = 0;
                CollectionSegmenter<string> segments = new CollectionSegmenter<string>(sqls, startAt);
                segments.ExecuteMethodInSegments(segmentSize,
                    segment =>
                    {
                        cmd.CommandText = (string.Join(String.Empty, segment)).ToString();
                        cmd.ExecuteNonQuery();
                    });
                this.DeletedItems.Clear();
            }
        }
    }
}
