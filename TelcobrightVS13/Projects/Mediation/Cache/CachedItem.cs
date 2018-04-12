namespace TelcobrightMediation
{
    public class CachedItem<TKey, TEntity>//this is returned when GetItem() is used against the cache
    {
        public TKey Key { get; }
        public TEntity Entity { get; }
        public CachedItem(TKey key, TEntity entity)
        {
            this.Key = key;
            this.Entity = entity;
        }
    }
}