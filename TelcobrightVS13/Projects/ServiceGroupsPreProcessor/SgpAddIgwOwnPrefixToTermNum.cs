using System.ComponentModel.Composition;
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
    public class SgpAddIgwOwnPrefixToTermNum : IServiceGroupPreProcessor
    {
        public override string ToString() => this.RuleName;
        public string RuleName => this.GetType().Name;
        public string HelpText => "Add Mir(igw) own prefix to  termination called number.";
        public int Id => 2;
        
        public void Execute(cdr thisCdr, CdrProcessor cdrProcessor)
        {
            if(thisCdr.TerminatingCalledNumber.StartsWith("00") ||  
               thisCdr.TerminatingCalledNumber.StartsWith("0013") || thisCdr.SwitchId == 11)
                return;
            thisCdr.TerminatingCalledNumber = "0013" + thisCdr.TerminatingCalledNumber;
        }
        
    }
}
