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
        public int HeartbeatSerialNo { get; }
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public TelcobrightHeartbeat() {
            var binPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            binPath = binPath.Substring(6);
            this.LogFileName = binPath + Path.DirectorySeparatorChar + "telcobright.log";
        }
        public void start() {
            File.AppendAllLines(this.LogFileName, new string[] { $@"Process:{this.ProcessName},heartbeatSerialNo:{this.HeartbeatSerialNo},heartbeatStart:{DateTime.Now.ToMySqlFormatWithoutQuote()},msg:{this.HeartbeatMsg}" });
        }
        public void end()
        {
            File.AppendAllLines(this.LogFileName, new string[] { $@"Process:{this.ProcessName},heartbeatSerialNo:{this.HeartbeatSerialNo},heartbeatEnd:{DateTime.Now.ToMySqlFormatWithoutQuote()},msg:{this.HeartbeatMsg}" });
        }
    }
}
