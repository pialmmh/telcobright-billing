using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LibraryExtensions;
using TelcobrightInfra;
using TelcobrightInfra.logManagement;

namespace QuartzTelcobright
{
    public class TelcobrightHeartbeat
    {
        private string heartbeatLogtype;//"start" and "end"
        public string ProcessName { get; set; }
        public string HeartbeatMsg { get; set; }
        public string LogFileName { get; set; }
        public int HeartbeatSerialNo { get; set; }
        public int HeartbeatReturnTimeSeconds { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExpectedEndTime { get; set; }
        public DateTime? ActualEndTime { get; set; } = null;
        public int MinFrequencySeconds { get; set; }
        public DateTime ExpectedNextStartTime { get; set; }
        public override string ToString()
        {
            Func<DateTime?, string> convertNullableDateToStr = (date) => date == null ? null : Convert.ToDateTime(date).ToMySqlFormatWithoutQuote();
            return $"heartbeatLogtype:{this.heartbeatLogtype},ProcessName:{this.ProcessName},HeartbeatMsg:{this.HeartbeatMsg}" +
                $",HeartbeatSerialNo:{this.HeartbeatSerialNo},HeartbeatReturnTimeSeconds:{this.HeartbeatReturnTimeSeconds}" +
                $",StartTime:{this.StartTime.ToMySqlFormatWithoutQuote()}" +
                $",ExpectedEndTime:{this.ExpectedEndTime.ToMySqlFormatWithoutQuote()}" +
                $",ActualEndTime:{convertNullableDateToStr(this.ActualEndTime)}" +
                $",minFrequencySeconds:{this.MinFrequencySeconds}" +
                $",expectedNextStartTime:{this.ExpectedNextStartTime.ToMySqlFormatWithoutQuote()}";
        }
        public static TelcobrightHeartbeat deSerializeHeartbeat(string lineFromLogFile) {
            Dictionary<string, string> map = lineFromLogFile.Split(',').Select(kv => kv.Trim().Split(':')).Select(arr => new KeyValuePair<string, string>(arr[0], arr[1])).ToDictionary(kv => kv.Key, kv => kv.Value);
            TelcobrightHeartbeat heartbeat = new TelcobrightHeartbeat(
                    processName: map["ProcessName"],
                    heartbeatSerialNo: Convert.ToInt32(map["HeartbeatSerialNo"]),
                    minFrequencySeconds: Convert.ToInt32(map["minFrequencySeconds"]),
                    heartbeatMsg: map["HeartbeatMsg"],
                    heartbeatReturnTimeSeconds: Convert.ToInt32(map["HeartbeatReturnTimeSeconds"])
                );
            heartbeat.StartTime = map["startTime"].ConvertToDateTimeFromMySqlFormat();
            heartbeat.ExpectedEndTime = map["expectedEndTime"].ConvertToDateTimeFromMySqlFormat();
            heartbeat.ExpectedNextStartTime = map["expectedNextStartTime"].ConvertToDateTimeFromMySqlFormat();
            return heartbeat;
        }
        public TelcobrightHeartbeat(string processName, int heartbeatSerialNo, int minFrequencySeconds, string heartbeatMsg, int heartbeatReturnTimeSeconds) {
            var binPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binPath = binPath.Substring(6);
            this.ProcessName = processName;
            this.LogFileName = binPath + Path.DirectorySeparatorChar + "telcobright.log";
            this.HeartbeatSerialNo = heartbeatSerialNo;
            this.HeartbeatMsg = heartbeatMsg;
            this.HeartbeatReturnTimeSeconds = heartbeatReturnTimeSeconds;
            this.MinFrequencySeconds = minFrequencySeconds;
        }
        public void start() {
            this.heartbeatLogtype = "start";
            this.StartTime = DateTime.Now;
            this.ExpectedEndTime = this.StartTime.AddSeconds(this.HeartbeatReturnTimeSeconds);
            this.ExpectedNextStartTime = this.StartTime.AddSeconds(this.MinFrequencySeconds);
            TelcobillingLogger.LogMessageToFile(this.LogFileName, this.ToString());
        }
        public void end()
        {
            this.heartbeatLogtype = "end";//chanage logging type to end in log file, rest param remains same as during start
            this.ActualEndTime= DateTime.Now;
            //File.AppendAllLines(this.LogFileName, new string[] { this.ToString() });
            TelcobillingLogger.LogMessageToFile(this.LogFileName, this.ToString());
        }
    }
}
