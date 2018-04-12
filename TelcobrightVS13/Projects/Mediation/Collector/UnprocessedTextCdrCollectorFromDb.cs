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
    public class RawCdrCollectorFromDb
    {
        public CdrCollectorInputData CollectorInput { get; protected set; }
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string FileName { get; set; }
        public PartnerEntities Context { get; }

        public RawCdrCollectorFromDb(CdrCollectorInputData collectorInput, string databaseName, 
            string tableName, string fileName,PartnerEntities context)
        {
            this.CollectorInput = collectorInput;
            this.DatabaseName = databaseName;
            this.TableName = tableName;
            this.FileName = fileName;
            this.Context =context;
        }


        public List<string[]> CollectRawCdrsFromDb()
        {
            List<string[]> rows = new List<string[]>();
            using (DbCommand cmd = this.Context.Database.Connection.CreateCommand())
            {
                cmd.CommandText = "select * from " + this.DatabaseName + "." + this.TableName + " where filename='" +
                                  this.FileName + "'";
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
            return rows;
        }
    }
}