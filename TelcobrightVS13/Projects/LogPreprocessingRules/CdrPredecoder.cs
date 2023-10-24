using TelcobrightMediation;
using MySql.Data.MySqlClient;
using System;
using System.CodeDom;
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
            this.maxParallelFileSizeForDecode = (int) dataAsDic["maxParallelFileForPreDecode"];
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
            CollectionSegmenter<job> segmentedJobs = new CollectionSegmenter<job>(newCdrFileJobs, 0);
            DbCommand cmd = context.Database.Connection.CreateCommand();

            ITelcobrightJob newCdrFileJob = null;
            mediationContext.MefJobContainer.DicExtensionsIdJobWise.TryGetValue(
                newCdrFileJobs.First().idjobdefinition.ToString(), out newCdrFileJob);
            if (newCdrFileJob == null)
                throw new Exception("JobRule not found in MEF collection.");

            segmentedJobs.ExecuteMethodInSegments(10, segment =>
            {
                Parallel.ForEach(segment, thisJob =>
                {
                    try
                    {
                        preDecodeFiles(thisJob, mediationContext, thisSwitch, tbc, context, tbConsole, cmd,
                            newCdrFileJob);
                    }
                    catch (Exception e)
                    {
                        tbConsole.WriteLine(e);//just print to console and continue with next job;
                    }
                });
            });
        }

        private static void preDecodeFiles(job thisJob, MediationContext mediationContext, ne thisSwitch,
            TelcobrightConfig tbc, PartnerEntities context, TBConsole tbConsole, DbCommand cmd,
            ITelcobrightJob newCdrFileJob)
        {
            tbConsole.WriteLine("Predecoding CdrJob for Switch:" + thisSwitch.SwitchName + ", JobName:" +
                                thisJob.JobName);
            var cdrJobInputData =
                new CdrJobInputData(mediationContext, context, thisSwitch, thisJob);
            if (thisJob.idjobdefinition != 1)
            {
                throw new Exception(
                    $"Job type must be 1= newCdrFileJob for cdrPredecoding. jobid:{thisJob.id}, jobName:{thisJob.JobName}");
            }

            string fileLocationName = thisSwitch.SourceFileLocations;
            FileLocation fileLocation = tbc.DirectorySettings.FileLocations[fileLocationName];
            string newCdrFileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                                    + Path.DirectorySeparatorChar + thisJob.JobName;
            FileInfo newCdrFileInfo = new FileInfo(newCdrFileName);
            string predecodedDirName = newCdrFileInfo.DirectoryName + Path.DirectorySeparatorChar +
                                       "predecoded";
            if (Directory.Exists(predecodedDirName) == false)
            {
                Directory.CreateDirectory(predecodedDirName);
            }
            string predecodedFileName = predecodedDirName + Path.DirectorySeparatorChar + newCdrFileName +
                                        ".predecoded";
            List<string[]> txtRows = (List<string[]>) newCdrFileJob.PreprocessJob(cdrJobInputData); //EXECUTE
            List<string> rowsAsCsvLinesWithDoubleQuotedFields = txtRows.Select(row =>
                string.Join(",",
                    row.Select(field => new StringBuilder("").Append(field).Append("").ToString()).ToArray())).ToList();
            File.WriteAllLines(predecodedFileName, rowsAsCsvLinesWithDoubleQuotedFields);
            cmd.CommandText = $" update job set status=2, Error=null where id={thisJob.id}";
            cmd.ExecuteNonQuery();
        }
    }
}


