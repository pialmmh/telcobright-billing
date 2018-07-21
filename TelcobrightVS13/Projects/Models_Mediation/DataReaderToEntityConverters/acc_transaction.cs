using System;
using System.Collections.Generic;
using System.Data;

namespace MediationModel
{
    public partial class acc_transaction : IDataReaderToStrArrConvertable
    {
        public string[] ConvertDataReaderToStrArr(object inputData, IDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}