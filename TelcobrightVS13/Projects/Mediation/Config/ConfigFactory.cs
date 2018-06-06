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
            //prev deserializer
            //using (StreamReader file = File.OpenText(configFileName))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    return (TelcobrightConfig) serializer.Deserialize(file, typeof(TelcobrightConfig));
            //}
            //end prev
            string json = File.ReadAllText(configFileName);
            var obj = JsonConvert.DeserializeObject<TelcobrightConfig>(json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                });
            return (TelcobrightConfig) obj;
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
