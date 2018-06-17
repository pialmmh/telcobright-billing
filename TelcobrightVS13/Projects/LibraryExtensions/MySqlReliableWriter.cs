using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class MySqlReliableWriter
    {
        public static string DefaultStoredProcname { get; set; }= "sp_ddl_writer";
        public static int WriteThroughStoredProc(DbCommand dbCmd,string command,int expectedRecCount)
        {
            dbCmd.CommandText = DefaultStoredProcname;
            dbCmd.CommandType=CommandType.StoredProcedure;
            
            DbParameter param1 = dbCmd.CreateParameter();
            param1.ParameterName = "@command";
            param1.Value = command.EndsWith(";")?command:new StringBuilder(command).Append(";").ToString();
            dbCmd.Parameters.Add(param1);

            DbParameter param2 = dbCmd.CreateParameter();
            param2.ParameterName = "@expectedRecCount";
            param2.Value = expectedRecCount;
            dbCmd.Parameters.Add(param2);

            int affectedRecordCount = (int)dbCmd.ExecuteScalar();
            dbCmd.Parameters.Clear();
            dbCmd.CommandType=CommandType.Text;
            return affectedRecordCount;
        }
    }
}
