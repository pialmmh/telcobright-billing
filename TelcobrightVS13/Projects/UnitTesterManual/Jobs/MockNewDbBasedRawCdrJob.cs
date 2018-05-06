using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.IO;
using TelcobrightFileOperations;
using System.Linq;
using System.Text;
using MediationModel;
using System.Threading.Tasks;
using Decoders;
using FlexValidation;
using LibraryExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
using TelcobrightMediation.Config;
using Jobs;
using TelcobrightMediation.Mediation.Cdr;

namespace UnitTesterManual
{
    [Export("Job", typeof(ITelcobrightJob))]
    public class MockNewDbBasedRawCdrJob : MockNewCdrFileJob
    {
        public MockNewDbBasedRawCdrJob(IEventCollector eventCollector, string operatorName):
            base(eventCollector,operatorName)
        {
            this.EventCollector = eventCollector;
            this.OperatorName = operatorName;
        }
        private string CdrLocation => @"C:\telcobright\Vault\Resources\CDR";
        protected override NewCdrPreProcessor CollectRaw()
        {
            this.CollectorInput = new CdrCollectorInputData(base.Input,
                $@"{this.CdrLocation}\{this.OperatorName}\{base.Input.Ne.SwitchName}\"
                + base.Input.TelcobrightJob.JobName);

            //base.CollectorInput = new CdrCollectorInputData(base.Input,);
            this.EventCollector.Params = new Dictionary<string, object>()
            {
                {
                    "fileName",
                    base.Input.TelcobrightJob.JobName
                },
                {
                    "collectorInput",
                    new CdrCollectorInputData(base.Input, "")
                }
            };
            return (NewCdrPreProcessor) this.EventCollector.Collect();
        }
    }
}
