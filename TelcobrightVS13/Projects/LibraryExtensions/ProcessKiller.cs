using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryExtensions
{
    public class ProcessKiller
    {
        public static void ForceKill(Process proc)
        {

            // Accessing ProcessName could throw an exception if the process has already been killed.
            string processName = string.Empty;
            try { processName = proc.ProcessName; } catch (Exception ex) { }

            // ProcessId can be accessed after the process has been killed but we'll do this safely anyways.
            int pId = 0;
            try { pId = proc.Id; } catch (Exception ex) { }

            // Will only work if started by this instance of the dll.
            try {
                proc.Kill();
                return;
            } catch (Exception ex) { }

            // Fallback to task kill
            if (pId > 0)
            {
                try
                {
                    var taskKilPsi = new ProcessStartInfo("taskkill");
                    taskKilPsi.Arguments = $"/pid {proc.Id} /T /F";
                    taskKilPsi.WindowStyle = ProcessWindowStyle.Hidden;
                    taskKilPsi.UseShellExecute = false;
                    taskKilPsi.RedirectStandardOutput = true;
                    taskKilPsi.RedirectStandardError = true;
                    taskKilPsi.CreateNoWindow = true;
                    var taskKillProc = Process.Start(taskKilPsi);
                    taskKillProc.WaitForExit();
                    String taskKillOutput = taskKillProc.StandardOutput.ReadToEnd(); // Contains success
                    String taskKillErrorOutput = taskKillProc.StandardError.ReadToEnd();
                    return;
                }
                catch (Exception e)
                {
                }
            }

            // Fallback to wmic delete process.
            if (!string.IsNullOrEmpty(processName))
            {
                // https://stackoverflow.com/a/38757852/591285
                try
                {
                    var wmicPsi = new ProcessStartInfo("wmic");
                    wmicPsi.Arguments = $@"process where ""name='{processName}.exe'"" delete";
                    wmicPsi.WindowStyle = ProcessWindowStyle.Hidden;
                    wmicPsi.UseShellExecute = false;
                    wmicPsi.RedirectStandardOutput = true;
                    wmicPsi.RedirectStandardError = true;
                    wmicPsi.CreateNoWindow = true;
                    var wmicProc = Process.Start(wmicPsi);
                    wmicProc.WaitForExit();
                    String wmicOutput = wmicProc.StandardOutput.ReadToEnd(); // Contains success
                    String wmicErrorOutput = wmicProc.StandardError.ReadToEnd();
                    return;
                }
                catch (Exception e)
                {
                }
            }

        }
    }
}
