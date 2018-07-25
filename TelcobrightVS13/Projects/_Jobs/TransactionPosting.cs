using TelcobrightMediation;
using Newtonsoft.Json;
using System.ComponentModel.Composition;
using System.IO;
using TelcobrightFileOperations;
using WinSCP;
using System;
using System.Linq;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Config;
using TelcobrightMediation.Accounting;

namespace Jobs
{

    [Export("Job", typeof(ITelcobrightJob))]
    public class TransactionPosting : ITelcobrightJob
    {
        public override string ToString() => this.RuleName;
        public string RuleName => "TransactionPosting";
        public string HelpText => "Posts a transaction to accounting context with automatic update of ledger summary";
        public int Id => 14;

        public JobCompletionStatus Execute(ITelcobrightJobInput jobInputData)
        {
            TransactionPostingJobInputData input = (TransactionPostingJobInputData) jobInputData;
            return JobCompletionStatus.Complete;
        } //execute
    }
}
