using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using TelcobrightMediation.Scheduler.Quartz;
using System.IO;
using System.Threading;
using LibraryExtensions;
using QuartzTelcobright.MefComposers;
using Spring.Context;
using Spring.Context.Support;
using System.Linq;
namespace QuartzTelcobright
{
    //[PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class QuartzTelcobrightProcessWrapper : IJob
    {
        public ITelcobrightProcess Process { get; set; }
        public void Execute(IJobExecutionContext context)
        {
            ITelcobrightProcess process = null;
            JobDataMap jobDataMap = context.JobDetail.JobDataMap;
            string identity = jobDataMap.GetString("identity");
            int telcobrightProcessId = Convert.ToInt32(jobDataMap.GetString("telcobrightProcessId"));
            MefProcessContainer mefProcessContainer = null;
            mefProcessContainer = (MefProcessContainer)context.Scheduler.Context.Get("processes");
            var binPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binPath = binPath.Substring(6);
            var processLogDir = binPath + Path.DirectorySeparatorChar + "processLogs";
            if (!Directory.Exists(processLogDir))
            {
                Directory.CreateDirectory(processLogDir);
            }
            else
            {
                var processDirs = Directory.GetDirectories(processLogDir).ToList();
                //clean log files from last run
                foreach (string processDir in processDirs)
                {
                    cleanProcessDir(processDir);
                }
            }
            context.Put("processLogDir", processLogDir);
            Action run = () =>
            {
                process = null;
                process = mefProcessContainer.Processes[telcobrightProcessId.ToString()];
                Console.WriteLine("Executing Process=> Id= " + process.ProcessId + ", Identity=" + identity + " Time: " + DateTime.Now.ToMySqlField());
                process.Execute(context);
                Console.WriteLine("Finished Process=> Id= " + process.ProcessId + ", Identity=" + identity + " Time: " + DateTime.Now.ToMySqlField());
            };
            try
            {
                run();
            }
            catch (Exception e) {
                Console.WriteLine("Exception in quartz process id=" + process.ProcessId + " , name="+process.HelpText);
                run();  
            }
        }

        private static void cleanProcessDir(string processDir)
        {
            DirectoryInfo di = new DirectoryInfo(processDir);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
#if DEBUG
        void TestLoop(JobDataMap jobDataMap)
        {
            Console.WriteLine("Running "
                              + jobDataMap.GetString("identity") + ", processid: "
                              + jobDataMap.GetString("telcobrightProcessId") + " at " + DateTime.Now);
            Console.WriteLine("going to sleep...");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("step " + (i + 1).ToString());
                Thread.Sleep(2000);
            }
        }
        #endif

    }
}
