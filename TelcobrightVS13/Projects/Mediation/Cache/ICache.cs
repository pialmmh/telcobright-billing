using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation
{
    public interface ICache
    {
        void WriteAllChanges(DbCommand cmd, int segmentSize);
        int WriteInserts(DbCommand cmd, int segmentSize);
        void WriteUpdates(DbCommand cmd, int segmentSize);
        void WriteDeletes(DbCommand cmd, int segmentSize);
    }
}
