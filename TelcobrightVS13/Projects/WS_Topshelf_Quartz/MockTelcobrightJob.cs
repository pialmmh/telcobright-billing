using System;
using Quartz;
using QuartzTelcobright;

namespace WS_Telcobright_Topshelf
{
    public class MockTelcobrightJob : ITelcobrightProcess
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Executing Mock Service [id=101], time: " + DateTime.Now);
        }
        public string HelpText => "Mock Telcobright Process";
        public int ProcessId => 101;
    }
}