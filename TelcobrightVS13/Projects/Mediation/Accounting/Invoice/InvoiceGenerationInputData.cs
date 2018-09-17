﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class InvoiceGenerationInputData : ITelcobrightJobInput
    {
        public Dictionary<string, IInvoiceGenerationRule> InvoiceGenerationRules { get;}
        public Dictionary<int,IServiceGroup> ServiceGroups { get; }
        public Dictionary<string,IInvoiceSectionGenerator> InvoiceSectionGenerators { get; }
        public TelcobrightConfig Tbc { get; }
        public PartnerEntities Context { get; }
        public int BatchSizeForJobSegment { get; set; }
        public job TelcobrightJob { get; }
        public Dictionary<string,string> JsonDetail { get; set; }
        public Dictionary<int,InvoiceGenerationConfig> ServiceGroupWiseInvoiceGenerationConfigs { get; set; }
        public InvoiceGenerationInputData(TelcobrightConfig tbc,PartnerEntities context,job telcobrightJob,
            Dictionary<int,InvoiceGenerationConfig> serviceGroupWiseinvoiceGenerationConfigs,
            Dictionary<string, IInvoiceGenerationRule> invoiceGenerationRules,
            Dictionary<int, IServiceGroup> serviceGroups, 
            Dictionary<string, IInvoiceSectionGenerator> invoiceSectionGenerators)
        {
            this.Tbc = tbc;
            Context = context;
            BatchSizeForJobSegment = this.Tbc.CdrSetting.SegmentSizeForDbWrite;
            this.TelcobrightJob = telcobrightJob;
            this.InvoiceGenerationRules = invoiceGenerationRules;
            this.ServiceGroups = serviceGroups;
            this.ServiceGroupWiseInvoiceGenerationConfigs = serviceGroupWiseinvoiceGenerationConfigs;
            this.InvoiceSectionGenerators = invoiceSectionGenerators;
        }
    }
}
