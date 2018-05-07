using System;
using System.Collections.Generic;
using TelcobrightMediation;
using System.ComponentModel.Composition;
using System.Data.Common;
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
    public class MockCdrDecoderDumperJob : NewCdrFileJob
    {
        public IFileDecoder CdrDecoder { get; set; }
        public string OperatorName { get; set; }

        public MockCdrDecoderDumperJob(IFileDecoder cdrDecoder, string operatorName)
        {
            this.CdrDecoder = cdrDecoder;
            this.OperatorName = operatorName;
        }

        public override JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            CdrJobInputData input = (CdrJobInputData) jobInputData;
            CdrCollectorInputData collectorInput =
                new CdrCollectorInputData(input, input.TelcobrightJob.JobName);
            collectorInput.FullPath =
                $@"C:\telcobright\Vault\Resources\CDR\{this.OperatorName}\{collectorInput.Ne.SwitchName}\"
                + input.TelcobrightJob.JobName;
            List<cdrinconsistent> inconsistentCdrs;
            List<string[]> decodedCdrRows = this.CdrDecoder.DecodeFile(collectorInput, out inconsistentCdrs);
            //NewCdrPreProcessor preProcessor =
              //  new NewCdrPreProcessor(decodedCdrRows, inconsistentCdrs, collectorInput);
            //base.PrepareDecodedRawCdrs(preProcessor, collectorInput);
            if (inconsistentCdrs.Any())
            {
                throw new Exception("Inconsistent cdrs are temporarily disallowed.");
            }
            string fileName = input.TelcobrightJob.JobName;
            string switchId = input.Ne.idSwitch.ToString();
            decodedCdrRows.ForEach(r =>
            {
                r[Fn.Filename] = fileName;
                r[Fn.Switchid]=switchId.ToString();
            });
            
            using (DbCommand cmd = ConnectionManager.CreateCommandFromDbContext(input.Context))
            {
                cmd.CommandText = $@"delete from mockcdr where fileserialno={fileName.EncloseWithSingleQuotes()}";
                cmd.ExecuteNonQuery();
                StringBuilder sb = new StringBuilder("insert into mockcdr values ")
                    .Append(String.Join(",",
                        decodedCdrRows
                            .Select(row => "(" + string.Join(",", row.Select(fld => fld.EncloseWithSingleQuotes())) +
                                           ")").ToList()));
                cmd.CommandText = sb.ToString();
                cmd.ExecuteNonQuery();

                cmd.ExecuteCommandText("commit;");
            }
            return JobCompletionStatus.Complete;
        }
    }
}
