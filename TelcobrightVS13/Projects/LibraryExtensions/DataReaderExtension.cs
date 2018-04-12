using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public static class DataReaderExtension
    {
        public static List<T> ConvertToList<T>(this IDataReader reader,
            Func<IDataRecord, T> instanceGenerator)
        {
            var list = new List<T>();
            while (reader.Read())
                list.Add(instanceGenerator(reader));
            return list;
        }
    }
}
