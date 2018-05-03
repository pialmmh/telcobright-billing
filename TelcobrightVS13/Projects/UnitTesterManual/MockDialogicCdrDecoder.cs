using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decoders;
using MediationModel;
using TelcobrightFileOperations;
using TelcobrightMediation;
using TelcobrightMediation.Cdr;

namespace UnitTesterManual
{
    public class MockDialogicCdrDecoder:DialogicControlSwitchDecoder
    {
        private CdrCollectorInputData CollectorInput { get; }
        private PartnerEntities Context { get; }
        public override string RuleName => "decDialogicCS";

        public MockDialogicCdrDecoder(CdrCollectorInputData collectorInput,PartnerEntities context)
        {
            this.CollectorInput = collectorInput;
            this.Context = context;
        }

        protected override List<string[]> GetTxtCdrs()
        {
            //RawTextCdrCollectorFromDb rawCdrCollector = new RawTextCdrCollectorFromDb(this.CollectorInput, "mockcdr",
            //    "dialogiccdr",
            //    this.CollectorInput.TelcobrightJob.JobName, this.Context);
            //List<string[]> txtRows = rawCdrCollector.CollectRawCdrsFromDb();
            //txtRows.ForEach(r => r[Fn.Partialflag] = "1");//enable partial flag for testing
            //return txtRows;
            return null;
        }
    }
}
