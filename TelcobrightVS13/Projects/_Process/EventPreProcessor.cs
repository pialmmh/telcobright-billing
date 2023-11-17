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
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;

namespace Process
{
    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class EventPreProcessor : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Pre Process log file pre-processing rules";
        public override int ProcessId => 120;
        public override void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            CdrSetting cdrSetting = tbc.CdrSetting;
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);
            try
            {
                context.Database.Connection.Open();
                var mediationContext = new MediationContext(tbc, context,true);
                tbc.GetPathIndependentApplicationDirectory();
                foreach (ne ne in mediationContext.Nes.Values)
                {
                    try
                    {
                        NeAdditionalSetting neAdditionalSetting = null;
                        if (cdrSetting.NeWiseAdditionalSettings.TryGetValue(ne.idSwitch, out neAdditionalSetting) ==
                            false)
                            continue;
                        List<EventPreprocessingRule> eventPreprocessingRules =
                            neAdditionalSetting.EventPreprocessingRules;
                        if (eventPreprocessingRules.Any() == false) continue;
                        eventPreprocessingRules.ForEach(rule => rule.PrepareRule());

                        if (ne.SkipCdrDecoded == 1 || CheckIncompleteExists(context, mediationContext, ne) == false)
                            continue;
                        int maxNumberOfFilesToPreDecode = neAdditionalSetting.MaxNumberOfFilesInPreDecodedDirectory;

                        List<job> incompleteJobs = GetNewCdrJobs(tbc, context, ne, maxNumberOfFilesToPreDecode);
                        if (incompleteJobs.Any() == false) continue;

                        foreach (var eventPreprocessingRule in eventPreprocessingRules)
                        {
                            if (eventPreprocessingRule.ProcessCollectionOnly == true)
                            {
                                var ruleInputData = new Dictionary<string, object>()
                                {
                                    {"mediationContext", mediationContext},
                                    {"ne", ne},
                                    {"newCdrFileJobs", incompleteJobs},
                                    {"partnerEntities", context},
                                    {"neAdditionalSetting", neAdditionalSetting}
                                };
                                eventPreprocessingRule.Execute(ruleInputData);
                            }
                            else //single job logic not implemented yet
                            {
                                throw new NotImplementedException();
                            }
                        }
                    } //try for each NE
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1);
                        ErrorWriter.WriteError(e1, "EventPreProcessor", null, "NE:" + ne.idSwitch,
                            tbc.Telcobrightpartner.CustomerName, context);
                        continue; //with next switch
                    }
                } //for each NE
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter.WriteError(e1, "EventPreProcessor", null, "", operatorName, context);
            }
        }

        bool CheckIncompleteExists(PartnerEntities context, MediationContext mediationContext, ne ne)
        {
            var anyJob = context.jobs.Where(job => job.CompletionTime == null && job.idjobdefinition == 1
                                          && job.idNE == ne.idSwitch).ToList().FirstOrDefault();
            return anyJob != null;
        }

        private static void PrintErrorMessageToConsole(ne ne, job telcobrightJob, Exception e)
        {
            Console.WriteLine("xxxErrorxxx Processing CdrJob for Switch:" +
                              ne.SwitchName + ", JobName:" + telcobrightJob.JobName);
            Console.WriteLine(e.Message);
        }
        public MefJobContainer ComposeMefJobData()
        {
            MefJobContainer mefJobData = new MefJobContainer();
            mefJobData.CmpJob.Compose();
            foreach (ITelcobrightJob ext in mefJobData.CmpJob.Jobs)
            {
                mefJobData.DicExtensions.Add(ext.Id.ToString(), ext);
            }
            return mefJobData;
        }

        public MefDecoderContainer ComposeMefDecoders(PartnerEntities context)
        {
            MefDecoderContainer mefDecoders = new MefDecoderContainer(context);
            mefDecoders.CmpDecoder.Compose();
            foreach (var ext in mefDecoders.CmpDecoder.Decoders)
                mefDecoders.DicExtensions.Add(ext.Id, ext);
            return mefDecoders;
        }

        public List<job> GetNewCdrJobs(TelcobrightConfig tbc, PartnerEntities contextTb, ne thisSwitch, int maxNumberOfFilesToPreDecode)
        {
            List<job> jobs = null;
            if (tbc.CdrSetting.DescendingOrderWhileProcessingListedFiles == true)
            {
                jobs = contextTb.jobs
                    .Where(c => c.CompletionTime == null
                                && c.idNE == thisSwitch.idSwitch
                                && c.Status == 7 && c.idjobdefinition == 1
                                && c.JobState != "paused") //downloaded & new cdr
                    .Include(c => c.ne.enumcdrformat)
                    .Include(c => c.ne.telcobrightpartner)
                    .OrderByDescending(c => c.JobName)
                    .Take(Convert.ToInt32(maxNumberOfFilesToPreDecode)).ToList();
            }
            else
            {
                jobs = contextTb.jobs
                    .Where(c => c.CompletionTime == null
                                && c.idNE == thisSwitch.idSwitch
                                && c.Status == 7 && c.idjobdefinition == 1
                                && c.JobState != "paused") //downloaded & new cdr
                    .Include(c => c.ne.enumcdrformat)
                    .Include(c => c.ne.telcobrightpartner)
                    .OrderBy(c => c.JobName)
                    .Take(Convert.ToInt32(maxNumberOfFilesToPreDecode)).ToList();
            }
            return jobs;
        }
    }





}


