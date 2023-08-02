using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using LibraryExtensions;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightFileOperations;
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
            if (CollectorInput.Tbc.CdrSetting.FilterDuplicates == true)
            {
                Dictionary<DateTime, List<string>> dayWiseNewTuples= new Dictionary<DateTime, List<string>>();
                foreach (string[] row in decodedCdrRows)
                {
                    CdrSetting cdrSetting = this.CollectorInput.CdrSetting;
                    int timeFieldNo = getTimeFieldNo(cdrSetting,row);
                    DateTime thisDate = row[timeFieldNo].ConvertToDateTimeFromMySqlFormat().Date;
                    List<string> tuplesOfTheDay;//cdrs expressed as tuple like string expressions
                    if (dayWiseNewTuples.TryGetValue(thisDate, out tuplesOfTheDay) == false)
                    {
                        tuplesOfTheDay= new List<string>();
                        dayWiseNewTuples.Add(thisDate,tuplesOfTheDay);
                    }
                    tuplesOfTheDay.Add(decoder.getTupleExpression(CollectorInput,row));
                }
                string sql = dayWiseNewTuples.Select(kv => $"select tuple from uniqueevent where starttime >" +
                                                           string.Join(",", kv.Value));
                List<string[]> uniqueEvents = new List<string[]>();
            }
            NewCdrPreProcessor textCdrCollectionPreProcessor =
                new NewCdrPreProcessor(decodedCdrRows, cdrinconsistents, this.CollectorInput);
            return textCdrCollectionPreProcessor;
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
