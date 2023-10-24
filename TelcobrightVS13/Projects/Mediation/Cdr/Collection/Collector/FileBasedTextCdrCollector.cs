using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
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
        public NeAdditionalSetting NeAdditionalSetting { get; } = null;
        public bool PreDecodeAsTextFile { get; }
        private DbCommand DbCmd { get; }
        public FileBasedTextCdrCollector(CdrCollectorInputData collectorInput)
        {
            this.CollectorInput = collectorInput;
            this.DbCmd = ConnectionManager.CreateCommandFromDbContext(this.CollectorInput.Context);
            this.DbCmd.CommandType = CommandType.Text;

            int switchId = this.CollectorInput.Ne.idSwitch;
            NeAdditionalSetting neAdditionalSetting = null;
            if (this.CollectorInput.CdrJobInputData.CdrSetting.NeWiseAdditionalSettings.TryGetValue(switchId,
                    out neAdditionalSetting) == true)
            {
                this.NeAdditionalSetting = neAdditionalSetting;
                this.PreDecodeAsTextFile = neAdditionalSetting != null && neAdditionalSetting.PreDecodeAsTextFile;
                
            }
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
            List<string[]> decodedCdrRows = new List<string[]>();
            if (this.PreDecodeAsTextFile == true)
            {
                string fileLocationName = this.CollectorInput.Ne.SourceFileLocations;
                FileLocation fileLocation = this.CollectorInput.Tbc.DirectorySettings.FileLocations[fileLocationName];
                string newCdrFileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                                        + Path.DirectorySeparatorChar + this.CollectorInput.TelcobrightJob.JobName;
                FileInfo newCdrFileInfo = new FileInfo(newCdrFileName);
                string predecodedDirName = newCdrFileInfo.DirectoryName + Path.DirectorySeparatorChar + "predecoded";
                if (Directory.Exists(predecodedDirName) == false)
                {
                    Directory.CreateDirectory(predecodedDirName);
                }
                string predecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileName +
                                            ".predecoded";
                if (File.Exists(predecodedFileName))//file may not exists occassionally due to clean up or while re-processing of new files
                {
                    decodedCdrRows =
                        FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(predecodedFileName, ',', 0, "`",
                            ";"); //backtick separated
                }
                else
                {
                    decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents); //collect
                }
            }
            else
            {
                decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents); //collect
            }

            NewCdrPreProcessor textCdrCollectionPreProcessor = null;
            if (CollectorInput.Ne.FilterDuplicateCdr == 1 && decodedCdrRows.Count > 0) //filter duplicates
            {
                DayWiseEventCollector<string[]> dayWiseEventCollector= new DayWiseEventCollector<string[]>
                    (uniqueEventsOnly: true, 
                    collectorInput: this.CollectorInput,
                    dbCmd: this.DbCmd,decoder: decoder,decodedEvents: decodedCdrRows,
                    sourceTablePrefix: decoder.PartialTablePrefix);
                dayWiseEventCollector.createNonExistingTables();
                dayWiseEventCollector.collectTupleWiseExistingEvents(decoder);
                DuplicaterEventFilter<string[]> duplicaterEventFilter= new DuplicaterEventFilter<string[]>(dayWiseEventCollector);
                List<string[]> excludedDuplicateCdrs = null;
                Dictionary<string, string[]> finalNonDuplicateEvents = duplicaterEventFilter.filterDuplicateCdrs(out excludedDuplicateCdrs);
                textCdrCollectionPreProcessor =
                    new NewCdrPreProcessor(finalNonDuplicateEvents.Values.ToList(), cdrinconsistents,
                        this.CollectorInput)
                    {
                        FinalNonDuplicateEvents = finalNonDuplicateEvents,
                        DuplicateEvents = excludedDuplicateCdrs
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
