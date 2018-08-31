﻿using TelcobrightMediation;
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
using TelcobrightMediation.Config;

namespace Process
{
    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class AutomationHandler : ITelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public string RuleName => this.GetType().ToString();
        public string HelpText => "";
        public int ProcessId => 118;
        public void Execute(IJobExecutionContext schedulerContext)
        {
            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            try
            {
                TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                    schedulerContext, operatorName);
                string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName);
                using (PartnerEntities context = new PartnerEntities(entityConStr))
                {
                    context.Database.Connection.Open();
                    var mediationContext = new MediationContext(tbc, context);
                    
                }
            } //try
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                ErrorWriter wr = new ErrorWriter(e1, "AccountingHelper", null, "", operatorName);
            }
        }

        bool CheckIncomplete(PartnerEntities context, MediationContext mediationContext)
        {
            List<int> idJobDefs = context.enumjobdefinitions.Where(c => c.JobQueue == this.ProcessId).Select(c => c.id)
                .ToList();
            return context.jobs.Any(c => c.CompletionTime == null && idJobDefs.Contains(c.idjobdefinition));
        }
    }
}


