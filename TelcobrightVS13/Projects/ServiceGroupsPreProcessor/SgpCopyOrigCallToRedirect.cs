﻿using System.ComponentModel.Composition;
using System;
using MediationModel;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
using MediationModel.enums;
using Newtonsoft.Json;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Accounting.Invoice;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int, long>;

namespace TelcobrightMediation
{
    [Export("ServiceGroupPreProcessor", typeof(IServiceGroupPreProcessor))]
    public class SgpCopyOrigCallToRedirect : IServiceGroupPreProcessor
    {
        public override string ToString() => this.RuleName;
        public string RuleName => this.GetType().Name;
        public string HelpText => "Copy originating called number to redirecting number.";
        public int Id => 1;
        
        public void Execute(cdr thisCdr, CdrProcessor cdrProcessor)
        {
            thisCdr.RedirectingNumber = thisCdr.OriginatingCalledNumber;
        }
        
    }
}
