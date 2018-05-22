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
    public class MockNewCdrFileJob : NewCdrFileJob
    {
        public IEventCollector EventCollector { get; set; }
        public string OperatorName { get; set; }
        public MockNewCdrFileJob(IEventCollector eventCollector, string operatorName)
        {
            this.EventCollector = eventCollector;
            this.OperatorName = operatorName;
        }
        private string CdrLocation => @"C:\telcobright\Vault\Resources\CDR";
        public override JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            base.Input = (CdrJobInputData) jobInputData;
            NewCdrPreProcessor preProcessor = this.CollectRaw();
            base.PreformatRawCdrs(preProcessor);
            preProcessor.TxtCdrRows.ForEach(txtRow => this.CdrConverter(preProcessor, txtRow));

            CdrCollectionResult newCollectionResult, oldCollectionResult = null;
            //todo: remove temp code
            var partialDuration = preProcessor.TxtCdrRows.Where(c => c[Fn.Partialflag].ValueIn(new[] {"1", "2", "3"}))
                .Sum(c => Convert.ToDecimal(c[Fn.DurationSec]));
            //end

            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);

            //CdrJob cdrJob = base.CreateCdrJob(preProcessor, newCollectionResult, oldCollectionResult);
            PartialCdrTesterData partialCdrTesterData = OrganizeTestDataForPartialCdrs(preProcessor, newCollectionResult);
            CdrJob cdrJob = (new CdrJobFactory(this.Input, this.RawCount)).
                CreateCdrJob(preProcessor, newCollectionResult, oldCollectionResult, partialCdrTesterData);
            ExecuteCdrJob(cdrJob);
            return JobCompletionStatus.Complete;
        }

        protected override NewCdrPreProcessor CollectRaw()
        {
            this.CollectorInput = new CdrCollectorInputData(base.Input,
                $@"{this.CdrLocation}\{this.OperatorName}\{base.Input.Ne.SwitchName}\"
                + base.Input.TelcobrightJob.JobName);

            //base.CollectorInput = new CdrCollectorInputData(base.Input,);
            FileBasedTextCdrCollector fileBasedNewCdrCollector =
                (FileBasedTextCdrCollector) this.EventCollector;
            fileBasedNewCdrCollector.CollectorInput = this.CollectorInput;
            return (NewCdrPreProcessor) this.EventCollector.Collect();
        }

        protected void WriteJobCompletionIfCollectionNotEmpty(CdrJob cdrJob, job telcobrightJob)
        {
            base.WriteJobCompletionIfCollectionNotEmpty(cdrJob.CdrProcessor,telcobrightJob);
        }

        protected void WriteJobCompletionIfCollectionIsEmpty(CdrJob cdrJob, job telcobrightJob)
        {
            base.WriteJobCompletionIfCollectionIsEmpty(cdrJob.CdrProcessor, telcobrightJob);
        }
    }
}
