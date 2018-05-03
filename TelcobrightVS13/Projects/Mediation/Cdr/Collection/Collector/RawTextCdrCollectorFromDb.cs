using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using MediationModel;
using TelcobrightMediation.Cdr;
using MySql.Data.MySqlClient;

namespace TelcobrightMediation
{
    public class RawTextCdrCollectorFromDb : IEventCollector
    {
        public CdrCollectorInputData CollectorInput { get; set; }
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string FileName { get; set; }
        public PartnerEntities Context { get; }
        public Dictionary<string,string> Params { get; set; }

        public RawTextCdrCollectorFromDb(string databaseName,
            string tableName, PartnerEntities context)
        {
            this.DatabaseName = databaseName;
            this.TableName = tableName;
            this.Context = context;
        }


        public object Collect()
        {
            List<string[]> rows = new List<string[]>();
            var connection = this.Context.Database.Connection;
            if (connection.State!=ConnectionState.Open)
                connection.Open();
            using (DbCommand cmd = this.Context.Database.Connection.CreateCommand())
            {
                cmd.CommandText = "select * from " + this.DatabaseName + "." + this.TableName + " where filename='" +
                                  this.Params["fileName"] + "'";
                DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    List<string> row = new List<string>();
                    for (int i = 0; i < reader.FieldCount - 1; i++) //skip filename in the last column
                    {
                        row.Add(reader[i].ToString());
                    }
                    rows.Add(row.ToArray());
                }
                reader.Close();
            }
            NewCdrPreProcessor textCdrCollectionPreProcessor =
                new NewCdrPreProcessor(rows, new List<cdrinconsistent>(), this.CollectorInput);
            return textCdrCollectionPreProcessor;
        }

    }
}