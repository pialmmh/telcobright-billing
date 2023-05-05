using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using System.IO;
using LibraryExtensions;
namespace QuartzTelcobright
{
    public abstract class AbstractTelcobrightProcess : ITelcobrightProcess
    {
        public abstract string RuleName { get; }
        public abstract string HelpText { get; }
        public abstract int ProcessId { get; }
        public abstract void Execute(IJobExecutionContext context);
        //public static void updateHeartbeat(IJobExecutionContext context, string heartbitMsg) {}
    }
}
