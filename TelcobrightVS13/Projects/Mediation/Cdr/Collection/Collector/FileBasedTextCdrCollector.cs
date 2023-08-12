using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightFileOperations;
using TelcobrightInfra;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using TelcobrightMediation.Mediation.Cdr;

namespace TelcobrightMediation
{
    public class FileBasedTextCdrCollector : IEventCollector
    {
        public CdrCollectorInputData CollectorInput { get; set; }
        public Dictionary<string, object> Params { get; set; }
        private DbCommand DbCmd;

        public FileBasedTextCdrCollector(CdrCollectorInputData collectorInput)
        {
            this.CollectorInput = collectorInput;
            this.DbCmd = ConnectionManager.CreateCommandFromDbContext(this.CollectorInput.Context);
            this.DbCmd.CommandType = CommandType.Text;
        }
        public object Collect()
        {
            IFileDecoder decoder = null;
            this.CollectorInput.MefDecodersData.DicExtensions.TryGetValue(this.CollectorInput.Ne.idcdrformat,
                out decoder);
            if (decoder == null)
            {
                throw new Exception("No suitable file decoder found for cdrformat: " +
                                    this.CollectorInput.Ne.idcdrformat
                                    + " and Ne:" + this.CollectorInput.Ne.idSwitch);
            }
            List<cdrinconsistent> cdrinconsistents = new List<cdrinconsistent>();
            List<string[]> decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents); //collect
            Dictionary<string, string[]> decodedEventsAsTupDic = new Dictionary<string, string[]>();
            NewCdrPreProcessor textCdrCollectionPreProcessor = null;
            if (CollectorInput.Ne.FilterDuplicateCdr == 1 && decodedCdrRows.Count > 0) //filter duplicates
            {
                Dictionary<string, string[]> finalNonDuplicateEvents =
                    filterDuplicateCdrs(decoder, decodedCdrRows, decodedEventsAsTupDic);
                textCdrCollectionPreProcessor =
                    new NewCdrPreProcessor(finalNonDuplicateEvents.Values.ToList(), cdrinconsistents, this.CollectorInput);
                textCdrCollectionPreProcessor.FinalNonDuplicateEvents = finalNonDuplicateEvents;
            }
            else//duplicate check not required
            {
                textCdrCollectionPreProcessor =
                    new NewCdrPreProcessor(decodedCdrRows, cdrinconsistents, this.CollectorInput);
            }
            if (textCdrCollectionPreProcessor == null)
            {
                throw new Exception("textCdrCollectionPreProcessor cannot be null");
            }
            return textCdrCollectionPreProcessor;
        }

        private Dictionary<string, string[]> filterDuplicateCdrs(IFileDecoder decoder, List<string[]> decodedCdrRows,
            Dictionary<string, string[]> decodedEventsAsTupDic)
        {
            Dictionary<DateTime, List<string>> dayWiseNewTuples = new Dictionary<DateTime, List<string>>();
            CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
            Dictionary<string, string[]> finalNonDuplicateEvents = new Dictionary<string, string[]>();
            foreach (string[] row in decodedCdrRows)
            {
                int timeFieldNo = getTimeFieldNo(cdrSetting, row);
                DateTime thisDate = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat().Date;
                List<string> tuplesOfTheDay; //cdrs expressed as tuple like string expressions
                if (dayWiseNewTuples.TryGetValue(thisDate, out tuplesOfTheDay) == false)
                {
                    tuplesOfTheDay = new List<string>();
                    dayWiseNewTuples.Add(thisDate, tuplesOfTheDay);
                }
                string tupleExpressionForRow = decoder.getTupleExpression(CollectorInput, row);
                decodedEventsAsTupDic.Add(tupleExpressionForRow, row);
                //**temp code mustafa
                if (tupleExpressionForRow == "10/2023-06-12 12:22:10/1686562286")
                {
                    Console.WriteLine("duplicate event found");
                }
                //
                tuplesOfTheDay.Add(tupleExpressionForRow);
            }
            Func<DateTime, List<string>, string> getSqlPerDay
                = (day, tuples) => $" select tuple from uniqueevent where tuple " +
                                   $" in ({string.Join(",", tuples.Select(t => new StringBuilder("'").Append(t).Append("'")))}) " +
                                   $" and {decoder.getSqlWhereClauseForDayWiseSafeCollection(this.CollectorInput, day)}";

            string sql = string.Join(" union all ", dayWiseNewTuples.Select(kv => getSqlPerDay(kv.Key, kv.Value)));
            List<string> newEvents = dayWiseNewTuples.SelectMany(kv => kv.Value).ToList();
            List<string> existingEvents = new List<string>();
            this.DbCmd.CommandText = sql;
            this.DbCmd.CommandType = CommandType.Text;
            DbDataReader reader = this.DbCmd.ExecuteReader();
            while (reader.Read())
            {
                existingEvents.Add(reader[0].ToString());
            }
            reader.Close();

            //temp code fetch all unique values
            List<string> allUniqueEvents = new List<string>();
            this.DbCmd.CommandText = "select tuple from uniqueevent;";
            this.DbCmd.CommandType = CommandType.Text;
            reader = this.DbCmd.ExecuteReader();
            while (reader.Read())
            {
                allUniqueEvents.Add(reader[0].ToString());
            }
            reader.Close();


            //end temp code

            Dictionary<string, string> alreadyConsideredEvents = existingEvents.ToDictionary(e => e);
            foreach (var kv in decodedEventsAsTupDic)
            {
                string tuple = kv.Key;
                string[] decodedRow = kv.Value;
                //temp code
                if (allUniqueEvents.Contains(tuple))
                {
                    if (alreadyConsideredEvents.ContainsKey(tuple) == false)
                    {
                        Console.WriteLine("Unique event fetching probably incorrect.");
                    }
                }

                //end temp doe
                if (alreadyConsideredEvents.ContainsKey(tuple) == false)
                {
                    finalNonDuplicateEvents.Add(tuple, decodedRow);
                    alreadyConsideredEvents.Add(tuple, tuple); //it's just used like hashmap
                }
                else
                {
                    //cdr is skipped
                    Console.WriteLine("Skipped duplicate cdrs");
                }
            }

            ////tempCode
            //this.DbCmd.CommandText = "insert into uniqueevent(tuple,starttime) values "
            //                         + string.Join(",", finalNonDuplicateEvents.Select(kv =>
            //                         {
            //                             string tuple = kv.Key;
            //                             string[] row = kv.Value;
            //                             return "('" + tuple + "','" + row[Fn.StartTime] + "')";
            //                         }));
            //this.DbCmd.CommandType = CommandType.Text;
            //if (finalNonDuplicateEvents.Any())
            //{
            //    //this.DbCmd.ExecuteNonQuery();
            //}
            ////end 
            return finalNonDuplicateEvents;
        }

        private static int getTimeFieldNo(CdrSetting cdrSettings, string[] row)
        {
            int timeFieldNo = -1;
            switch (cdrSettings.SummaryTimeField)
            {
                case SummaryTimeFieldEnum.StartTime:
                    timeFieldNo = Fn.StartTime;
                    break;
                case SummaryTimeFieldEnum.AnswerTime:
                    timeFieldNo = Fn.AnswerTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return timeFieldNo;
        }

    }
}
