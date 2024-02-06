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
        public bool CollectFromPreDecodedFile { get; }
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
                this.CollectFromPreDecodedFile = neAdditionalSetting != null && neAdditionalSetting.PreDecodeAsTextFile;

            }
        }
        public object Collect()
        {
            AbstractCdrDecoder decoder = getDecoder();
            List<cdrinconsistent> cdrinconsistents = new List<cdrinconsistent>();
            List<string[]> decodedCdrRows = new List<string[]>();
            if (this.CollectFromPreDecodedFile == false)//regular decoding
            {
                decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents);
                decodedCdrRows = decodedCdrRows
                    .Where(r => r[Fn.AnswerTime].ConvertToDateTimeFromMySqlFormat() >= this.CollectorInput.CdrSetting.ExcludeBefore
                                || (r[Fn.StartTime].IsNullOrEmptyOrWhiteSpace() && r[Fn.StartTime].ConvertToDateTimeFromMySqlFormat() >= this.CollectorInput.CdrSetting.ExcludeBefore))
                    .ToList();
            }
            else//collect from pre-decoded, but fallback to decode if predecoded file doesn't exist
            {
                string predecodedFileName = getPredecodedFileName();
                if (File.Exists(predecodedFileName)) //file may not exists occassionally due to clean up or while re-processing of new files
                {
                    decodedCdrRows =
                        FileUtil.ParseCsvWithEnclosedAndUnenclosedFields(predecodedFileName, ',', 0, "`", ";"); //backtick separated
                    decodedCdrRows = decodedCdrRows
                        .Where(r => (!r[Fn.ConnectTime].IsNullOrEmptyOrWhiteSpace() && r[Fn.ConnectTime].ConvertToDateTimeFromMySqlFormat() >= this.CollectorInput.CdrSetting.ExcludeBefore)
                                    || (!r[Fn.StartTime].IsNullOrEmptyOrWhiteSpace() && r[Fn.StartTime].ConvertToDateTimeFromMySqlFormat() >= this.CollectorInput.CdrSetting.ExcludeBefore))
                        .ToList();
                }
                else
                {
                    decodedCdrRows = decoder.DecodeFile(this.CollectorInput, out cdrinconsistents); //collect
                    decodedCdrRows = decodedCdrRows
                        .Where(r => (!r[Fn.ConnectTime].IsNullOrEmptyOrWhiteSpace() && r[Fn.ConnectTime].ConvertToDateTimeFromMySqlFormat() >= this.CollectorInput.CdrSetting.ExcludeBefore)
                                    || (!r[Fn.StartTime].IsNullOrEmptyOrWhiteSpace() && r[Fn.StartTime].ConvertToDateTimeFromMySqlFormat() >= this.CollectorInput.CdrSetting.ExcludeBefore))
                        .ToList();
                }
            }
            var newCdrPreProcessor = new NewCdrPreProcessor(decodedCdrRows, cdrinconsistents, this.CollectorInput);
            newCdrPreProcessor.Decoder = decoder;
            return newCdrPreProcessor;
        }

        public AbstractCdrDecoder getDecoder()
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

            return decoder;
        }

        private string getPredecodedFileName()
        {
            string fileLocationName = this.CollectorInput.Ne.SourceFileLocations;
            FileLocation fileLocation = this.CollectorInput.Tbc.DirectorySettings.FileLocations[fileLocationName];
            string newCdrFileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                                    + Path.DirectorySeparatorChar + this.CollectorInput.TelcobrightJob.JobName;
            FileInfo newCdrFileInfo = new FileInfo(newCdrFileName);
            string predecodedDirName = newCdrFileInfo.DirectoryName + Path.DirectorySeparatorChar + "predecoded";
            string predecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileInfo.Name +
                                        ".predecoded";
            return predecodedFileName;
        }


    }
}
