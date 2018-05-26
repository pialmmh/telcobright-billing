using System;
using System.Collections.Generic;
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

        public static List<T> ConvertToList<T>(this IDataReader reader,
            Func<IDataRecord, T> instanceGenerator)
        {
            var list = new List<T>();
            while (reader.Read())
                list.Add(instanceGenerator(reader));
            reader.Close();
            return list;
        }
        public static List<T> GetObjectsByQuery<T>(this DbCommand cmd, Func<IDataRecord, T> instanceGenerator)
        {
            IDataReader reader = cmd.ExecuteReader();
            return reader.ConvertToList(instanceGenerator);
        }
    }
}
