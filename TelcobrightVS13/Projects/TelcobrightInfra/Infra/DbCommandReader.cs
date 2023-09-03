using System.Collections.Generic;
using System.Data.Common;

namespace TelcobrightInfra
{
    public class DbCommandReader<T>
    {
        private DbCommand Cmd { get; set; }
        public DbCommandReader(DbCommand cmd, string sql)
        {
            this.Cmd = cmd;
        }

        public T getScaler()
        {
            var reader = this.Cmd.ExecuteReader();
            object value = null;
            while (reader.Read())
            {
                value= reader[0];
                break;//read once only
            }
            reader.Close();
            return value != null ? (T)value: default(T);
        }
        public List<T> getSingleColumnList()
        {
            var reader = this.Cmd.ExecuteReader();
            List<T> values= new List<T>();
            while (reader.Read())
            {
                object value=  reader[0];
                values.Add(value != null ? (T)value : default(T));
            }
            reader.Close();
            return values;
        }
    }
}