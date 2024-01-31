using System.Collections.Generic;
using System.Data.Common;
using MediationModel;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class CdrRowCollector<T> :DbRowCollector<T> where T : IDataReaderToStrArrConvertable, new()
    {
        private ITelcobrightJobInput TelcobrightJobInput { get; }
        private string SelectSql { get; }
        private CdrJobInputData CdrJobInputData { get; }
        public CdrRowCollector(ITelcobrightJobInput telcobrightJobInput, string selectSql)
            :base(telcobrightJobInput,selectSql)
        {
            this.TelcobrightJobInput = telcobrightJobInput;
            this.CdrJobInputData = (CdrJobInputData) this.TelcobrightJobInput;
            this.SelectSql = selectSql;
        }
        public virtual List<string[]> CollectAsTxtRows()
        {
            List<string[]> txtRows = new List<string[]>();
            using (DbCommand command = ConnectionManager.CreateCommandFromDbContext(this.CdrJobInputData.Context))
            {
                command.CommandText = this.SelectSql;
                var reader = command.ExecuteReader();
                T dummyDataObjectToCallExtensionMethod = new T();
                while (reader.Read())
                {
                    Dictionary<int, cdrfieldlist> cdrfieldlists = this.CdrJobInputData.MediationContext.CdrFieldLists;
                    string[] convertedRow = dummyDataObjectToCallExtensionMethod.ConvertDataReaderToStrArr(cdrfieldlists, reader);
                    txtRows.Add(convertedRow);
                }
                reader.Close();
            }
            return txtRows;
        }
        public object CollectAsTxtRows(DbCommand cmd)
        {
            List<string[]> txtRows = new List<string[]>();
            cmd.CommandText = this.SelectSql;
            var reader = cmd.ExecuteReader();
            T dummyDataObjectToCallExtensionMethod = new T();
            while (reader.Read())
            {
                Dictionary<int, cdrfieldlist> cdrfieldlists = this.CdrJobInputData.MediationContext.CdrFieldLists;
                string[] convertedRow =
                    dummyDataObjectToCallExtensionMethod.ConvertDataReaderToStrArr(cdrfieldlists, reader);
                txtRows.Add(convertedRow);
            }
            reader.Close();
            return txtRows;
        }
    }
}