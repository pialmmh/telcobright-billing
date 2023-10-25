using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using System.Reflection;
using System.Threading.Tasks;
using MediationModel;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using Quartz;
using LibraryExtensions;
using QuartzTelcobright;
using TelcobrightInfra;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using System.IO;
using System.Text;

namespace LogPreProcessor
{
    [Export("LogPreprocessor", typeof(EventPreprocessingRule))]
    public class CdrPredecoder : EventPreprocessingRule
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => this.GetType().ToString();
        public string HelpText => "Predecodes cdr rows as telcoright string[] in text file";
        public bool ProcessCollectionOnly { get; set; } = true;
        public bool IsPrepared { get; set; }
        public object RuleConfigData { get; set; }
        private int maxParallelFileSizeForDecode { get; set; }
        public void PrepareRule()
        {
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>) this.RuleConfigData;
            string val=(string) dataAsDic["maxParallelFileForPreDecode"];
            this.maxParallelFileSizeForDecode = Convert.ToInt32(val);
            this.IsPrepared = true;
        }

        public void Execute(Object input)
        {
            if (this.IsPrepared == false)
                throw new Exception(
                    $"Rule {this.RuleName} is not prepared, rule must have initial data in config file, then method prepare()" +
                    $" must be called during mediation, before executing rule.");
            Dictionary<string, object> dataAsDic = (Dictionary<string, object>) input;
            MediationContext mediationContext = (MediationContext) dataAsDic["mediationContext"];
            ne thisSwitch = (ne) dataAsDic["ne"];
            TelcobrightConfig tbc = mediationContext.Tbc;
            List<job> newCdrFileJobs = (List<job>) dataAsDic["newCdrFileJobs"];
            PartnerEntities context = (PartnerEntities) dataAsDic["partnerEntities"];
            TBConsole tbConsole = (TBConsole) dataAsDic["tbConsole"];
            NeAdditionalSetting neAdditionalSetting = (NeAdditionalSetting) dataAsDic["neAdditionalSetting"];
            int maxParallelPreDecoding = neAdditionalSetting.MaxNoOfFilesForParallelPreDecoding;
            CollectionSegmenter<job> segmentedJobs = new CollectionSegmenter<job>(newCdrFileJobs, 0);
            DbCommand cmd = context.Database.Connection.CreateCommand();

            ITelcobrightJob newCdrFileJob = null;
            mediationContext.MefJobContainer.DicExtensionsIdJobWise.TryGetValue(
                newCdrFileJobs.First().idjobdefinition.ToString(), out newCdrFileJob);
            if (newCdrFileJob == null)
                throw new Exception("JobRule not found in MEF collection.");

            segmentedJobs.ExecuteMethodInSegments(maxParallelPreDecoding, segment =>
            {
                var enumerable = segment as job[] ?? segment.ToArray();
                BlockingCollection<job> successfullyPreDecodedJobs= new BlockingCollection<job>();
                Parallel.ForEach(enumerable.AsParallel(), thisJob =>
                {
                    try
                    {
                        preDecodeFiles(thisJob, mediationContext, thisSwitch, tbc, context, tbConsole, newCdrFileJob);
                        successfullyPreDecodedJobs.Add(thisJob);        
                    }
                    catch (Exception e)
                    {
                        tbConsole.WriteLine(e);//just print to console and continue with next job;
                    }
                });
                //can't write to db in parallel, reader busy error occurs
                //no worries about failed jobs, they will be retried again or decoded again in decoder if .predecoded file doesn't exist
                //no need for commit or rollback too.
                foreach (var job in successfullyPreDecodedJobs)
                {
                    try
                    {
                        cmd.CommandText = $" update job set status=2, Error=null where id={job.id}";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }

        private static void preDecodeFiles(job thisJob, MediationContext mediationContext, ne thisSwitch,
            TelcobrightConfig tbc, PartnerEntities context, TBConsole tbConsole, ITelcobrightJob newCdrFileJob)
        {
            tbConsole.WriteLine("Predecoding CdrJob for Switch:" + thisSwitch.SwitchName + ", JobName:" +
                                thisJob.JobName);
            var cdrJobInputData = new CdrJobInputData(mediationContext, context, thisSwitch, thisJob);
            if (thisJob.idjobdefinition != 1)
                throw new Exception(
                    $"Job type must be 1= newCdrFileJob for cdrPredecoding. jobid:{thisJob.id}, jobName:{thisJob.JobName}");

            string predecodedDirName, predecodedFileName;
            getPathNamesForPreDecoding(thisJob, thisSwitch, tbc, out predecodedDirName, out predecodedFileName);
            if (!Directory.Exists(predecodedDirName)) Directory.CreateDirectory(predecodedDirName);

            var preProcessorInput = new Dictionary<string, object>
            {
                { "cdrJobInputData",cdrJobInputData},
                { "preDecodingStageOnly", true}
            };
            NewCdrPreProcessor newCdrPreProcessor =
                (NewCdrPreProcessor)newCdrFileJob.PreprocessJob(preProcessorInput);//EXECUTE preDecoding
            List<string[]> txtRows = newCdrPreProcessor.TxtCdrRows;
            List<string> rowsAsCsvLinesFieldsEnclosedWithBacktick = txtRows.Select(row =>
                string.Join(",",
                    row.Select(field => new StringBuilder("`").Append(field).Append("`").ToString()).ToArray())).ToList();
            File.WriteAllLines(predecodedFileName, rowsAsCsvLinesFieldsEnclosedWithBacktick);
            
        }

        private static void getPathNamesForPreDecoding(job thisJob, ne thisSwitch, TelcobrightConfig tbc, out string predecodedDirName, out string predecodedFileName)
        {
            string fileLocationName = thisSwitch.SourceFileLocations;
            FileLocation fileLocation = tbc.DirectorySettings.FileLocations[fileLocationName];
            string newCdrFileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                                    + Path.DirectorySeparatorChar + thisJob.JobName;
            FileInfo newCdrFileInfo = new FileInfo(newCdrFileName);
            predecodedDirName = newCdrFileInfo.DirectoryName + Path.DirectorySeparatorChar +
                                       "predecoded";
            predecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileInfo.Name +
                                       ".predecoded";
        }
    }
}


