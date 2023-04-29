using TelcobrightMediation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TelcobrightFileOperations;
using MediationModel;
using Quartz;
using QuartzTelcobright;

namespace Process
{

    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class TpAutoCreateJob: AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public override void Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "method AutoCrateJob";
        public override int ProcessId => 105;
    }
}
