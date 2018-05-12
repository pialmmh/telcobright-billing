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

    [Export("TelcobrightProcess", typeof(ITelcobrightProcess))]
    public class TpAutoCreateJob:ITelcobrightProcess 
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public void Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public string RuleName => this.GetType().ToString();
        public string HelpText => "method AutoCrateJob";
        public int ProcessId => 105;

        
    }
}
