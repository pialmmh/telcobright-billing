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
    public class GenericDisposable<T> : IDisposable
    {
        public Action Dispose { get; set; }
        public T Object { get; set; }
        void IDisposable.Dispose()
        {
            Dispose();
        }

    }
    public class User : MarshalByRefObject
    {
        public void Sayhello()
        {
            Console.WriteLine("Hello from User");
        }
    }


    public class ProcessWrapper
    {
        public string instanceName;
        //private Telcobright2 telcobright;
        private Thread thread;

        Process process;
        public int processId;

        public static GenericDisposable<T> CreateDomainWithType<T>()
        {
            var appDomain = AppDomain.CreateDomain(@"apple");
            var inst = appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
            appDomain.DomainUnload += (a, b) => Console.WriteLine("Unloaded");
            return new GenericDisposable<T>() { Dispose = () => AppDomain.Unload(appDomain), Object = (T)inst };
        }
        public ProcessWrapper(string instanceName, Action<string> callbackFromUI)
        {
            string appName = instanceName+"_cas";
            instanceName = ConfigurationManager.AppSettings["configRoot"] + "\\" + instanceName + "_cas" + "\\" + instanceName + "_cas.conf";
            this.instanceName = instanceName;

          
            ProcessStartInfo processStartInfo;
            StringBuilder outputBuilder;
            outputBuilder = new StringBuilder();
            string fileName = @"D:\TelcobrightProject\TelcobrightVS13\Projects\WS_Topshelf_Quartz\bin\Debug\WS_Telcobright_Topshelf.exe";

            processStartInfo = new ProcessStartInfo(fileName);
            processStartInfo.CreateNoWindow = true;
            //processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments =$@"{appName}";
            

            process = new Process();
          
           
           
            process.StartInfo = processStartInfo;
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += new DataReceivedEventHandler
            (
                delegate (object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    outputBuilder.Append(e.Data);
                    callbackFromUI(outputBuilder.ToString());

                }
            );
            process.Start();
            process.BeginOutputReadLine();
            this.processId = process.Id;

            //Thread.Sleep(5000);  
            System.Diagnostics.Process procs = System.Diagnostics.Process.GetProcessById(this.processId, ".");
            procs.CloseMainWindow();// use "." for this machine
            //foreach (var proc in procs)
            //{
            //    proc.CloseMainWindow();
            //}







            //AppDomain newAppDomain = AppDomain.CreateDomain("NewApplicationDomain");
            //newAppDomain.ExecuteAssembly(fileName, new[] { "mothertelecom_cas" });
            //Thread.Sleep(10000);
            //AppDomain.Unload(newAppDomain);

            //using (var wrap = CreateDomainWithType<WS_Telcobright_Topshelf.Telcobright2>())
            //{
            //    Thread th = new Thread(() =>
            //    {
            //        wrap.Object.run(false);
            //    });
            //    th.Start();
            //    Thread.Sleep(30000);
            //    th.Abort();
            //    wrap.Object.isActive = false;
            //    wrap.Dispose();
            //}



            //this.thread = new Thread(() =>
            //{
            //    bool isConsoleApp = false;
            //    var telcobright = new Telcobright2(instanceName, callbackFromUI);
            //    telcobright.run(isConsoleApp);

            //});
        }

        public void start()
        {
            try
            {
                //this.thread.Start();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        //[SecurityPermission(SecurityAction.InheritanceDemand, ControlThread = true)]
        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void stop()
        {
            try
            {


                System.Diagnostics.Process procs = System.Diagnostics.Process.GetProcessById(this.processId, "."); // use "." for this machine
                procs.Kill();
           
           
            }
            catch (ThreadAbortException e)
            {
                Thread.ResetAbort();
                Console.WriteLine(e.StackTrace);
            }
        }



    }
}
