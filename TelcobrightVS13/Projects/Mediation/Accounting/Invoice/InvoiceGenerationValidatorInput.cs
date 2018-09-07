using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationValidatorInput 
    {
        public PartnerEntities Context { get; }
        public job TelcobrightJob { get; }
        public InvoiceGenerationValidatorInput(PartnerEntities context, job telcobrightJob)
        {
            this.Context = context;
            this.TelcobrightJob = telcobrightJob;
        }
    }
}
