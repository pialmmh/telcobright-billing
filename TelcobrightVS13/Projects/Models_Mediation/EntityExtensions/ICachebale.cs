using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace MediationModel
{
    public interface ICacheble
    {
        
    }
    public interface ICacheble<TEntity> : ICacheble //where TEntity:class
    {
        StringBuilder GetExtInsertValues();
        StringBuilder GetExtInsertCustom(Func<TEntity, string> externalInsertMethod);
        StringBuilder GetUpdateCommand(Func<TEntity, string> whereClauseMethod);
        StringBuilder GetUpdateCommandCustom(Func<TEntity, string> updateCommandMethodCustom);
        StringBuilder GetDeleteCommand(Func<TEntity, string> whereClauseMethod);
    }
    public interface ICacheble<TEntity, TKey> : ICacheble<TEntity> //where TEntity:class
    {
        string Name { get; }
        void PopulateItems(DbContext context);
        Dictionary<TKey, TEntity> GetItems();
        TEntity GetItem(TKey id);
    }
}
