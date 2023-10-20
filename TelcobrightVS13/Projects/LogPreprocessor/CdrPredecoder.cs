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

namespace LogPreProcessor
{
    [Export("LogPreprocessor", typeof(ILogPreprocessor))]
    public class CdrPredecoder : ILogPreprocessor
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => this.GetType().ToString();
        public string HelpText => "Predecodes cdr rows as telcoright string[] in text file";
        public bool IsPrepared { get; set; }
        public object RuleConfigData { get; set; }

        public void PrepareRule()
        {
            throw new NotImplementedException();
        }

        public void Execute(Object input)
        {
            //Dictionary<string, object> dataAsDic = (Dictionary<string, object>) input;
            //TelcobrightConfig tbc = (TelcobrightConfig) dataAsDic["tbc"];
            //ne thisSwitch = (ne) dataAsDic["ne"];
            //string operatorName = tbc.Telcobrightpartner.CustomerName;
            //string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            //using (PartnerEntities context = new PartnerEntities(entityConStr))
            //{
            //    context.Database.Connection.Open();
            //    var mediationContext = new MediationContext(tbc, context);
            //    tbc.GetPathIndependentApplicationDirectory();
                
            //    List<job> incompleteJobs = GetReProcessJobs(context, ne, ne.DecodingSpanCount);
            //    incompleteJobs.AddRange(GetNewCdrJobs(tbc, context, ne, ne.DecodingSpanCount)); //combine
            //    using (DbCommand cmd = context.Database.Connection.CreateCommand())
            //    {
            //        foreach (job telcobrightJob in incompleteJobs)
            //        {
            //            Console.WriteLine("Processing CdrJob for Switch:" + ne.SwitchName + ", JobName:" +
            //                              telcobrightJob.JobName);
            //            this.TbConsole.WriteLine("Processing CdrJob for Switch:" + ne.SwitchName +
            //                                     ", JobName:" +
            //                                     telcobrightJob.JobName);
            //            try
            //            {
            //                if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
            //                cmd.ExecuteCommandText("set autocommit=0;");
            //                ITelcobrightJob iJob = null;
            //                mediationContext.MefJobContainer.DicExtensionsIdJobWise.TryGetValue(
            //                    telcobrightJob.idjobdefinition.ToString(), out iJob);
            //                if (iJob == null)
            //                    throw new Exception("JobRule not found in MEF collection.");
            //                var cdrJobInputData =
            //                    new CdrJobInputData(mediationContext, context, ne, telcobrightJob);
            //                iJob.Execute(cdrJobInputData); //EXECUTE
            //                cmd.ExecuteCommandText(" commit; ");
            //            }
            //            catch (Exception e)
            //            {
            //                try
            //                {
            //                    if (cmd.Connection.State != ConnectionState.Open) cmd.Connection.Open();
            //                    cmd.ExecuteCommandText(" rollback; ");
            //                    bool cacheLimitExceeded =
            //                        RateCacheCleaner.CheckAndClearRateCache(mediationContext, e);
            //                    if (cacheLimitExceeded) continue;
            //                    cacheLimitExceeded = RateCacheCleaner.ClearTempRateTable(e, cmd);
            //                    if (cacheLimitExceeded) continue;
            //                    PrintErrorMessageToConsole(ne, telcobrightJob, e);
            //                    ErrorWriter wr = new ErrorWriter(e, "ProcessCdr", telcobrightJob,
            //                        "CdrJob processing error.", tbc.Telcobrightpartner.CustomerName, context);
            //                    try
            //                    {
            //                        UpdateJobWithErrorInfo(cmd, telcobrightJob, e);
            //                    }
            //                    catch (Exception e2)
            //                    {
            //                        ErrorWriter wr2 = new ErrorWriter(e2, "ProcessCdr", telcobrightJob,
            //                            "Exception within catch block.",
            //                            tbc.Telcobrightpartner.CustomerName);
            //                    }
            //                    continue; //with next cdr or job
            //                }
            //                catch (Exception)
            //                {
            //                    //reaching here would be database problem
            //                    context.Database.Connection.Close();
            //                }
            //            } //end catch
            //        } //for each job
            //    } //using mysql command
            //}
        }
    }
}


