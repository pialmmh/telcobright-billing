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
        public IFileDecoder CdrDecoder { get; set; }
        public string OperatorName { get; set; }

        public MockNewCdrFileJob(IFileDecoder cdrDecoder, string operatorName)
        {
            this.CdrDecoder = cdrDecoder;
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
            preProcessor.GetCollectionResults(out newCollectionResult, out oldCollectionResult);

            CdrJob cdrJob = base.PrepareCdrJob(preProcessor, newCollectionResult, oldCollectionResult);
            ExecuteCdrJob(cdrJob);
            return JobCompletionStatus.Complete;
        }
        protected override NewCdrPreProcessor CollectRaw()
        {
            Vault vault = base.Input.MediationContext.Tbc.DirectorySettings.Vaults.First(
                c => c.Name == base.Input.TelcobrightJob.ne.SourceFileLocations);
            FileLocation fileLocation = vault.LocalLocation.FileLocation;
            string fileName = fileLocation.GetOsNormalizedPath(fileLocation.StartingPath)
                              + Path.DirectorySeparatorChar + base.Input.TelcobrightJob.JobName;
            base.CollectorInput = new CdrCollectorInputData(base.Input, fileName);
            base.CollectorInput.FullPath =
                $@"{this.CdrLocation}\{this.OperatorName}\{base.CollectorInput.Ne.SwitchName}\"
                + base.Input.TelcobrightJob.JobName;
            List<cdrinconsistent> inconsistentCdrs;
            List<string[]> decodedCdrRows = this.CdrDecoder.DecodeFile(base.CollectorInput, out inconsistentCdrs);
            return new NewCdrPreProcessor(decodedCdrRows, inconsistentCdrs, base.CollectorInput);
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
