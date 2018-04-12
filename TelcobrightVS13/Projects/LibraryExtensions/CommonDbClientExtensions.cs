using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using MySql.Data.MySqlClient;
namespace LibraryExtensions
{
    public static class CommonDbClientExtensions
    {
        public static void ExecuteCommandText(this DbCommand cmd, string commandText)
        {
            cmd.CommandText = commandText;
            cmd.ExecuteNonQuery();
        }
    }
}
