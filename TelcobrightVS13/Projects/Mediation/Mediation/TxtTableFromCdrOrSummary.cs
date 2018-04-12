using System;
using System.Collections.Generic;
using System.Text;
using MediationModel;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using TelcobrightMediation.Config;

namespace TelcobrightMediation
{
    public class TxtTableFromCdrOrSummary
    {

        public int TxtTableFromCdrorSummary(string sourceTable, string database, int idSwitch,
          long? fileSerialStart, long? fileSerialEnd, DateTime? startingPeriod, DateTime? endingPeriod,
            List<string[]> txtTable, PartnerEntities context, List<long> LstIdCall,
            Dictionary<int, cdrfieldlist> dicCdrFieldList, string processName,
            string uniqueBillId)
        {
            string sqlTxtTable = " select * from " + sourceTable;
            if (startingPeriod != null)
            {
                //make sure both parameters are set if one of these is mentioned
                if (endingPeriod == null || endingPeriod < startingPeriod)
                {
                    //Console.WriteLine("Both Starting Period and Ending Period must be mentioned and End Period must be greater than starting period!");
                    allerror thisError = new allerror();
                    thisError.idError = 102; //arbitrary
                    thisError.TimeRaised = DateTime.Now;
                    thisError.Status = 1;

                    thisError.ExceptionMessage =
                        @"Both Starting Period and Ending Period must be mentioned and End Period must be greater than starting period!";

                    thisError.ProcessName = processName;
                    context.allerrors.Add(thisError);

                    context.SaveChanges();
                    return 0;
                }

                if (uniqueBillId != null)
                {
                    //partial cdr old instance mode
                    sqlTxtTable += " where starttime='" +
                                   Convert.ToDateTime(startingPeriod).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                else
                {
                    sqlTxtTable += " where starttime>='" + Convert.ToDateTime(startingPeriod)
                                       .ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                                   " and starttime<='" + Convert.ToDateTime(endingPeriod)
                                       .ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
            }

            sqlTxtTable += " and switchid=" + idSwitch;

            if (fileSerialStart != null)
            {
                if (fileSerialStart == null || fileSerialEnd < fileSerialStart)
                {
                    //Console.WriteLine("Both FileSerialStart and FileSerialEnd must be mentioned and FileSerialEnd must be greater than FileSerialStart!");
                    allerror thisError = new allerror();
                    thisError.idError = 102; //arbitrary
                    thisError.TimeRaised = DateTime.Now;
                    thisError.Status = 1;

                    thisError.ExceptionMessage =
                        @"Both FileSerialStart and FileSerialEnd must be mentioned and FileSerialEnd must be greater than FileSerialStart!";

                    thisError.ProcessName = processName;
                    context.allerrors.Add(thisError);
                    context.SaveChanges();
                    return 0;
                }
                sqlTxtTable += " and fileserialno>=" + fileSerialStart +
                               " and fileserialno<= " + fileSerialEnd;
            }

            if (uniqueBillId == null && LstIdCall != null && LstIdCall.Count > 0)
            {
                sqlTxtTable += " and idcall in (" + string.Join(",", LstIdCall.ToArray()) + ") ;";
            }

            if (uniqueBillId != null
            ) //loading old instances of partial cdr mode, make sure that the final instance is loaded at last
            {
                //SqlTxtTable += " and UniqueBillId='" + UniqueBillId + "'";
                //load idcalls for this uniquebillid and switch
                StringBuilder whereIn = new StringBuilder();
                whereIn.Append(" and idcall in (");
                //using (MySqlConnection con = new MySqlConnection(ConStrPartner))

                //con.Open();
                using (DbCommand command = ConnectionManager.CreateCommandFromDbContext(context))
                {
                    command.CommandText =
                        " select idcall from cdrpartial where uniquebillid='" + uniqueBillId + "' and idswitch=" +
                        idSwitch +
                        " and starttime='" + Convert.ToDateTime(startingPeriod).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    DbDataReader myReader = command.ExecuteReader();
                    List<string> lstIdCall = new List<string>();
                    while (myReader.Read())
                    {
                        long idCall = -1;
                        long.TryParse(myReader[0].ToString(), out idCall);
                        if (idCall == -1)
                        {
                            allerror thisError = new allerror();
                            thisError.idError = 102; //arbitrary
                            thisError.TimeRaised = DateTime.Now;
                            thisError.Status = 1;

                            thisError.ExceptionMessage =
                                @"Could not parse idcall during partial cdr listing for unique bill id=" +
                                uniqueBillId + " and switchid=" + idSwitch;

                            thisError.ProcessName = processName;
                            context.allerrors.Add(thisError);
                            context.SaveChanges();
                            return 0;
                        }
                        else
                        {
                            lstIdCall.Add(idCall.ToString());
                        }
                    } //while reader...
                    myReader.Close();
                    myReader = null;
                    //idcalls are loaded
                    if (lstIdCall.Count > 0)
                    {
                        sqlTxtTable += whereIn.Append(string.Join(",", lstIdCall.ToArray()))
                            .Append(" ) order by partialanswertime, -PartialNextIdCall desc ");
                    }
                    else
                    {
                        return 1; // old instances can't be found as no idcall found from cdrpartial...
                    }
                }
                
                
            } //if uniquebillid


            bool cdrExists = false;
            //create TxtTable from Cdrs in database
            //using (MySqlConnection con = new MySqlConnection(ConStrPartner))
            {
                //con.Open();
                using (DbCommand command = ConnectionManager.CreateCommandFromDbContext(context))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = " select exists( " + sqlTxtTable + ") as existence ";
                    DbDataReader myReader;
                    myReader = command.ExecuteReader();
                    while (myReader.Read())
                    {
                        int existence = int.Parse(myReader[0].ToString());
                        if (existence == 1)
                        {
                            cdrExists = true;
                        }
                        else
                        {
                            cdrExists = false;
                            break;
                        }
                    }
                    myReader.Close();

                    if (cdrExists == false)
                    {
                        return 1; // no error but just no record...
                    }

                    //now fetch cdrs and populate txttable
                    command.CommandText = sqlTxtTable;
                    myReader = command.ExecuteReader();
                    int columnCount = myReader.FieldCount;
                    while (myReader.Read())
                    {
                        int fldCount = 0;
                        string[] thisRow = null;
                        thisRow = new string[columnCount];
                        for (fldCount = 0; fldCount < columnCount; fldCount++) //for each field
                        {
                            //findout whether to enclose the field with quote
                            cdrfieldlist dateField = null;
                            dicCdrFieldList.TryGetValue(fldCount, out dateField);
                            int dateTimeFlag = 0;
                            if (dateField != null)
                            {
                                dateTimeFlag = Convert.ToInt32(dateField.IsDateTime);
                            }
                            if (dateTimeFlag != 1)
                            {
                                thisRow[fldCount] = myReader[fldCount].ToString();
                            }
                            else
                            {
                                //datetime field
                                //while loading summary data, endtime was '0000-00-00' like that.
                                //this kept on throwing exception while reading using reader
                                //end time is not required in summary cache, so set field15=""
                                if (sourceTable == "cdrsummary" && fldCount == 15)
                                {
                                    thisRow[fldCount] = "";
                                }
                                else
                                {
                                    if (myReader[fldCount] != DBNull.Value)
                                    {
                                        DateTime? tempdate = myReader.GetDateTime(fldCount);
                                        thisRow[fldCount] =
                                            Convert.ToDateTime(tempdate).ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                    else
                                    {
                                        thisRow[fldCount] = "";
                                    }
                                }
                            }
                        } //for each field
                        txtTable.Add(thisRow);
                    }
                    myReader.Close();
                } //using mysql command
            } //using mysql connection

            return 1; //success
        }
    }
}
