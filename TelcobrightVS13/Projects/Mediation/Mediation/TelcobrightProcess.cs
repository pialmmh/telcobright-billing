using Newtonsoft.Json;
using System;
using System.IO;
using MediationModel;
using QuartzTelcobright;

namespace TelcobrightMediation
{
    public class TelcobrightProcess
    {

        public ProcessParamater ProcessParameter { get; set; }
        public process Process { get; set; }
        public TelcoBrightAdhocMethods Methods { get; set; }
        public ITelcobrightProcess MefProcess { get;}
        public TelcobrightProcess(process pProcess,ITelcobrightProcess mefProcess)
        {
            try
            {
                this.Methods = new TelcoBrightAdhocMethods();
                this.Process = pProcess;
                this.MefProcess = mefProcess;
                this.ProcessParameter = JsonConvert.DeserializeObject<ProcessParamater>(this.Process.ProcessParamaterJson);
                }
            catch (Exception e1)
            {
                Console.WriteLine(e1);
                var logFileName = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "telcobright.log";
                File.WriteAllText(logFileName, e1.Message + Environment.NewLine + (e1.InnerException != null ? e1.InnerException.ToString() : ""));
            }
        }

        
    }
}
