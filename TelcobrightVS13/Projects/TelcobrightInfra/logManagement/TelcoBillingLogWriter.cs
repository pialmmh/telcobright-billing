using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightInfra.logManagement
{
    public static class TelcobillingLogger
    {
        private static Object theLock = new Object();
        public static void LogMessageToFile(string logFileFullPath, string msg)
        {
            lock (theLock)
            {
                File.AppendAllText(logFileFullPath, msg);
            }
        }
    }
}
