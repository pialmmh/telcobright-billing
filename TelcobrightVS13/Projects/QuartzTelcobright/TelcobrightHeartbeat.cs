using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LibraryExtensions;
namespace QuartzTelcobright
{
    public class TelcobrightHeartbeat
    {
        public string ProcessName { get; }
        public string HeartbeatMsg { get; }
        public string LogFileName { get; }
        public int HeartbeatId { get; }
        public TelcobrightHeartbeat(string processName, int heartbeatId, string heartbeatMsg) {
            var binPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binPath = binPath.Substring(6);
            this.ProcessName = processName;
            this.LogFileName = binPath + Path.DirectorySeparatorChar + "telcobright.log";
            this.HeartbeatId = heartbeatId;
            this.HeartbeatMsg = heartbeatMsg;
        }
        public void start() {
            File.AppendAllLines(this.LogFileName, new string[] { $@"Process:{this.ProcessName},heartbeatId:{this.HeartbeatId},heartbeatStart:{DateTime.Now.ToMySqlFormatWithoutQuote()},msg:{this.HeartbeatMsg}" });
        }
        public void end()
        {
            File.AppendAllLines(this.LogFileName, new string[] { $@"Process:{this.ProcessName},heartbeatId:{this.HeartbeatId},heartbeatEnd:{DateTime.Now.ToMySqlFormatWithoutQuote()},msg:{this.HeartbeatMsg}" });
        }
    }
}
