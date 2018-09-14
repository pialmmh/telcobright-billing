using System;
using System.Collections.Generic;
using System.Data;

namespace MediationModel
{
    public partial class cdr : IDataReaderToStrArrConvertable, IBillingEvent
    {
        public string[] ConvertDataReaderToStrArr(object inputData, IDataReader reader)
        {
            return CdrDataRowToStrArrHelper.ConvertDataReaderToStrArr(inputData, reader);
        }
    }
}