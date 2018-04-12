using System;
using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;
namespace LibraryExtensions
{
    public static class MySqlClientExtensions
    {
        public static void ExecuteCommandText(this MySqlCommand cmd, string commandText)
        {
            if (cmd.CommandType != CommandType.Text)
            {
                throw new Exception("Command Type is not set to Text");
            }
            cmd.CommandText = commandText;
            cmd.ExecuteNonQuery();
        }
    }
}
