using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
using TelcobrightMediation.Config;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class DbRowCollector<T> where T:IDataReaderToStrArrConvertable,new() 
    {
        private CdrJobInputData CdrJobInputData { get; }
        private string SelectSql { get; }
        public DbRowCollector(CdrJobInputData cdrJobInputData, string selectSql)
        {
            this.CdrJobInputData = cdrJobInputData;
            this.SelectSql = selectSql;
        }

        public virtual List<T> Collect()
        {
            return this.CdrJobInputData.Context.Database.SqlQuery<T>(this.SelectSql).ToList();
        }

        public virtual List<string[]> CollectAsTxtRows()
        {
            List<string[]> txtRows=new List<string[]>();
            using (DbCommand command = ConnectionManager.CreateCommandFromDbContext(this.CdrJobInputData.Context))
            {
                command.CommandText = this.SelectSql;
                var reader = command.ExecuteReader();
                T dummyDataObjectToCallExtensionMethod= new T();
                while (reader.Read())
                {
                    Dictionary<int, cdrfieldlist> cdrfieldlists = this.CdrJobInputData.MediationContext.CdrFieldLists;
                    string[] convertedRow = dummyDataObjectToCallExtensionMethod.ConvertDataReaderToStrArr(cdrfieldlists,reader);
                    txtRows.Add(convertedRow);
                }
                reader.Close();
            }
            return txtRows;
        }
    }
}
