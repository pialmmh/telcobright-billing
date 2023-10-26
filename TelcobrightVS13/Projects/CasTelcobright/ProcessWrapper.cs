using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CasTelcobright.Forms;
using LibraryExtensions;
using WS_Telcobright_Topshelf;
using System.Configuration;
using System.Diagnostics;
using System.Security.Permissions;
using TelcobrightInfra;



namespace CasTelcobright
{

    public class ProcessWrapper
    {
        public string instanceName;
        //private Telcobright2 telcobright;
        private Thread thread;

        Process process;
        public int processId;
        public string appName;
        private Action<string> callbackFromUI;

        public ProcessWrapper(string instanceName, Action<string> callbackFromUI)
        {
            this.appName = instanceName+"_cas";
            instanceName = ConfigurationManager.AppSettings["configRoot"] + "\\" + instanceName + "_cas" + "\\" + instanceName + "_cas.conf";
            this.instanceName = instanceName;
            this.callbackFromUI = callbackFromUI;
        }

        public void start()
        {
            try
            {
              
                ProcessStartInfo processStartInfo;
                StringBuilder outputBuilder;
                outputBuilder = new StringBuilder();
                string fileName = @"D:\TelcobrightProject\TelcobrightVS13\Projects\WS_Topshelf_Quartz\bin\Debug\WS_Telcobright_Topshelf.exe";
            
                processStartInfo = getProcessStartInfo(fileName);

                process.OutputDataReceived += ProcessOnOutputDataReceived();
                process.Start();
                process.BeginOutputReadLine();
                this.processId = process.Id;

                Thread.Sleep(500);  
                System.Diagnostics.Process procs = System.Diagnostics.Process.GetProcessById(this.processId, ".");// use "." for this machine
                procs.CloseMainWindow();


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private DataReceivedEventHandler ProcessOnOutputDataReceived()
        {
            return new DataReceivedEventHandler
            (
                delegate (object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        this.callbackFromUI(e.Data);
                    }
                } 
            );
        }

        private ProcessStartInfo getProcessStartInfo(string fileName)
        {
            ProcessStartInfo processStartInfo;
            processStartInfo = new ProcessStartInfo(fileName);
            processStartInfo.CreateNoWindow = true;
            //processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = $@"{this.appName}";
            process = new Process();
            process.StartInfo = processStartInfo;
            process.EnableRaisingEvents = true;
            return processStartInfo;
        }

        //[SecurityPermission(SecurityAction.InheritanceDemand, ControlThread = true)]
        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void stop()
        {
            try
            {
                if(!this.process.HasExited)
                {
                    System.Diagnostics.Process procs = System.Diagnostics.Process.GetProcessById(this.processId, "."); // use "." for this machine
                    procs.Kill();
                }
                   
           
            }
            catch (Exception e)
            {
                Thread.ResetAbort();
                Console.WriteLine(e.StackTrace);
            }
        }



    }
}
