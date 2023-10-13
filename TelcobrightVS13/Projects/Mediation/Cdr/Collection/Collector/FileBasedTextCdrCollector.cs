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
using TelcobrightInfra;
using TelcobrightMediation.Cdr.Collection.PreProcessors;

namespace TelcobrightMediation
{
    public class FileBasedTextCdrCollector : IEventCollector
    {
        public CdrCollectorInputData CollectorInput { get; }
        public Dictionary<string, object> Params { get; set; }
        private DbCommand DbCmd { get; }

        public FileBasedTextCdrCollector(CdrCollectorInputData collectorInput)
        {
            this.CollectorInput = collectorInput;
            this.DbCmd = ConnectionManager.CreateCommandFromDbContext(this.CollectorInput.Context);
            this.DbCmd.CommandType = CommandType.Text;
        }
        public object Collect()
        {
            AbstractCdrDecoder decoder = null;
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
            //Dictionary<string, string[]> decodedEventsAsTupDic = new Dictionary<string, string[]>();
            NewCdrPreProcessor textCdrCollectionPreProcessor = null;
            if (CollectorInput.Ne.FilterDuplicateCdr == 1 && decodedCdrRows.Count > 0) //filter duplicates
            {
                DayWiseEventCollector<string[]> dayWiseEventCollector= new DayWiseEventCollector<string[]>(this.CollectorInput,this.DbCmd,decoder,decodedCdrRows,
                    decoder.PartialTablePrefix);
                dayWiseEventCollector.createNonExistingTables();
                dayWiseEventCollector.collectExistingEvents(decoder);
                DuplicaterEventFilter<string[]> duplicaterEventFilter= new DuplicaterEventFilter<string[]>(dayWiseEventCollector);
                Dictionary<string, string[]> finalNonDuplicateEvents =
                    duplicaterEventFilter.filterDuplicateCdrs();
                textCdrCollectionPreProcessor =
                    new NewCdrPreProcessor(finalNonDuplicateEvents.Values.ToList(), cdrinconsistents,
                        this.CollectorInput)
                    {
                        FinalNonDuplicateEvents = finalNonDuplicateEvents
                    };
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
    }
}
