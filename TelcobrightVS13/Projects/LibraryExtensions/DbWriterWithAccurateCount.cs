using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class DbWriterWithAccurateCount
    {
        public static string DefaultSpForSingleStatement { get; set; }= "sp_exec_single_statement";
        public static string DefaultSpForMultipleStatements { get; set; } = "sp_exec_multiple_statement";
        public static int ExecSingleStatementThroughStoredProc(DbCommand dbCmd,string command,int expectedRecCount)
        {
            dbCmd.CommandText = DefaultSpForSingleStatement;
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
        public static int ExecMultipleStatementsThroughStoredProc(DbCommand dbCmd, List<string> commands, int expectedRecCount)
        {
            dbCmd.CommandType = CommandType.Text;
            dbCmd.CommandText = $@"delete from temp_sql_statement;";
            dbCmd.ExecuteNonQuery();
            commands = commands.Select(c => c.Replace("\r", "")
                                        .Replace("\n","")
                                        .Replace("\t","")
                                        .Replace("'", @"\'")).ToList();
            dbCmd.CommandText = new StringBuilder("insert into temp_sql_statement(statement) values ")
                                .Append(string.Join(",", commands.Select(c => "('" + c + "')").ToList())).ToString();
            dbCmd.ExecuteNonQuery();

            dbCmd.CommandText = DefaultSpForMultipleStatements;
            dbCmd.CommandType = CommandType.StoredProcedure;

            DbParameter param1 = dbCmd.CreateParameter();
            param1.ParameterName = "@expectedRecCount";
            param1.Value = expectedRecCount;
            dbCmd.Parameters.Add(param1);

            int affectedRecordCount = (int)dbCmd.ExecuteScalar();
            dbCmd.Parameters.Clear();
            dbCmd.CommandType = CommandType.Text;
            return affectedRecordCount;
        }
    }
}
