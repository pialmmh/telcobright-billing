using System;
using System.IO;
using Newtonsoft.Json;
using Quartz;

using System.Collections.Generic;
namespace TelcobrightMediation.Config
{
    public static class ConfigFactory
    {
        public static TelcobrightConfig GetConfigFromFile(string configFileName)
        {
            if (string.IsNullOrEmpty(configFileName)) 
            {
                throw new Exception("Config file name is missing.");
            }
            string json = File.ReadAllText(configFileName);
            var obj = JsonConvert.DeserializeObject<TelcobrightConfig>(json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,5
                });
            return (TelcobrightConfig) obj;
        }

        public static TelcobrightConfig GetConfigFromSchedulerExecutionContext(IJobExecutionContext context,string operatorName)
        {
            return (TelcobrightConfig)context.Scheduler.Context.Get("configs");
        }
        public static TelcobrightConfig GetConfigFromDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
