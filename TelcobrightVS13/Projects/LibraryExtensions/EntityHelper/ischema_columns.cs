using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions.EntityHelper
{
    public class ischema_columns
    {
        public string TableNameInDb { get; set; }
        public string ColumnName { get; set; }
        public int OrdinalPosition { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
    }
}
