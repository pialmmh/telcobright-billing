using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediationModel
{
    public static class CdrDataRowToStrArrHelper
    {
        public static string[] ConvertDataReaderToStrArr(object inputData, IDataReader reader)
        {
            Dictionary<int, cdrfieldlist> cdrfieldlists = (Dictionary<int, cdrfieldlist>)inputData;
            string[] txtRow = new string[reader.FieldCount];
            for (int fldNo = 0; fldNo < reader.FieldCount; fldNo++) //for each field
            {
                //findout whether to enclose the field with quote
                cdrfieldlist cdrFieldList = null;
                cdrfieldlists.TryGetValue(fldNo, out cdrFieldList);
                int dateTimeFlag = 0;
                if (cdrFieldList != null)
                {
                    dateTimeFlag = Convert.ToInt32(cdrFieldList.IsDateTime);
                }
                if (dateTimeFlag != 1)
                {
                    txtRow[fldNo] = reader[fldNo].ToString();
                }
                else
                {
                    if (reader[fldNo] != DBNull.Value)
                    {
                        DateTime? tempdate = reader.GetDateTime(fldNo);
                        txtRow[fldNo] = Convert.ToDateTime(tempdate).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        txtRow[fldNo] = "";
                    }
                }
            } //for each field
            return txtRow;
        }
    }
}
