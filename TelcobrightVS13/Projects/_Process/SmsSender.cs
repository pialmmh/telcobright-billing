using TelcobrightMediation;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using MediationModel;
using Quartz;
using QuartzTelcobright;
using TelcobrightMediation.Config;
using System.Net;


namespace Process
{
    [DisallowConcurrentExecution]
    [Export("TelcobrightProcess", typeof(AbstractTelcobrightProcess))]
    public class SmsSender
         : AbstractTelcobrightProcess
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public override string RuleName => this.GetType().ToString();
        public override string HelpText => "Sms sender";
        public override int ProcessId => 102;

        public override void Execute(IJobExecutionContext schedulerContext)
        {

            string operatorName = schedulerContext.JobDetail.JobDataMap.GetString("operatorName");
            TelcobrightConfig tbc = ConfigFactory.GetConfigFromSchedulerExecutionContext(
                schedulerContext, operatorName);
            string entityConStr = ConnectionManager.GetEntityConnectionStringByOperator(operatorName, tbc);
            PartnerEntities context = new PartnerEntities(entityConStr);

            string lastProcessedCdrTime = context.jobs
                .Where(j => j.idjobdefinition == 1 && j.Status == 1)
                .OrderByDescending(j => j.CompletionTime)
                .Select(j => j.CompletionTime)
                .FirstOrDefault()?.ToString();

            bool stoppedSinceOneHour = !((DateTime.Now - Convert.ToDateTime(lastProcessedCdrTime)).TotalMinutes < 60);

            //bool stoppedSinceOneHour = (DateTime.Now - Convert.ToDateTime(lastProcessedCdrTime)).TotalMinutes < 60 ? false : true;

            if (stoppedSinceOneHour)
            {
                try
                {
                    foreach (string desNumber in tbc.SmsSenderConfig.DestinationNumber)
                    {
                        string queryString = $"?username={tbc.SmsSenderConfig.Username}&password={tbc.SmsSenderConfig.Password}&type=0&dlr=1&destination={desNumber}&source={tbc.SmsSenderConfig.Source}&message={tbc.SmsSenderConfig.Message}";

                        using (WebClient client = new WebClient())
                        {
                            string response = client.DownloadString(tbc.SmsSenderConfig.ApiUrl + queryString);
                            Console.WriteLine(response);
                        }
                        Console.WriteLine($"SMS Sent to respective {desNumber} number!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send SMS. Error: {ex.Message}");
                }
            }
        }
    }
}