using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace MediationModel
{
    public interface ICacheble
    {
        
    }
    public interface ICacheble<TEntity> : ICacheble //where TEntity:class
    {
        string GetExtInsertValues();
        string GetExtInsertCustom(Func<TEntity, string> externalInsertMethod);
        string GetUpdateCommand(Func<TEntity, string> whereClauseMethod);
        string GetUpdateCommandCustom(Func<TEntity, string> updateCommandMethodCustom);
        string GetDeleteCommand(Func<TEntity, string> whereClauseMethod);
    }
    public interface ICacheble<TEntity, TKey> : ICacheble<TEntity> //where TEntity:class
    {
        string Name { get; }
        void PopulateItems(DbContext context);
        Dictionary<TKey, TEntity> GetItems();
        TEntity GetItem(TKey id);
    }
}
