using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class MySqlReliableWriter
    {
        public static int WriteThroughStoredProc(DbCommand dbCmd,string command,int expectedRecCount)
        {
            DbParameter param1 = dbCmd.CreateParameter();
            param1.ParameterName = "@command";
            param1.Value = command;
            dbCmd.Parameters.Add(param1);

            DbParameter param2 = dbCmd.CreateParameter();
            param2.ParameterName = "@expectedRecCount";
            param2.Value = expectedRecCount;
            dbCmd.Parameters.Add(param2);
            int affectedRecordCount = (int)dbCmd.ExecuteScalar();
            return affectedRecordCount;
        }
    }
}
