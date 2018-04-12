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
            using (StreamReader file = File.OpenText(configFileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (TelcobrightConfig) serializer.Deserialize(file, typeof(TelcobrightConfig));
            }
        }

        public static TelcobrightConfig GetConfigFromSchedulerExecutionContext(IJobExecutionContext context,string operatorName)
        {
            return ((Dictionary<string,TelcobrightConfig>)context.Scheduler.Context.Get("configs"))[operatorName];
        }
        public static TelcobrightConfig GetConfigFromDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
