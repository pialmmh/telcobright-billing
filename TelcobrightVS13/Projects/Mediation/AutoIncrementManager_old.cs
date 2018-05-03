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
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    class MyLocker_old
    {
        private int id;

        public MyLocker_old(int id)
        {
            this.id = id;
        }
    }
    public class AutoIncrementManager_old
    {
        private ConcurrentDictionary<string, autoincrementcounter> Counters { get; } =
                                    new ConcurrentDictionary<string, autoincrementcounter>();

        private ConcurrentDictionary<string, autoincrementcounter> UpdatedCounters { get; } =
                                    new ConcurrentDictionary<string, autoincrementcounter>();

        private ConcurrentDictionary<string, autoincrementcounter> InsertedCounters { get; } =
                                    new ConcurrentDictionary<string, autoincrementcounter>();
        private PartnerEntities Context { get; }
        private readonly object _locker = new object();
        public AutoIncrementManager_old(PartnerEntities context)
        {
            this.Context = context;
            var existingCounters = this.Context.autoincrementcounters.ToDictionary(c => c.tableName);
            foreach (KeyValuePair<string, autoincrementcounter> kv in existingCounters)
            {
                if (this.Counters.TryAdd(kv.Key, kv.Value) == false)
                    throw new Exception("Could not add item to Counters of autoincrement manager, probably duplicate item.");
            }
        }

        public long GetNewCounter(string tableNameLowerCase)//don't use .ToLower() for performance
        {
            lock (this._locker)
            {
                autoincrementcounter thisCounter = null;
                this.Counters.TryGetValue(tableNameLowerCase, out thisCounter);
                if (thisCounter == null) //if doesn't exist in cache
                {
                    thisCounter = InsertNewCounter(tableNameLowerCase);
                }
                else //existing AI in cache
                {
                    if (!AlreadyInInsertedCounters(tableNameLowerCase)
                        && this.UpdatedCounters.ContainsKey(tableNameLowerCase) == false)
                    {
                        if (this.UpdatedCounters.TryAdd(thisCounter.tableName, thisCounter) == false)
                            throw new Exception("Could not add item to updated counters of autoincrement manager, probably duplicate item.");
                    }
                }
                ++thisCounter.value;
                return thisCounter.value;
            }
        }

        bool AlreadyInInsertedCounters(string tableNameLowerCase)
        {
            return this.InsertedCounters.ContainsKey(tableNameLowerCase);
        }
        public long GetCurrentCounterValue(string tableNameLowerCase)
        {
            return this.Counters[tableNameLowerCase].value;
        }
        private autoincrementcounter InsertNewCounter(string tableName)
        {
            autoincrementcounter newCounter = new autoincrementcounter() { tableName = tableName, value = 0 };
            if (this.Counters.TryAdd(tableName, newCounter) == false)
                throw new Exception("Could not insert new item to Counters of autoincrement manager, probably duplicate item.");
            if (this.InsertedCounters.TryAdd(newCounter.tableName, newCounter) == false)
                throw new Exception("Could not insert new item to Counters of autoincrement manager, probably duplicate item.");
            return newCounter;
        }
        public void WriteState()
        {
            lock (this._locker)
            {
                using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(this.Context))
                {
                    StringBuilder sbSql = new StringBuilder();
                    foreach (autoincrementcounter counter in this.InsertedCounters.Values)
                    {
                        sbSql.Append($@" insert into autoincrementcounter 
                                     (tablename,value) values(
                                     {counter.tableName.ToMySqlField()},{counter.value.ToMySqlField()});");
                    }
                    if (!string.IsNullOrEmpty(sbSql.ToString()))
                    {
                        cmd.CommandText = sbSql.ToString();
                        cmd.ExecuteNonQuery();
                    }

                    sbSql = new StringBuilder();
                    foreach (autoincrementcounter counter in this.UpdatedCounters.Values)
                    {
                        sbSql.Append($@" update autoincrementcounter 
                                     set value={counter.value.ToMySqlField()}
                                     where tablename={counter.tableName.EncloseWith("'")};");
                    }
                    if (!string.IsNullOrEmpty(sbSql.ToString()))
                    {
                        cmd.CommandText = sbSql.ToString();
                        cmd.ExecuteNonQuery();
                    }

                }
            }
        }
    }

}
