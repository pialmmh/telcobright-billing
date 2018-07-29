using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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
    public abstract class AutoIncrementManagerManual<T> : IAutoIncrementManager<T>
        where T : struct
    {
        protected PartnerEntities Context { get; }
        private DbConnection con { get; }
        private T t { get; }

        public AutoIncrementManagerManual(PartnerEntities context)
        {
            this.Context = context;
            con = this.Context.Database.Connection;
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            this.t = default(T);
        }

        public abstract T GetNewCounter();
        public abstract long GetNewCounter(AutoIncrementCounterType counterType);

        protected virtual T GetCounterFromDb<T>(string tableName)
        {
            Random r = new Random();
            int randomNumber = r.Next();
            DateTime requestedDateTime = DateTime.Now;
            string sql = $" insert into {tableName} (incrementRequestedOn,randomNumber) values( " +
                         $" {requestedDateTime.ToMySqlField()},{randomNumber}" +
                         $");";
            var cmd = this.con.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            //retrieve
            cmd.CommandText = $"select id from {tableName} " +
                              $"where incrementRequestedOn={requestedDateTime.ToMySqlField()} " +
                              $"and randomNumber={randomNumber};";
            T newCounter = (T) cmd.ExecuteScalar();
            return newCounter;
        }

        public void WriteAllChanges()
        {
            //do nothing
        }
    }
}
