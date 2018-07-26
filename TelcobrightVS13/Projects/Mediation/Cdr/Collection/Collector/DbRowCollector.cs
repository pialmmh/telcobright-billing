using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class DbRowCollector<T> where T : IDataReaderToStrArrConvertable, new()
    {
        private ITelcobrightJobInput TelcobrightJobInput { get; }
        private string SelectSql { get; }

        public DbRowCollector(ITelcobrightJobInput telcobrightJobInput, string selectSql)
        {
            this.TelcobrightJobInput = telcobrightJobInput;
            this.SelectSql = selectSql;
        }

        public virtual List<T> Collect()
        {
            return this.TelcobrightJobInput.Context.Database.SqlQuery<T>(this.SelectSql).ToList();
        }
    }
}
